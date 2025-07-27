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
    /// Service for validating tamper-proof licenses using RSA digital signatures.
    /// Provides lean validation focused on cryptographic integrity and temporal validation.
    /// Consuming applications handle feature-specific business logic using the validated license data.
    /// </summary>
    public class LicenseValidationService : ILicenseValidationService
    {
        private readonly ILogger<LicenseValidationService> _logger;
        private readonly IMemoryCache _cache;
        private readonly LicenseValidationOptions _defaultOptions;

        /// <summary>
        /// Initializes a new instance of the LicenseValidationService.
        /// </summary>
        /// <param name="logger">Logger for validation operations</param>
        /// <param name="configuration">Configuration provider for default settings</param>
        /// <param name="cache">Memory cache for validation result caching</param>
        public LicenseValidationService(
            ILogger<LicenseValidationService> logger,
            IConfiguration configuration,
            IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            _defaultOptions = configuration.GetSection("LicenseValidation").Get<LicenseValidationOptions>() ?? new LicenseValidationOptions();
        }

        /// <summary>
        /// Validates a signed license with cryptographic signature verification and temporal validation.
        /// Returns the decoded license data for consuming applications to interpret features and limits.
        /// </summary>
        /// <param name="signedLicense">The signed license to validate</param>
        /// <param name="publicKey">The RSA public key for signature verification</param>
        /// <param name="options">Optional validation options (uses defaults if not provided)</param>
        /// <returns>License validation result containing decoded license data and validation status</returns>
        public Task<LicenseValidationResult> ValidateAsync(SignedLicense signedLicense,string publicKey, LicenseValidationOptions? options = null)
        {
            var validationOptions = options ?? _defaultOptions;
            var cacheKey = $"license_validation_{signedLicense.PublicKeyThumbprint}_{signedLicense.Checksum}";

            try
            {
                // Check cache first if enabled
                if (validationOptions.EnableCaching && _cache.TryGetValue(cacheKey, out LicenseValidationResult? cachedResult) && cachedResult != null)
                {
                    _logger.LogDebug("License validation result retrieved from cache");
                    return Task.FromResult(cachedResult);
                }

                _logger.LogInformation("Validating license with format version {FormatVersion}", signedLicense.FormatVersion);

                // Decode and deserialize license data first
                var license = DecodeLicenseData(signedLicense);
                if (license == null)
                {
                    var result = LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License data is corrupted or unreadable");
                    LogValidationAttempt(result);
                    return Task.FromResult(result);
                }                 

                // Validate signature if enabled
                if (validationOptions.ValidateSignature)
                {
                    var signatureValid = ValidateSignature(signedLicense, publicKey);
                    if (!signatureValid)
                    {
                        var result = LicenseValidationResult.Failure(LicenseStatus.Invalid, "License signature validation failed");
                        LogValidationAttempt(result);
                        return Task.FromResult(result);
                    }
                }

                // Validate dates if enabled
                if (validationOptions.ValidateDates)
                {
                    var dateValidation = ValidateLicenseDates(license, validationOptions);
                    if (!dateValidation.IsValid)
                    {
                        LogValidationAttempt(dateValidation);
                        if (validationOptions.EnableCaching)
                        {
                            _cache.Set(cacheKey, dateValidation, TimeSpan.FromMinutes(validationOptions.CacheDurationMinutes));
                        }
                        return Task.FromResult(dateValidation);
                    }
                }

                // Create successful validation result
                var successResult = LicenseValidationResult.Success(license);
                successResult.IsSignatureValid = true;
                successResult.AreDatesValid = true;

                // Note: Core does not interpret features - consuming applications handle feature validation
                // using the validated license data in successResult.License

                // Cache result if enabled
                if (validationOptions.EnableCaching)
                {
                    _cache.Set(cacheKey, successResult, TimeSpan.FromMinutes(validationOptions.CacheDurationMinutes));
                }

                LogValidationAttempt(successResult);
                return Task.FromResult(successResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "License validation failed with exception");
                var errorResult = LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable,
                    $"License validation service encountered an error: {ex.Message}");
                LogValidationAttempt(errorResult);
                return Task.FromResult(errorResult);
            }
        }

        /// <summary>
        /// Validates a license from JSON string format.
        /// </summary>
        /// <param name="licenseJson">The license in JSON format</param>
        /// <param name="publicKey">The RSA public key for signature verification</param>
        /// <param name="options">Optional validation options (uses defaults if not provided)</param>
        /// <returns>License validation result containing decoded license data and validation status</returns>
        public async Task<LicenseValidationResult> ValidateFromJsonAsync(string licenseJson, string publicKey, LicenseValidationOptions? options = null)
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

                return await ValidateAsync(signedLicense, publicKey, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse license JSON");
                return LicenseValidationResult.Failure(LicenseStatus.Corrupted, "License JSON parsing failed");
            }
        }

        /// <summary>
        /// Validates a license from file.
        /// </summary>
        /// <param name="filePath">Path to the license file</param>
        /// <param name="publicKeyPath">Path to the public key file</param>
        /// <param name="options">Optional validation options (uses defaults if not provided)</param>
        /// <returns>License validation result containing decoded license data and validation status</returns>
        public async Task<LicenseValidationResult> ValidateFromFileAsync(string filePath, string publicKeyPath, LicenseValidationOptions? options = null)
        {
            try
            {
                if (!File.Exists(filePath) || !File.Exists(publicKeyPath))
                {
                    _logger.LogError("License file or public key file not found: {FilePath} or {PublicKeyPath}", filePath, publicKeyPath);
                    return LicenseValidationResult.Failure(LicenseStatus.NotFound, $"License file or public key file not found: {filePath} or {publicKeyPath}");
                }
                var publicKey = await File.ReadAllTextAsync(publicKeyPath);
                if (string.IsNullOrWhiteSpace(publicKey))
                {
                    _logger.LogError("Public key file is empty: {PublicKeyPath}", publicKeyPath);
                    return LicenseValidationResult.Failure(LicenseStatus.Invalid, "Public key file is empty");
                }
                var licenseJson = await File.ReadAllTextAsync(filePath);
                return await ValidateFromJsonAsync(licenseJson, publicKey, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read license file {FilePath}", filePath);
                return LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, "Failed to read license file");
            }
        }

        /// <summary>
        /// Validates the digital signature of a signed license
        /// </summary>
        /// <param name="signedLicense">The signed license to validate</param>
        /// <param name="publicKey">Public key for signature verification</param>
        /// <returns>True if signature is valid</returns>
        public bool ValidateSignature(SignedLicense signedLicense, string publicKey)
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

        private License? DecodeLicenseData(SignedLicense signedLicense)
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

        private void LogValidationAttempt(LicenseValidationResult result)
        {
            try
            {
                //TODO: Log in memory and block after 3 failed retry.
                if (_defaultOptions.EnableAuditLogging && result.License != null)
                {
                    var logEntry = new
                    {
                        LicenseId = result.License.LicenseId,
                        Status = result.Status,
                        IsSignatureValid = result.IsSignatureValid,
                        AreDatesValid = result.AreDatesValid,
                        AvailableFeatures = result.AvailableFeatures,
                        Messages = result.ValidationMessages,
                        Timestamp = DateTime.UtcNow
                    };

                    // Log the validation attempt (this could be to a file, database, etc.)
                    _logger.LogInformation("License validation attempt: {@LogEntry}", logEntry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log validation attempt");
            }
        }
    }
}
