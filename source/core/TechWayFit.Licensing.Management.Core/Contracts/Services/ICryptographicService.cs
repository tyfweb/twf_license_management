namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for cryptographic operations in license management
/// WARNING: This service should ONLY be used internally by IProductLicenseService.
/// Direct usage by other services or external components is not allowed.
/// All cryptographic operations must go through the ProductLicenseService layer.
/// </summary>
public interface ICryptographicService
{
    /// <summary>
    /// Generates a new license key
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="additionalData">Additional data to include in key generation</param>
    /// <returns>Generated license key</returns>
    Task<string> GenerateLicenseKeyAsync(string productId, string consumerId, Dictionary<string, object>? additionalData = null);

    /// <summary>
    /// Validates a license key cryptographically
    /// </summary>
    /// <param name="licenseKey">License key to validate</param>
    /// <param name="productId">Product ID</param>
    /// <param name="consumerId">Consumer ID</param>
    /// <returns>True if key is cryptographically valid</returns>
    Task<bool> ValidateLicenseKeyAsync(string licenseKey, string productId, string consumerId);

    /// <summary>
    /// Generates a digital signature for license data
    /// </summary>
    /// <param name="licenseData">License data to sign</param>
    /// <returns>Digital signature</returns>
    Task<string> GenerateDigitalSignatureAsync(string licenseData);

    /// <summary>
    /// Verifies a digital signature
    /// </summary>
    /// <param name="licenseData">Original license data</param>
    /// <param name="signature">Digital signature to verify</param>
    /// <returns>True if signature is valid</returns>
    Task<bool> VerifyDigitalSignatureAsync(string licenseData, string signature);

    /// <summary>
    /// Generates a cryptographic hash of license data
    /// </summary>
    /// <param name="licenseData">License data to hash</param>
    /// <returns>Cryptographic hash</returns>
    Task<string> GenerateLicenseHashAsync(string licenseData);

    /// <summary>
    /// Encrypts license data
    /// </summary>
    /// <param name="data">Data to encrypt</param>
    /// <returns>Encrypted data</returns>
    Task<string> EncryptDataAsync(string data);

    /// <summary>
    /// Decrypts license data
    /// </summary>
    /// <param name="encryptedData">Encrypted data to decrypt</param>
    /// <returns>Decrypted data</returns>
    Task<string> DecryptDataAsync(string encryptedData);

    /// <summary>
    /// Generates a unique activation code
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <param name="machineId">Machine ID</param>
    /// <returns>Activation code</returns>
    Task<string> GenerateActivationCodeAsync(string licenseKey, string machineId);

    /// <summary>
    /// Validates an activation code
    /// </summary>
    /// <param name="activationCode">Activation code to validate</param>
    /// <param name="licenseKey">License key</param>
    /// <param name="machineId">Machine ID</param>
    /// <returns>True if activation code is valid</returns>
    Task<bool> ValidateActivationCodeAsync(string activationCode, string licenseKey, string machineId);

    /// <summary>
    /// Generates a secure random token
    /// </summary>
    /// <param name="length">Token length</param>
    /// <returns>Secure random token</returns>
    Task<string> GenerateSecureTokenAsync(int length = 32);

    /// <summary>
    /// Generates a machine fingerprint
    /// </summary>
    /// <param name="machineInfo">Machine information</param>
    /// <returns>Machine fingerprint</returns>
    Task<string> GenerateMachineFingerprintAsync(Dictionary<string, object> machineInfo);

    /// <summary>
    /// Validates cryptographic integrity of license
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <param name="digitalSignature">Digital signature</param>
    /// <param name="licenseHash">License hash</param>
    /// <returns>Cryptographic validation result</returns>
    Task<CryptographicValidationResult> ValidateLicenseIntegrityAsync(string licenseKey, string digitalSignature, string licenseHash);

    /// <summary>
    /// Rotates cryptographic keys
    /// </summary>
    /// <returns>True if key rotation succeeded</returns>
    Task<bool> RotateKeysAsync();

    /// <summary>
    /// Gets current key version
    /// </summary>
    /// <returns>Current key version</returns>
    Task<string> GetCurrentKeyVersionAsync();
}

/// <summary>
/// Cryptographic validation result
/// </summary>
public class CryptographicValidationResult
{
    public bool IsValid { get; set; }
    public bool SignatureValid { get; set; }
    public bool HashValid { get; set; }
    public bool KeyValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ValidationDetails { get; set; } = new();
}
