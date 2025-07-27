namespace TechWayFit.Licensing.Core.Models
{
    /// <summary>
    /// Represents a signed license with cryptographic protection
    /// This is the final format stored in appsettings.json
    /// </summary>
    public class SignedLicense
    {
        /// <summary>
        /// Base64-encoded license data (JSON)
        /// </summary>
        public string LicenseData { get; set; } = string.Empty;

        /// <summary>
        /// Base64-encoded digital signature of the license data
        /// </summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Algorithm used for signing (e.g., "RS256", "RS512")
        /// </summary>
        public string SignatureAlgorithm { get; set; } = "RS256";

        /// <summary>
        /// Thumbprint of the public key used for verification
        /// </summary>
        public string PublicKeyThumbprint { get; set; } = string.Empty;

        /// <summary>
        /// Version of the license format for compatibility
        /// </summary>
        public string FormatVersion { get; set; } = "1.0";

        /// <summary>
        /// Creation timestamp of the signed license
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional checksum for additional integrity verification
        /// </summary>
        public string? Checksum { get; set; }
    }

    /// <summary>
    /// Result of license validation process
    /// </summary>
    public class LicenseValidationResult
    {
        /// <summary>
        /// Overall validation status
        /// </summary>
        public LicenseStatus Status { get; set; }

        /// <summary>
        /// The validated license (null if validation failed)
        /// </summary>
        public License? License { get; set; }

        /// <summary>
        /// Detailed validation messages
        /// </summary>
        public List<string> ValidationMessages { get; set; } = new();

        /// <summary>
        /// Whether the license is in grace period
        /// </summary>
        public bool IsGracePeriod { get; set; }

        /// <summary>
        /// Grace period expiry date (if applicable)
        /// </summary>
        public DateTime? GracePeriodExpiry { get; set; }

        /// <summary>
        /// Validation timestamp
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the signature is valid
        /// </summary>
        public bool IsSignatureValid { get; set; }

        /// <summary>
        /// Whether the license dates are valid
        /// </summary>
        public bool AreDatesValid { get; set; }

        /// <summary>
        /// List of features that are available based on the license
        /// </summary>
        public List<LicenseFeature> AvailableFeatures { get; set; } = new();

        /// <summary>
        /// Indicates if validation was successful
        /// </summary>
        public bool IsValid => Status == LicenseStatus.Valid || Status == LicenseStatus.GracePeriod;

        /// <summary>
        /// Adds a validation message
        /// </summary>
        /// <param name="message">Message to add</param>
        public void AddMessage(string message)
        {
            ValidationMessages.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        /// <param name="license">Valid license</param>
        /// <returns>Successful validation result</returns>
        public static LicenseValidationResult Success(License license)
        {
            return new LicenseValidationResult
            {
                Status = LicenseStatus.Valid,
                License = license,
                IsSignatureValid = true,
                AreDatesValid = true,
                AvailableFeatures = license.FeaturesIncluded.Where(f => f.IsCurrentlyValid).ToList()
            };
        }

        /// <summary>
        /// Creates a failed validation result
        /// </summary>
        /// <param name="status">Failure status</param>
        /// <param name="message">Failure message</param>
        /// <returns>Failed validation result</returns>
        public static LicenseValidationResult Failure(LicenseStatus status, string message)
        {
            var result = new LicenseValidationResult
            {
                Status = status,
                IsSignatureValid = false,
                AreDatesValid = false
            };
            result.AddMessage(message);
            return result;
        }
    }

    /// <summary>
    /// Configuration for license validation
    /// </summary>
    public class LicenseValidationOptions
    {
        /// <summary>
        /// Grace period in days after license expiry
        /// </summary>
        public int GracePeriodDays { get; set; } = 30;

        /// <summary>
        /// Whether to allow grace period
        /// </summary>
        public bool AllowGracePeriod { get; set; } = true;

        /// <summary>
        /// Whether to cache validation results
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Cache duration in minutes
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 60;

        /// <summary>
        /// Whether to validate signature
        /// </summary>
        public bool ValidateSignature { get; set; } = true;

        /// <summary>
        /// Whether to validate license dates
        /// </summary>
        public bool ValidateDates { get; set; } = true;

        /// <summary>
        /// Whether to log validation attempts
        /// </summary>
        public bool EnableAuditLogging { get; set; } = true;
    }
}
