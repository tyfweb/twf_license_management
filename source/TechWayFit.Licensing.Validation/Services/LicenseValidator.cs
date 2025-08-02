using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Services;

namespace TechWayFit.Licensing.Validation.Services
{
    /// <summary>
    /// Service for validating tamper-proof licenses using RSA digital signatures
    /// This is used by the API Gateway and Web UI for license verification
    /// </summary>
    public class LicenseValidator : ILicenseValidator
    {
        private readonly ILogger<LicenseValidator> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly RSA _publicKey;
        private readonly LicenseValidationOptions _defaultOptions;

        public LicenseValidator(
            ILogger<LicenseValidator> logger,
            IConfiguration configuration,
            IMemoryCache cache)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            _publicKey = RSA.Create();
            
            _defaultOptions = new LicenseValidationOptions();
            configuration.GetSection("LicenseValidation").Bind(_defaultOptions);

            // Load public key for validation
            LoadPublicKeyFromConfiguration();
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

                // Validate signature first
                if (validationOptions.ValidateSignature)
                {
                    var signatureValid = await ValidateSignatureAsync(signedLicense);
                    if (!signatureValid)
                    {
                        var result = LicenseValidationResult.Failure(LicenseStatus.Invalid, "License signature validation failed");
                        await LogValidationAttempt(result);
                        return result;
                    }
                }

