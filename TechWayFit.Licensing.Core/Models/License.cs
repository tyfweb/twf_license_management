using System.Text.Json.Serialization;

namespace TechWayFit.Licensing.Core.Models
{
    /// <summary>
    /// Core license model containing all license information and features
    /// This class represents the license payload that gets signed and validated
    /// </summary>
    public class License
    {
        /// <summary>
        /// Product this license is for
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product type enumeration
        /// </summary>
        public ProductType ProductType { get; set; } = ProductType.ApiGateway;

        /// <summary>
        /// Consumer this license belongs to
        /// </summary>
        public string ConsumerId { get; set; } = string.Empty;

        /// <summary>
        /// Primary organization or individual licensed to use the software
        /// </summary>
        public string LicensedTo { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact person for the license
        /// </summary>
        public string ContactPerson { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact email address
        /// </summary>
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Secondary contact person (optional)
        /// </summary>
        public string? SecondaryContactPerson { get; set; }

        /// <summary>
        /// Secondary contact email address (optional)
        /// </summary>
        public string? SecondaryContactEmail { get; set; }

        /// <summary>
        /// Date when the license becomes valid
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Date when the license expires
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Current license status
        /// </summary>
        public LicenseStatus Status { get; set; } = LicenseStatus.Pending;

        /// <summary>
        /// List of features included in this license
        /// </summary>
        public List<LicenseFeature> FeaturesIncluded { get; set; } = new();

        /// <summary>
        /// Unique license identifier
        /// </summary>
        public string LicenseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// License tier (Community, Professional, Enterprise)
        /// </summary>
        public LicenseTier Tier { get; set; }

        /// <summary>
        /// Maximum number of API calls per month (if applicable)
        /// </summary>
        public int? MaxApiCallsPerMonth { get; set; }

        /// <summary>
        /// Maximum number of concurrent connections (if applicable)
        /// </summary>
        public int? MaxConcurrentConnections { get; set; }

        /// <summary>
        /// License generation timestamp
        /// </summary>
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Issuer information (TechWayFit)
        /// </summary>
        public string Issuer { get; set; } = "TechWayFit";

        /// <summary>
        /// License version for compatibility tracking
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// License version number for lifecycle management
        /// </summary>
        public int VersionNumber { get; set; } = 1;

        /// <summary>
        /// Previous license ID if this is a renewal/update
        /// </summary>
        public string? PreviousLicenseId { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created/updated this license
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata as key-value pairs
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Product version this license is for
        /// </summary>
        public Version? ProductVersion { get; set; }

        /// <summary>
        /// Maximum supported product version
        /// </summary>
        public Version? MaxSupportedVersion { get; set; }

        /// <summary>
        /// Tier identifier string
        /// </summary>
        public string TierId { get; set; } = string.Empty;

        /// <summary>
        /// Usage metrics and statistics
        /// </summary>
        public Dictionary<string, object> UsageMetrics { get; set; } = new();

        /// <summary>
        /// Version compatibility information
        /// </summary>
        public LicenseVersionCompatibility? VersionCompatibility { get; set; }

        /// <summary>
        /// Additional notes for this license
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// When this license was revoked (if applicable)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Who revoked this license
        /// </summary>
        public string? RevokedBy { get; set; }

        /// <summary>
        /// Reason for revocation
        /// </summary>
        public string? RevocationReason { get; set; }

        /// <summary>
        /// When this license was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who last updated this license
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Checks if the license is currently valid based on date range
        /// </summary>
        [JsonIgnore]
        public bool IsValid => DateTime.UtcNow <= ValidTo;

        /// <summary>
        /// Checks if the license expires within the specified number of days
        /// </summary>
        /// <param name="days">Number of days to check ahead</param>
        /// <returns>True if license expires within the specified days</returns>
        public bool ExpiresWithin(int days) => (ValidTo - DateTime.UtcNow).TotalDays <= days;

        /// <summary>
        /// Gets the number of days until expiry
        /// </summary>
        [JsonIgnore]
        public int DaysUntilExpiry => Math.Max(0, (int)(ValidTo - DateTime.UtcNow).TotalDays);

        /// <summary>
        /// Checks if a specific feature is included in this license
        /// </summary>
        /// <param name="featureName">Name of the feature to check</param>
        /// <returns>True if feature is included and enabled</returns>
        public bool HasFeature(string featureName)
        {
            return FeaturesIncluded.Any(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the feature configuration if it exists
        /// </summary>
        /// <param name="featureName">Name of the feature</param>
        /// <returns>LicenseFeature if found, null otherwise</returns>
        public LicenseFeature? GetFeature(string featureName)
        {
            return FeaturesIncluded.FirstOrDefault(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// License version compatibility information
    /// </summary>
    public class LicenseVersionCompatibility
    {
        /// <summary>
        /// Minimum supported version
        /// </summary>
        public Version? MinVersion { get; set; }

        /// <summary>
        /// Maximum supported version
        /// </summary>
        public Version? MaxVersion { get; set; }

        /// <summary>
        /// Supported version ranges
        /// </summary>
        public List<VersionRange> SupportedRanges { get; set; } = new();

        /// <summary>
        /// Excluded versions
        /// </summary>
        public List<Version> ExcludedVersions { get; set; } = new();

        /// <summary>
        /// Compatibility notes
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Version range specification
    /// </summary>
    public class VersionRange
    {
        /// <summary>
        /// Start version (inclusive)
        /// </summary>
        public Version FromVersion { get; set; } = new();

        /// <summary>
        /// End version (inclusive)
        /// </summary>
        public Version ToVersion { get; set; } = new();

        /// <summary>
        /// Range description
        /// </summary>
        public string? Description { get; set; }
    }
}
