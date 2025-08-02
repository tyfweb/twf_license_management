using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Generator.Models
{
    /// <summary>
    /// Simplified license generation request containing only the essential data needed for license generation.
    /// This model is stateless and does not include any database-specific or management-related data.
    /// </summary>
    public class SimplifiedLicenseGenerationRequest
    {
        /// <summary>
        /// Unique identifier for this license
        /// </summary>
        public string LicenseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Product identifier for which the license is being generated
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Organization or person this license is issued to
        /// </summary>
        public string LicensedTo { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact person
        /// </summary>
        public string ContactPerson { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact email
        /// </summary>
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Secondary contact person (optional)
        /// </summary>
        public string? SecondaryContactPerson { get; set; }

        /// <summary>
        /// Secondary contact email (optional)
        /// </summary>
        public string? SecondaryContactEmail { get; set; }

        /// <summary>
        /// License start date (UTC)
        /// </summary>
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// License expiration date (UTC)
        /// </summary>
        public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddYears(1);

        /// <summary>
        /// License tier
        /// </summary>
        public LicenseTier Tier { get; set; } = LicenseTier.Community;

        /// <summary>
        /// Maximum API calls per month (optional, for Professional/Enterprise tiers)
        /// </summary>
        public int? MaxApiCallsPerMonth { get; set; }

        /// <summary>
        /// Maximum concurrent connections (optional, for Professional/Enterprise tiers)
        /// </summary>
        public int? MaxConcurrentConnections { get; set; }

        /// <summary>
        /// Additional custom features for this license
        /// </summary>
        public List<LicenseFeature> Features { get; set; } = new List<LicenseFeature>();

        /// <summary>
        /// Custom metadata that will be included in the license
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Private key PEM string for signing the license
        /// This should be provided by the management system, not stored by the generator
        /// </summary>
        public string PrivateKeyPem { get; set; } = string.Empty;

        /// <summary>
        /// Optional issuer information
        /// </summary>
        public string Issuer { get; set; } = "TechWayFit";

        /// <summary>
        /// Validates the request to ensure all required fields are present
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ProductId) &&
                   !string.IsNullOrWhiteSpace(ProductName) &&
                   !string.IsNullOrWhiteSpace(LicensedTo) &&
                   !string.IsNullOrWhiteSpace(ContactEmail) &&
                   !string.IsNullOrWhiteSpace(PrivateKeyPem) &&
                   ValidFrom < ValidTo;
        }

        /// <summary>
        /// Gets validation errors for the request
        /// </summary>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ProductId))
                errors.Add("ProductId is required");

            if (string.IsNullOrWhiteSpace(ProductName))
                errors.Add("ProductName is required");

            if (string.IsNullOrWhiteSpace(LicensedTo))
                errors.Add("LicensedTo is required");

            if (string.IsNullOrWhiteSpace(ContactEmail))
                errors.Add("ContactEmail is required");

            if (string.IsNullOrWhiteSpace(PrivateKeyPem))
                errors.Add("PrivateKeyPem is required for license signing");

            if (ValidFrom >= ValidTo)
                errors.Add("ValidFrom must be before ValidTo");

            return errors;
        }
    }
}
