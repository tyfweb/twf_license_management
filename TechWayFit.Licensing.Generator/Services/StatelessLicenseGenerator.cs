using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Generator.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Stateless license generator that focuses purely on cryptographic operations
    /// Does not store any keys or data - all operations are performed on provided inputs
    /// </summary>
    public class StatelessLicenseGenerator : ILicenseGenerator
    {
        private readonly ILogger<StatelessLicenseGenerator> _logger;

        public StatelessLicenseGenerator(ILogger<StatelessLicenseGenerator> logger)
        {
            _logger = logger;
        }

        public Task<SignedLicense> GenerateLicenseAsync(SimplifiedLicenseGenerationRequest request)
        {
            _logger.LogInformation("Generating license for {LicensedTo} - Product: {ProductName}", 
                request.LicensedTo, request.ProductName);

            // Validate the request
            if (!request.IsValid())
            {
                var errors = request.GetValidationErrors();
                var errorMessage = $"Invalid license generation request: {string.Join(", ", errors)}";
                _logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // Validate the private key
            if (!ValidatePrivateKey(request.PrivateKeyPem))
            {
                throw new ArgumentException("Invalid private key provided");
            }

            try
            {
                // Create the license object
                var license = new License
                {
                    LicenseId = request.LicenseId,
                    LicensedTo = request.LicensedTo,
                    ContactPerson = request.ContactPerson,
                    ContactEmail = request.ContactEmail,
                    SecondaryContactPerson = request.SecondaryContactPerson,
                    SecondaryContactEmail = request.SecondaryContactEmail,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    Tier = request.Tier,
                    MaxApiCallsPerMonth = request.MaxApiCallsPerMonth,
                    MaxConcurrentConnections = request.MaxConcurrentConnections,
                    FeaturesIncluded = request.Features,
                    IssuedAt = DateTime.UtcNow,
                    Metadata = ConvertToStringDictionary(request.CustomData)
                };

                // Serialize the license for signing
                var licenseJson = JsonSerializer.Serialize(license, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false // Compact format for signing
                });

                // Convert to base64 for signing
                var licenseDataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(licenseJson));
                
                // Sign the license data
                var signature = SignData(licenseJson, request.PrivateKeyPem);
                
                // Get public key thumbprint
                var publicKeyThumbprint = GetPublicKeyThumbprint(request.PrivateKeyPem);

                var signedLicense = new SignedLicense
                {
                    LicenseData = licenseDataBase64,
                    Signature = signature,
                    SignatureAlgorithm = "RS256",
                    PublicKeyThumbprint = publicKeyThumbprint,
                    FormatVersion = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    Checksum = GenerateChecksum(licenseDataBase64)
                };

                _logger.LogInformation("Successfully generated license {LicenseId} for {LicensedTo}", 
                    license.LicenseId, license.LicensedTo);

                return Task.FromResult(signedLicense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating license for {LicensedTo}", request.LicensedTo);
                throw;
            }
        }

        public KeyGenerationResult GenerateKeyPair(int keySize = 2048)
        {
            _logger.LogInformation("Generating new RSA key pair with size {KeySize} bits", keySize);

            try
            {
                using var rsa = RSA.Create(keySize);
                
                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                var publicKeyPem = rsa.ExportRSAPublicKeyPem();

                var result = new KeyGenerationResult
                {
                    PublicKeyPem = publicKeyPem,
                    PrivateKeyPem = privateKeyPem,
                    KeySize = keySize,
                    Algorithm = "RSA",
                    GeneratedAt = DateTime.UtcNow,
                    KeyId = Guid.NewGuid().ToString()
                };

                _logger.LogInformation("Successfully generated RSA key pair {KeyId} with size {KeySize} bits", 
                    result.KeyId, keySize);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating RSA key pair with size {KeySize}", keySize);
                throw;
            }
        }

        public string ExtractPublicKeyFromPrivateKey(string privateKeyPem)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(privateKeyPem);
                return rsa.ExportRSAPublicKeyPem();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting public key from private key");
                throw new ArgumentException("Invalid private key format", ex);
            }
        }

        public bool ValidatePrivateKey(string privateKeyPem)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(privateKeyPem);
                
                // Try to sign some test data to verify the key works
                var testData = "test"u8.ToArray();
                var signature = rsa.SignData(testData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                return signature.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidatePublicKey(string publicKeyPem)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem);
                
                // Verify the key has a valid public key
                var parameters = rsa.ExportParameters(false);
                return parameters.Modulus != null && parameters.Exponent != null;
            }
            catch
            {
                return false;
            }
        }

        public string EncryptPrivateKey(string privateKeyPem, string password)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(privateKeyPem);
                
                // Convert password to bytes
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                
                // Export with password protection
                var encryptedKey = rsa.ExportEncryptedPkcs8PrivateKeyPem(passwordBytes, 
                    new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 100000));
                
                return encryptedKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting private key");
                throw new InvalidOperationException("Failed to encrypt private key", ex);
            }
        }

        public string DecryptPrivateKey(string encryptedPrivateKeyPem, string password)
        {
            try
            {
                using var rsa = RSA.Create();
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                
                rsa.ImportFromEncryptedPem(encryptedPrivateKeyPem, passwordBytes);
                
                return rsa.ExportRSAPrivateKeyPem();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting private key");
                throw new InvalidOperationException("Failed to decrypt private key - check password", ex);
            }
        }

        private string SignData(string data, string privateKeyPem)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);
            
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            return Convert.ToBase64String(signature);
        }

        private string GetPublicKeyThumbprint(string privateKeyPem)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);
            
            var publicKeyPem = rsa.ExportRSAPublicKeyPem();
            var publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyPem);
            
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(publicKeyBytes);
            
            return Convert.ToBase64String(hash);
        }

        private string GenerateChecksum(string data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        private Dictionary<string, string> ConvertToStringDictionary(Dictionary<string, object> source)
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in source)
            {
                result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
            return result;
        }
    }
}
