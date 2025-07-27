using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Contracts;

namespace TechWayFit.Licensing.Core.Services
{
    /// <summary>
    /// Service for validating tamper-proof licenses using RSA digital signatures
    /// Moved from separate validation project to consolidate validation logic
    /// </summary>
    public class LicenseValidationService : ILicenseValidationService
    {
        private readonly ILogger<LicenseValidationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IKeyRepository _keyRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly LicenseValidationOptions _defaultOptions;

        public LicenseValidationService(
            ILogger<LicenseValidationService> logger,
            IConfiguration configuration,
            IMemoryCache cache,
            IKeyRepository keyRepository,
            IAuditRepository auditRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            _keyRepository = keyRepository;
            _auditRepository = auditRepository;
            
            _defaultOptions = new LicenseValidationOptions();
            configuration.GetSection("LicenseValidation").Bind(_defaultOptions);
        }

        public async Task<LicenseValidationResult> ValidateAsync(SignedLicense signedLicense, LicenseValidationOptions? options = null)
        {
            var validationOptions = options ?? _defaultOptions;
            var cacheKey = $"license_validation_{signedLicense.PublicKeyThumbprint}_{signedLicense.Checksum}";

            try
            {
                // Check cache first if enabled
                if (validationOptions.EnableCaching && _cache.TryGetValue(cacheKey, out LicenseValidationResult cachedResult))
                {
                    _logger.LogDebug("License validation result retrieved from cache");
                    return cachedResult;
                }

                _logger.LogInformation("Validating license with format version {FormatVersion}", signedLicense.FormatVersion);

                // Decode and deserialize license data first
                var license = await DecodeLicenseDataAsync(signedLicense);
                if (license == null)
                {
                    var result = LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License data is corrupted or unreadable");
                    await LogValidationAttempt(result);
                    return result;
                }

                // Get public key for this product
                var publicKey = await _keyRepository.GetPublicKeyAsync(license.ProductId);
                if (publicKey == null)
                {
                    var result = LicenseValidationResult.Failure(LicenseStatus.Invalid, $"Public key not found for product {license.ProductId}");
                    await LogValidationAttempt(result);
                    return result;
                }

                // Validate signature if enabled
                if (validationOptions.ValidateSignature)
                {
                    var signatureValid = await ValidateSignatureAsync(signedLicense, publicKey);
                    if (!signatureValid)
                    {
                        var result = LicenseValidationResult.Failure(LicenseStatus.Invalid, "License signature validation failed");
                        await LogValidationAttempt(result);
                        return result;
                    }
                }

                // Validate dates if enabled
                if (validationOptions.ValidateDates)
                {
                    var dateValidation = ValidateLicenseDates(license, validationOptions);
                    if (!dateValidation.IsValid)
                    {
                        await LogValidationAttempt(dateValidation);
                        if (validationOptions.EnableCaching)
                        {
                            _cache.Set(cacheKey, dateValidation, TimeSpan.FromMinutes(validationOptions.CacheDurationMinutes));
                        }
                        return dateValidation;
                    }
                }

                // Create successful validation result
                var successResult = LicenseValidationResult.Success(license);
                successResult.IsSignatureValid = true;
                successResult.AreDatesValid = true;

                // Populate available features
                successResult.AvailableFeatures = license.FeaturesIncluded
                    .Where(f => f.IsCurrentlyValid)
                    .Select(f => f.Name)
                    .ToList();

                // Cache result if enabled
                if (validationOptions.EnableCaching)
                {
                    _cache.Set(cacheKey, successResult, TimeSpan.FromMinutes(validationOptions.CacheDurationMinutes));
                }

                await LogValidationAttempt(successResult);
                return successResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "License validation failed with exception");
                var errorResult = LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, 
                    $"License validation service encountered an error: {ex.Message}");
                await LogValidationAttempt(errorResult);
                return errorResult;
            }
        }

        public async Task<LicenseValidationResult> ValidateFromJsonAsync(string licenseJson, LicenseValidationOptions? options = null)
        {
            try
            {
                var signedLicense = JsonSerializer.Deserialize<SignedLicense>(licenseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (signedLicense == null)
                {
                    return LicenseValidationResult.Failure(LicenseStatus.Corrupted, "Invalid license JSON format");
                }

                return await ValidateAsync(signedLicense, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse license JSON");
                return LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License JSON parsing failed");
            }
        }

        public async Task<LicenseValidationResult> ValidateFromFileAsync(string filePath, LicenseValidationOptions? options = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return LicenseValidationResult.Failure(LicenseStatus.NotFound, $"License file not found: {filePath}");
                }

                var licenseJson = await File.ReadAllTextAsync(filePath);
                return await ValidateFromJsonAsync(licenseJson, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read license file {FilePath}", filePath);
                return LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, "Failed to read license file");
            }
        }

        public async Task<bool> IsFeatureAllowedAsync(string licenseId, string featureName)
        {
            try
            {
                // This would need integration with license repository to get the license
                // For now, return a basic implementation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check feature {FeatureName} for license {LicenseId}", featureName, licenseId);
                return false;
            }
        }

        public async Task<List<string>> GetAvailableFeaturesAsync(string licenseId)
        {
            try
            {
                // This would need integration with license repository
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available features for license {LicenseId}", licenseId);
                return new List<string>();
            }
        }

        public async Task<bool> ValidateSignatureAsync(SignedLicense signedLicense, string publicKey)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);

                var licenseDataBytes = Encoding.UTF8.GetBytes(signedLicense.LicenseData);
                var signatureBytes = Convert.FromBase64String(signedLicense.Signature);

                return rsa.VerifyData(licenseDataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signature validation failed");
                return false;
            }
        }

        private async Task<License?> DecodeLicenseDataAsync(SignedLicense signedLicense)
        {
            try
            {
                var licenseDataBytes = Convert.FromBase64String(signedLicense.LicenseData);
                var licenseJson = Encoding.UTF8.GetString(licenseDataBytes);

                return JsonSerializer.Deserialize<License>(licenseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode license data");
                return null;
            }
        }

        private LicenseValidationResult ValidateLicenseDates(License license, LicenseValidationOptions options)
        {
            var now = DateTime.UtcNow;

            // Check if license is not yet valid
            if (license.ValidFrom > now)
            {
                var result = LicenseValidationResult.Failure(LicenseStatus.NotYetValid, 
                    $"License is not yet valid. Valid from: {license.ValidFrom:yyyy-MM-dd HH:mm:ss} UTC");
                result.AreDatesValid = false;
                return result;
            }

            // Check if license has expired
            if (license.ValidTo < now)
            {
                // Check grace period
                if (options.AllowGracePeriod)
                {
                    var gracePeriodExpiry = license.ValidTo.AddDays(options.GracePeriodDays);
                    if (now <= gracePeriodExpiry)
                    {
                        var result = LicenseValidationResult.Success(license);
                        result.Status = LicenseStatus.GracePeriod;
                        result.IsGracePeriod = true;
                        result.GracePeriodExpiry = gracePeriodExpiry;
                        result.AreDatesValid = true;
                        result.AddMessage($"License is in grace period until {gracePeriodExpiry:yyyy-MM-dd HH:mm:ss} UTC");
                        return result;
                    }
                }

                var expiredResult = LicenseValidationResult.Failure(LicenseStatus.Expired, 
                    $"License has expired. Expired on: {license.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
                expiredResult.AreDatesValid = false;
                return expiredResult;
            }

            var validResult = LicenseValidationResult.Success(license);
            validResult.AreDatesValid = true;
            
            // Add warning if license expires soon
            var daysUntilExpiry = (license.ValidTo - now).TotalDays;
            if (daysUntilExpiry <= 7)
            {
                validResult.AddMessage($"Warning: License expires in {(int)daysUntilExpiry} days on {license.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
            }
            
            return validResult;
        }

        private async Task LogValidationAttempt(LicenseValidationResult result)
        {
            try
            {
                if (_defaultOptions.EnableAuditLogging && result.License != null)
                {
                    await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                    {
                        LicenseId = result.License.LicenseId,
                        ProductId = result.License.ProductId,
                        ConsumerId = result.License.ConsumerId,
                        Operation = LicenseOperation.Validated,
                        Description = $"License validation: {result.Status}",
                        PerformedBy = "ValidationService",
                        AdditionalDetails = new Dictionary<string, string>
                        {
                            ["ValidationStatus"] = result.Status.ToString(),
                            ["IsValid"] = result.IsValid.ToString(),
                            ["Messages"] = string.Join("; ", result.ValidationMessages)
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log validation attempt");
            }
        }
    }
}