                // Decode and deserialize license data
                var license = await DecodeLicenseDataAsync(signedLicense);
                if (license == null)
                {
                    var result = LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License data is corrupted or unreadable");
                    await LogValidationAttempt(result);
                    return result;
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

                _logger.LogInformation("License validation successful for {LicensedTo}, Tier: {Tier}, Features: {FeatureCount}",
                    license.LicensedTo, license.Tier, successResult.AvailableFeatures.Count);

                // Cache the result if enabled
                if (validationOptions.EnableCaching)
                {
                    var cacheExpiry = TimeSpan.FromMinutes(validationOptions.CacheDurationMinutes);
                    _cache.Set(cacheKey, successResult, cacheExpiry);
                }

                await LogValidationAttempt(successResult);
                return successResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during license validation");
                var errorResult = LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, 
                    $"License validation service error: {ex.Message}");
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
                _logger.LogWarning("Failed to parse license JSON: {Error}", ex.Message);
                return LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License JSON parsing failed");
            }
        }

        public async Task<LicenseValidationResult> ValidateFromConfigurationAsync(LicenseValidationOptions? options = null)
        {
            try
            {
                var licenseSection = _configuration.GetSection("License");
                if (!licenseSection.Exists())
                {
                    return LicenseValidationResult.Failure(LicenseStatus.NotFound, "No license found in configuration");
                }

                var signedLicense = new SignedLicense();
                licenseSection.Bind(signedLicense);

                if (string.IsNullOrEmpty(signedLicense.LicenseData))
                {
                    return LicenseValidationResult.Failure(LicenseStatus.NotFound, "License data not found in configuration");
                }

                return await ValidateAsync(signedLicense, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load license from configuration");
                return LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, "Failed to load license from configuration");
            }
        }

        public async Task<bool> IsFeatureAvailableAsync(string featureName)
        {
            var validation = await ValidateFromConfigurationAsync();
            return validation.IsValid && validation.AvailableFeatures.Contains(featureName, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<License?> GetCurrentLicenseAsync()
        {
            var validation = await ValidateFromConfigurationAsync();
            return validation.IsValid ? validation.License : null;
        }

        public async Task<List<string>> GetAvailableFeaturesAsync()
        {
            var validation = await ValidateFromConfigurationAsync();
            return validation.IsValid ? validation.AvailableFeatures : new List<string>();
        }

        public async Task<bool> SupportsLicenseTierAsync(LicenseTier requiredTier)
        {
            var license = await GetCurrentLicenseAsync();
            return license != null && license.Tier >= requiredTier;
        }

        public async Task<FeatureLimits?> GetFeatureLimitsAsync(string featureName)
        {
            var license = await GetCurrentLicenseAsync();
            var feature = license?.GetFeature(featureName);
            return feature?.Limits;
        }

        private async Task<bool> ValidateSignatureAsync(SignedLicense signedLicense)
        {
            try
            {
                var licenseDataBytes = Convert.FromBase64String(signedLicense.LicenseData);
                var signatureBytes = Convert.FromBase64String(signedLicense.Signature);

                // Verify the signature
                var isValid = _publicKey.VerifyData(licenseDataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                if (!isValid)
                {
                    _logger.LogWarning("License signature validation failed");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signature validation");
                return false;
            }
        }

        private async Task<License?> DecodeLicenseDataAsync(SignedLicense signedLicense)
        {
            try
            {
                var licenseDataBytes = Convert.FromBase64String(signedLicense.LicenseData);
                var licenseJson = Encoding.UTF8.GetString(licenseDataBytes);

                var license = JsonSerializer.Deserialize<License>(licenseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return license;
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
            if (now < license.ValidFrom)
            {
                var result = LicenseValidationResult.Failure(LicenseStatus.NotYetValid, 
                    $"License is not yet valid. Valid from: {license.ValidFrom:yyyy-MM-dd}");
                result.AreDatesValid = false;
                return result;
            }

            // Check if license has expired
            if (now > license.ValidTo)
            {
                // Check grace period
                if (options.AllowGracePeriod)
                {
                    var gracePeriodEnd = license.ValidTo.AddDays(options.GracePeriodDays);
                    if (now <= gracePeriodEnd)
                    {
                        var result = LicenseValidationResult.Success(license);
                        result.Status = LicenseStatus.GracePeriod;
                        result.IsGracePeriod = true;
                        result.GracePeriodExpiry = gracePeriodEnd;
                        result.AddMessage($"License is in grace period until {gracePeriodEnd:yyyy-MM-dd}");
                        
                        // Populate available features for grace period
                        result.AvailableFeatures = license.FeaturesIncluded
                            .Where(f => f.IsCurrentlyValid)
                            .Select(f => f.Name)
                            .ToList();
                        
                        return result;
                    }
                }

                var expiredResult = LicenseValidationResult.Failure(LicenseStatus.Expired, 
                    $"License expired on {license.ValidTo:yyyy-MM-dd}");
                expiredResult.AreDatesValid = false;
                return expiredResult;
            }

            // License dates are valid
            var validResult = LicenseValidationResult.Success(license);
            validResult.AreDatesValid = true;
            return validResult;
        }

        private void LoadPublicKeyFromConfiguration()
        {
            try
            {
                var publicKeyPem = _configuration["LicenseValidation:PublicKey"];
                if (string.IsNullOrEmpty(publicKeyPem))
                {
                    throw new InvalidOperationException("Public key not found in configuration. Add LicenseValidation:PublicKey to appsettings.json");
                }

                var publicKeyBytes = ExtractKeyBytesFromPem(publicKeyPem);
                _publicKey.ImportRSAPublicKey(publicKeyBytes, out _);

                _logger.LogInformation("License validation public key loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load public key for license validation");
                throw new InvalidOperationException("License validation cannot be initialized without a valid public key", ex);
            }
        }

        private byte[] ExtractKeyBytesFromPem(string pemContent)
        {
            var lines = pemContent.Split('\n')
                .Where(line => !line.StartsWith("-----"))
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line));

            var base64Content = string.Join("", lines);
            return Convert.FromBase64String(base64Content);
        }

        private async Task LogValidationAttempt(LicenseValidationResult result)
        {
            if (_defaultOptions.EnableAuditLogging)
            {
                _logger.LogInformation("License validation attempt: Status={Status}, Valid={IsValid}, Features={FeatureCount}",
                    result.Status, result.IsValid, result.AvailableFeatures.Count);
            }
        }

        public void Dispose()
        {
            _publicKey?.Dispose();
        }
    }
}
