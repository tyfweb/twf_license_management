namespace TechWayFit.Licensing.Generator.Models
{
    /// <summary>
    /// Result of key pair generation containing both public and private keys
    /// </summary>
    public class KeyGenerationResult
    {
        /// <summary>
        /// Public key in PEM format for license validation
        /// This can be safely distributed and embedded in applications
        /// </summary>
        public string PublicKeyPem { get; set; } = string.Empty;

        /// <summary>
        /// Private key in PEM format for license signing
        /// This must be kept secure and only used by the license generation system
        /// </summary>
        public string PrivateKeyPem { get; set; } = string.Empty;

        /// <summary>
        /// Key size in bits (e.g., 2048, 4096)
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        /// Algorithm used for key generation (e.g., "RSA")
        /// </summary>
        public string Algorithm { get; set; } = "RSA";

        /// <summary>
        /// Timestamp when the keys were generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique identifier for this key pair
        /// Can be used for key management and rotation
        /// </summary>
        public string KeyId { get; set; } = Guid.NewGuid().ToString();
    }
}
