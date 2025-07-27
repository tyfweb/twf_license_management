public class LicenseFeature
    {
        /// <summary>
        /// Unique identifier for the feature
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the feature
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the feature
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the feature is currently valid
        /// </summary>
        public bool IsCurrentlyValid { get; set; } = true;

        /// <summary>
        /// Date when the feature was added
        /// </summary>
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the feature expires (if applicable)
        /// </summary>
        public DateTime? ExpiresOn { get; set; }
    }