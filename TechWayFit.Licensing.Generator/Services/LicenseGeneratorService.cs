using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Service for generating tamper-proof licenses with RSA digital signatures
    /// This is used by the internal license generation application
    /// </summary>
    public class LicenseGenerator : ILicenseGenerator, IDisposable
    {
        private readonly ILogger<LicenseGenerator> _logger;
        private RSA _privateKey = null!;
        private RSA _publicKey = null!;
        private readonly string _keyStorePath;

        public LicenseGenerator(ILogger<LicenseGenerator> logger)
        {
            _logger = logger;
            
            // Set up key storage path relative to the content root (project directory)
            // This ensures keys persist across builds and deployments
            var contentRoot = Directory.GetCurrentDirectory();
            _keyStorePath = Path.Combine(contentRoot, "Keys");
            
            InitializeKeys();
        }

        public LicenseGenerator(ILogger<LicenseGenerator> logger, string keyStorePath)
        {
            _logger = logger;
            _keyStorePath = keyStorePath;
            
            InitializeKeys();
        }

        private void InitializeKeys()
        {
            // Ensure the directory exists
            Directory.CreateDirectory(_keyStorePath);
            
            // Try to load existing keys, generate new ones if not found
            if (!LoadExistingKeys())
            {
                _logger.LogInformation("No existing keys found, generating new key pair");
                GenerateNewKeyPair();
                SaveKeys();
            }
            else
            {
                _logger.LogInformation("Loaded existing RSA key pair from local storage: {KeyStorePath}", _keyStorePath);
            }
        }

        /// <summary>
        /// Generates a complete license with all required information using dynamic data
        /// </summary>
        /// <param name="request">Enhanced license generation request with dynamic data</param>
        /// <returns>Signed license ready for deployment</returns>
        public async Task<SignedLicense> GenerateLicenseAsync(EnhancedLicenseGenerationRequest request)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generating license for {LicensedTo} (Consumer: {ConsumerId}, Product: {ProductId})", 
                        request.LicensedTo, request.ConsumerId, request.ProductId);

                // Create the license object with dynamic data
                var license = new License
                {
                    LicenseId = Guid.NewGuid().ToString(),
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
                    FeaturesIncluded = request.Features, // Use dynamic features from request
                    IssuedAt = DateTime.UtcNow,
                    Metadata = request.Metadata
                };

                // Validate license data
                ValidateLicenseData(license);

                // Serialize license to JSON
                var licenseJson = JsonSerializer.Serialize(license, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Encode license data
                var licenseDataBytes = Encoding.UTF8.GetBytes(licenseJson);
                var licenseDataBase64 = Convert.ToBase64String(licenseDataBytes);

                // Generate digital signature
                var signature = GenerateSignature(licenseDataBytes);
                var signatureBase64 = Convert.ToBase64String(signature);

                // Get public key thumbprint for verification
                var publicKeyThumbprint = GetPublicKeyThumbprint();

                // Create signed license
                var signedLicense = new SignedLicense
                {
                    LicenseData = licenseDataBase64,
                    Signature = signatureBase64,
                    SignatureAlgorithm = "RS256",
                    PublicKeyThumbprint = publicKeyThumbprint,
                    FormatVersion = "1.0",
                    CreatedAt = DateTime.UtcNow,
                    Checksum = GenerateChecksum(licenseDataBase64)
                };

                _logger.LogInformation("License generated successfully for {LicensedTo} with ID {LicenseId}", 
                    request.LicensedTo, license.LicenseId);

                    return signedLicense;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate license for {LicensedTo}", request.LicensedTo);
                    throw;
                }
            });
        }

        /// <summary>
        /// Exports the public key for license validation
        /// </summary>
        /// <returns>Public key in PEM format</returns>
        public string ExportPublicKey()
        {
            var publicKeyBytes = _publicKey.ExportRSAPublicKey();
            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);
            
            var pemBuilder = new StringBuilder();
            pemBuilder.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
            
            // Split into 64-character lines
            for (int i = 0; i < publicKeyBase64.Length; i += 64)
            {
                var length = Math.Min(64, publicKeyBase64.Length - i);
                pemBuilder.AppendLine(publicKeyBase64.Substring(i, length));
            }
            
            pemBuilder.AppendLine("-----END RSA PUBLIC KEY-----");
            return pemBuilder.ToString();
        }

        /// <summary>
        /// Saves the private key for license generation (keep secure!)
        /// </summary>
        /// <param name="filePath">Path to save the private key</param>
        /// <param name="password">Password to encrypt the private key</param>
        public async Task SavePrivateKeyAsync(string filePath, string? password = null)
        {
            byte[] keyBytes;
            
            if (!string.IsNullOrEmpty(password))
            {
                // Encrypt private key with password
                keyBytes = _privateKey.ExportEncryptedPkcs8PrivateKey(
                    Encoding.UTF8.GetBytes(password), 
                    new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 10000));
            }
            else
            {
                keyBytes = _privateKey.ExportPkcs8PrivateKey();
            }

            var privateKeyBase64 = Convert.ToBase64String(keyBytes);
            var pemBuilder = new StringBuilder();
            
            if (!string.IsNullOrEmpty(password))
            {
                pemBuilder.AppendLine("-----BEGIN ENCRYPTED PRIVATE KEY-----");
            }
            else
            {
                pemBuilder.AppendLine("-----BEGIN PRIVATE KEY-----");
            }
            
            for (int i = 0; i < privateKeyBase64.Length; i += 64)
            {
                var length = Math.Min(64, privateKeyBase64.Length - i);
                pemBuilder.AppendLine(privateKeyBase64.Substring(i, length));
            }
            
            if (!string.IsNullOrEmpty(password))
            {
                pemBuilder.AppendLine("-----END ENCRYPTED PRIVATE KEY-----");
            }
            else
            {
                pemBuilder.AppendLine("-----END PRIVATE KEY-----");
            }

            await File.WriteAllTextAsync(filePath, pemBuilder.ToString());
            _logger.LogInformation("Private key saved to {FilePath}", filePath);
        }

        /// <summary>
        /// Loads a private key from file for license generation
        /// </summary>
        /// <param name="filePath">Path to the private key file</param>
        /// <param name="password">Password if the key is encrypted</param>
        public async Task LoadPrivateKeyAsync(string filePath, string? password = null)
        {
            var pemContent = await File.ReadAllTextAsync(filePath);
            var keyBytes = ExtractKeyBytesFromPem(pemContent);

            if (!string.IsNullOrEmpty(password))
            {
                _privateKey.ImportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(password), keyBytes, out _);
            }
            else
            {
                _privateKey.ImportPkcs8PrivateKey(keyBytes, out _);
            }

            // Also import public key
            _publicKey.ImportParameters(_privateKey.ExportParameters(false));
            
            _logger.LogInformation("Private key loaded from {FilePath}", filePath);
        }

        /// <summary>
        /// Generates a complete license with all required information using the original request format
        /// This method is kept for backward compatibility
        /// </summary>
        /// <param name="request">Original license generation request</param>
        /// <returns>Signed license ready for deployment</returns>
        public async Task<SignedLicense> GenerateLicenseAsync(LicenseGenerationRequest request)
        {
            // Convert the original request to the enhanced format
            var enhancedRequest = new EnhancedLicenseGenerationRequest
            {
                ProductId = request.ProductId,
                ConsumerId = request.ConsumerId,
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
                Features = CreateFeaturesForTier(request.Tier, request.CustomFeatures),
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
                CreatedBy = request.CreatedBy
            };

            return await GenerateLicenseAsync(enhancedRequest);
        }

        /// <summary>
        /// Creates features for a tier - kept for backward compatibility
        /// For new implementations, pass features directly in EnhancedLicenseGenerationRequest
        /// </summary>
        /// <param name="tier">License tier</param>
        /// <param name="customFeatures">Custom features to include</param>
        /// <returns>List of features for the tier</returns>
        private List<LicenseFeature> CreateFeaturesForTier(LicenseTier tier, List<LicenseFeature>? customFeatures = null)
        {
            var features = new List<LicenseFeature>();

            switch (tier)
            {
                case LicenseTier.Community:
                    features.AddRange(GetDefaultCommunityFeatures());
                    break;
                case LicenseTier.Professional:
                    features.AddRange(GetDefaultCommunityFeatures());
                    features.AddRange(GetDefaultProfessionalFeatures());
                    break;
                case LicenseTier.Enterprise:
                    features.AddRange(GetDefaultCommunityFeatures());
                    features.AddRange(GetDefaultProfessionalFeatures());
                    features.AddRange(GetDefaultEnterpriseFeatures());
                    break;
                case LicenseTier.Custom:
                    if (customFeatures != null)
                    {
                        features.AddRange(customFeatures);
                    }
                    break;
            }

            return features;
        }

        /// <summary>
        /// Default community features - used only for backward compatibility
        /// </summary>
        private List<LicenseFeature> GetDefaultCommunityFeatures()
        {
            return new List<LicenseFeature>
            {
                new LicenseFeature
                {
                    Name = "BasicApiGateway",
                    Description = "Basic API Gateway functionality",
                    Category = FeatureCategory.Core,
                    MinimumTier = LicenseTier.Community,
                    Limits = new FeatureLimits
                    {
                        MaxUsagePerMonth = 10000,
                        MaxConcurrentUsage = 10
                    }
                },
                new LicenseFeature
                {
                    Name = "BasicAuthentication",
                    Description = "JWT and API Key authentication",
                    Category = FeatureCategory.Security,
                    MinimumTier = LicenseTier.Community
                },
                new LicenseFeature
                {
                    Name = "BasicRateLimiting",
                    Description = "Simple rate limiting",
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Community
                }
            };
        }

        /// <summary>
        /// Default professional features - used only for backward compatibility
        /// </summary>
        private List<LicenseFeature> GetDefaultProfessionalFeatures()
        {
            return new List<LicenseFeature>
            {
                new LicenseFeature
                {
                    Name = "AdvancedAuthentication",
                    Description = "OAuth2, SAML, and certificate-based authentication",
                    Category = FeatureCategory.Security,
                    MinimumTier = LicenseTier.Professional
                },
                new LicenseFeature
                {
                    Name = "LoadBalancing",
                    Description = "Advanced load balancing strategies",
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Professional
                },
                new LicenseFeature
                {
                    Name = "RequestTransformation",
                    Description = "Header and body transformation",
                    Category = FeatureCategory.Integration,
                    MinimumTier = LicenseTier.Professional
                },
                new LicenseFeature
                {
                    Name = "BasicMonitoring",
                    Description = "Basic metrics and monitoring",
                    Category = FeatureCategory.Monitoring,
                    MinimumTier = LicenseTier.Professional
                }
            };
        }

        /// <summary>
        /// Default enterprise features - used only for backward compatibility
        /// </summary>
        private List<LicenseFeature> GetDefaultEnterpriseFeatures()
        {
            return new List<LicenseFeature>
            {
                new LicenseFeature
                {
                    Name = "AdvancedAnalytics",
                    Description = "Comprehensive analytics and reporting",
                    Category = FeatureCategory.BusinessIntelligence,
                    MinimumTier = LicenseTier.Enterprise
                },
                new LicenseFeature
                {
                    Name = "MultiRegion",
                    Description = "Multi-region deployment and failover",
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Enterprise
                },
                new LicenseFeature
                {
                    Name = "CustomPolicies",
                    Description = "Custom policy development and deployment",
                    Category = FeatureCategory.Management,
                    MinimumTier = LicenseTier.Enterprise
                },
                new LicenseFeature
                {
                    Name = "AdvancedSecurity",
                    Description = "WAF, DDoS protection, and advanced security features",
                    Category = FeatureCategory.Security,
                    MinimumTier = LicenseTier.Enterprise
                },
                new LicenseFeature
                {
                    Name = "UnlimitedClients",
                    Description = "Support for unlimited client configurations",
                    Category = FeatureCategory.Management,
                    MinimumTier = LicenseTier.Enterprise
                }
            };
        }

        /// <summary>
        /// Helper method to validate license data
        /// </summary>
        /// <param name="license">License to validate</param>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>

        private void ValidateLicenseData(License license)
        {
            if (string.IsNullOrWhiteSpace(license.LicensedTo))
                throw new ArgumentException("LicensedTo is required");

            if (string.IsNullOrWhiteSpace(license.ContactPerson))
                throw new ArgumentException("ContactPerson is required");

            if (string.IsNullOrWhiteSpace(license.ContactEmail))
                throw new ArgumentException("ContactEmail is required");

            if (license.ValidFrom >= license.ValidTo)
                throw new ArgumentException("ValidTo must be after ValidFrom");

            if (license.ValidTo <= DateTime.UtcNow.AddDays(-1))
                throw new ArgumentException("License cannot expire in the past");

            if (!license.FeaturesIncluded.Any())
                throw new ArgumentException("At least one feature must be included");
        }

        private byte[] GenerateSignature(byte[] data)
        {
            return _privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        private string GetPublicKeyThumbprint()
        {
            var publicKeyBytes = _publicKey.ExportRSAPublicKey();
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(publicKeyBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private string GenerateChecksum(string data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private byte[] ExtractKeyBytesFromPem(string pemContent)
        {
            // Remove PEM headers and whitespace
            var lines = pemContent.Split('\n')
                .Where(line => !line.StartsWith("-----"))
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line));

            var base64Content = string.Join("", lines);
            return Convert.FromBase64String(base64Content);
        }

        #region Key Management

        /// <summary>
        /// Generate a new RSA key pair and replace existing keys
        /// </summary>
        public void GenerateNewKeyPair()
        {
            _logger.LogInformation("Generating new RSA key pair");
            
            // Dispose existing keys
            _privateKey?.Dispose();
            _publicKey?.Dispose();
            
            // Generate new keys
            _privateKey = RSA.Create(2048);
            _publicKey = RSA.Create();
            
            // Import the public key from the private key
            var publicKeyParams = _privateKey.ExportParameters(false);
            _publicKey.ImportParameters(publicKeyParams);
            
            _logger.LogInformation("New RSA key pair generated successfully");
        }

        /// <summary>
        /// Load existing keys from storage
        /// </summary>
        /// <returns>True if keys were loaded successfully, false if no keys exist</returns>
        private bool LoadExistingKeys()
        {
            try
            {
                var privateKeyPath = Path.Combine(_keyStorePath, "private_key.pem");
                var publicKeyPath = Path.Combine(_keyStorePath, "public_key.pem");

                if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
                {
                    _logger.LogInformation("Key files not found at {PrivateKeyPath} or {PublicKeyPath}", 
                        privateKeyPath, publicKeyPath);
                    return false;
                }

                // Load private key
                var privateKeyPem = File.ReadAllText(privateKeyPath);
                _privateKey = RSA.Create();
                _privateKey.ImportFromPem(privateKeyPem);

                // Load public key
                var publicKeyPem = File.ReadAllText(publicKeyPath);
                _publicKey = RSA.Create();
                _publicKey.ImportFromPem(publicKeyPem);

                _logger.LogInformation("Successfully loaded existing RSA key pair from storage");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load existing keys from storage");
                return false;
            }
        }

        /// <summary>
        /// Save current keys to storage
        /// </summary>
        private void SaveKeys()
        {
            try
            {
                var privateKeyPath = Path.Combine(_keyStorePath, "private_key.pem");
                var publicKeyPath = Path.Combine(_keyStorePath, "public_key.pem");

                // Save private key
                var privateKeyPem = _privateKey.ExportPkcs8PrivateKeyPem();
                File.WriteAllText(privateKeyPath, privateKeyPem);

                // Save public key
                var publicKeyPem = _publicKey.ExportRSAPublicKeyPem();
                File.WriteAllText(publicKeyPath, publicKeyPem);

                // Set restrictive file permissions (Unix/Linux/macOS only)
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    try
                    {
                        // Set private key file to be readable only by owner (600)
                        File.SetUnixFileMode(privateKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                        // Set public key file to be readable by owner and group (644)
                        File.SetUnixFileMode(publicKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
                    }
                    catch (Exception permEx)
                    {
                        _logger.LogWarning(permEx, "Failed to set file permissions on key files");
                    }
                }

                _logger.LogInformation("Successfully saved RSA key pair to storage at {KeyStorePath}", _keyStorePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save keys to storage");
                throw;
            }
        }

        /// <summary>
        /// Generate a new key pair and save it to storage
        /// </summary>
        public void GenerateAndSaveNewKeyPair()
        {
            GenerateNewKeyPair();
            SaveKeys();
            _logger.LogInformation("Generated and saved new RSA key pair");
        }

        #endregion

        public void Dispose()
        {
            _privateKey?.Dispose();
            _publicKey?.Dispose();
        }
    }
}
