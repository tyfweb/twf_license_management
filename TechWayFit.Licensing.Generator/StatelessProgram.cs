using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Generator.Services;

namespace TechWayFit.Licensing.Generator
{
    /// <summary>
    /// Stateless License Generator Console Application
    /// This tool generates licenses, public/private keys without storing any data
    /// All inputs must be provided and all outputs are returned to the caller
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create host builder
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                    services.AddSingleton<ILicenseGenerator, StatelessLicenseGenerator>();
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var licenseGenerator = host.Services.GetRequiredService<ILicenseGenerator>();

            try
            {
                logger.LogInformation("TechWayFit Stateless License Generator v2.0");
                logger.LogInformation("=============================================");

                if (args.Length == 0)
                {
                    await InteractiveMode(licenseGenerator, logger);
                }
                else
                {
                    await CommandLineMode(args, licenseGenerator, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during license generation");
                Environment.Exit(1);
            }
        }

        private static async Task InteractiveMode(ILicenseGenerator generator, ILogger logger)
        {
            logger.LogInformation("Running in interactive mode...");
            
            while (true)
            {
                Console.WriteLine("\nTechWayFit Stateless License Generator");
                Console.WriteLine("1. Generate New Key Pair");
                Console.WriteLine("2. Generate License (requires private key)");
                Console.WriteLine("3. Extract Public Key from Private Key");
                Console.WriteLine("4. Validate Private Key");
                Console.WriteLine("5. Validate Public Key");
                Console.WriteLine("6. Encrypt Private Key");
                Console.WriteLine("7. Decrypt Private Key");
                Console.WriteLine("8. Exit");
                Console.Write("Select option (1-8): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        GenerateKeyPairInteractive(generator, logger);
                        break;
                    case "2":
                        await GenerateLicenseInteractive(generator, logger);
                        break;
                    case "3":
                        ExtractPublicKeyInteractive(generator, logger);
                        break;
                    case "4":
                        ValidatePrivateKeyInteractive(generator, logger);
                        break;
                    case "5":
                        ValidatePublicKeyInteractive(generator, logger);
                        break;
                    case "6":
                        EncryptPrivateKeyInteractive(generator, logger);
                        break;
                    case "7":
                        DecryptPrivateKeyInteractive(generator, logger);
                        break;
                    case "8":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task CommandLineMode(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "generate-keys":
                    await GenerateKeysFromArgs(args, generator, logger);
                    break;
                case "generate-license":
                    await GenerateLicenseFromArgs(args, generator, logger);
                    break;
                case "extract-public":
                    await ExtractPublicKeyFromArgs(args, generator, logger);
                    break;
                case "validate-private":
                    await ValidatePrivateKeyFromArgs(args, generator, logger);
                    break;
                case "validate-public":
                    await ValidatePublicKeyFromArgs(args, generator, logger);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static void GenerateKeyPairInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Key Pair Generation ===");
                
                Console.Write("Key size in bits (2048, 4096) [default: 2048]: ");
                var keySizeInput = Console.ReadLine();
                
                int keySize = 2048;
                if (!string.IsNullOrEmpty(keySizeInput) && int.TryParse(keySizeInput, out var customKeySize))
                {
                    keySize = customKeySize;
                }

                var result = generator.GenerateKeyPair(keySize);

                // Save to files
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var privateKeyFile = $"private_key_{timestamp}.pem";
                var publicKeyFile = $"public_key_{timestamp}.pem";

                File.WriteAllText(privateKeyFile, result.PrivateKeyPem);
                File.WriteAllText(publicKeyFile, result.PublicKeyPem);

                Console.WriteLine($"\n✅ Key pair generated successfully!");
                Console.WriteLine($"🔒 Private key saved to: {privateKeyFile}");
                Console.WriteLine($"🔓 Public key saved to: {publicKeyFile}");
                Console.WriteLine($"🔑 Key ID: {result.KeyId}");
                Console.WriteLine($"📏 Key size: {result.KeySize} bits");
                Console.WriteLine($"⚠️  Keep the private key secure - it's used to generate licenses!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate key pair");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static async Task GenerateLicenseInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== License Generation ===");
                
                var request = new SimplifiedLicenseGenerationRequest();

                Console.Write("Product ID: ");
                request.ProductId = Console.ReadLine() ?? "";

                Console.Write("Product Name: ");
                request.ProductName = Console.ReadLine() ?? "";

                Console.Write("Licensed To (Organization/Person): ");
                request.LicensedTo = Console.ReadLine() ?? "";

                Console.Write("Contact Person: ");
                request.ContactPerson = Console.ReadLine() ?? "";

                Console.Write("Contact Email: ");
                request.ContactEmail = Console.ReadLine() ?? "";

                Console.Write("Secondary Contact Person (optional): ");
                request.SecondaryContactPerson = Console.ReadLine();

                Console.Write("Secondary Contact Email (optional): ");
                request.SecondaryContactEmail = Console.ReadLine();

                Console.Write("Valid From (YYYY-MM-DD) [Enter for today]: ");
                var validFromInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(validFromInput) && DateTime.TryParse(validFromInput, out var validFrom))
                {
                    request.ValidFrom = validFrom;
                }

                Console.Write("Valid To (YYYY-MM-DD) [Enter for 1 year]: ");
                var validToInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(validToInput) && DateTime.TryParse(validToInput, out var validTo))
                {
                    request.ValidTo = validTo;
                }
                else
                {
                    request.ValidTo = DateTime.UtcNow.AddYears(1);
                }

                Console.WriteLine("License Tier:");
                Console.WriteLine("1. Community");
                Console.WriteLine("2. Professional");
                Console.WriteLine("3. Enterprise");
                Console.Write("Select tier (1-3): ");
                var tierChoice = Console.ReadLine();
                request.Tier = tierChoice switch
                {
                    "1" => LicenseTier.Community,
                    "2" => LicenseTier.Professional,
                    "3" => LicenseTier.Enterprise,
                    _ => LicenseTier.Community
                };

                if (request.Tier == LicenseTier.Professional || request.Tier == LicenseTier.Enterprise)
                {
                    Console.Write("Max API Calls Per Month (optional): ");
                    var apiCallsInput = Console.ReadLine();
                    if (!string.IsNullOrEmpty(apiCallsInput) && long.TryParse(apiCallsInput, out var apiCalls))
                    {
                        request.MaxApiCallsPerMonth = (int)apiCalls;
                    }

                    Console.Write("Max Concurrent Connections (optional): ");
                    var connectionsInput = Console.ReadLine();
                    if (!string.IsNullOrEmpty(connectionsInput) && int.TryParse(connectionsInput, out var connections))
                    {
                        request.MaxConcurrentConnections = connections;
                    }
                }

                Console.Write("Private Key File Path: ");
                var privateKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(privateKeyPath) || !File.Exists(privateKeyPath))
                {
                    Console.WriteLine("❌ Private key file not found.");
                    return;
                }

                request.PrivateKeyPem = await File.ReadAllTextAsync(privateKeyPath);

                // Generate the license
                var signedLicense = await generator.GenerateLicenseAsync(request);

                // Save to file
                var fileName = $"license_{request.LicensedTo.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.json";
                var licenseJson = JsonSerializer.Serialize(signedLicense, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(fileName, licenseJson);

                Console.WriteLine($"\n✅ License generated successfully!");
                Console.WriteLine($"📁 Saved to: {fileName}");
                Console.WriteLine($"🔑 License ID: {request.LicenseId}");
                Console.WriteLine($"📅 Valid until: {request.ValidTo:yyyy-MM-dd}");
                Console.WriteLine($"🏷️  Tier: {request.Tier}");

                Console.WriteLine("\n📋 License file content:");
                Console.WriteLine(licenseJson);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate license");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void ExtractPublicKeyInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Extract Public Key ===");
                
                Console.Write("Private Key File Path: ");
                var privateKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(privateKeyPath) || !File.Exists(privateKeyPath))
                {
                    Console.WriteLine("❌ Private key file not found.");
                    return;
                }

                var privateKeyPem = File.ReadAllText(privateKeyPath);
                var publicKeyPem = generator.ExtractPublicKeyFromPrivateKey(privateKeyPem);

                var fileName = $"extracted_public_key_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pem";
                File.WriteAllText(fileName, publicKeyPem);

                Console.WriteLine($"\n✅ Public key extracted successfully!");
                Console.WriteLine($"📁 Saved to: {fileName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to extract public key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void ValidatePrivateKeyInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Validate Private Key ===");
                
                Console.Write("Private Key File Path: ");
                var privateKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(privateKeyPath) || !File.Exists(privateKeyPath))
                {
                    Console.WriteLine("❌ Private key file not found.");
                    return;
                }

                var privateKeyPem = File.ReadAllText(privateKeyPath);
                var isValid = generator.ValidatePrivateKey(privateKeyPem);

                Console.WriteLine(isValid ? "✅ Private key is valid!" : "❌ Private key is invalid!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to validate private key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void ValidatePublicKeyInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Validate Public Key ===");
                
                Console.Write("Public Key File Path: ");
                var publicKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(publicKeyPath) || !File.Exists(publicKeyPath))
                {
                    Console.WriteLine("❌ Public key file not found.");
                    return;
                }

                var publicKeyPem = File.ReadAllText(publicKeyPath);
                var isValid = generator.ValidatePublicKey(publicKeyPem);

                Console.WriteLine(isValid ? "✅ Public key is valid!" : "❌ Public key is invalid!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to validate public key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void EncryptPrivateKeyInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Encrypt Private Key ===");
                
                Console.Write("Private Key File Path: ");
                var privateKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(privateKeyPath) || !File.Exists(privateKeyPath))
                {
                    Console.WriteLine("❌ Private key file not found.");
                    return;
                }

                Console.Write("Password for encryption: ");
                var password = ReadPassword();

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("❌ Password is required for encryption.");
                    return;
                }

                var privateKeyPem = File.ReadAllText(privateKeyPath);
                var encryptedKey = generator.EncryptPrivateKey(privateKeyPem, password);

                var fileName = $"encrypted_private_key_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pem";
                File.WriteAllText(fileName, encryptedKey);

                Console.WriteLine($"\n✅ Private key encrypted successfully!");
                Console.WriteLine($"📁 Saved to: {fileName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to encrypt private key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void DecryptPrivateKeyInteractive(ILicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== Decrypt Private Key ===");
                
                Console.Write("Encrypted Private Key File Path: ");
                var encryptedKeyPath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(encryptedKeyPath) || !File.Exists(encryptedKeyPath))
                {
                    Console.WriteLine("❌ Encrypted private key file not found.");
                    return;
                }

                Console.Write("Password for decryption: ");
                var password = ReadPassword();

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("❌ Password is required for decryption.");
                    return;
                }

                var encryptedKeyPem = File.ReadAllText(encryptedKeyPath);
                var decryptedKey = generator.DecryptPrivateKey(encryptedKeyPem, password);

                var fileName = $"decrypted_private_key_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pem";
                File.WriteAllText(fileName, decryptedKey);

                Console.WriteLine($"\n✅ Private key decrypted successfully!");
                Console.WriteLine($"📁 Saved to: {fileName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to decrypt private key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static async Task GenerateKeysFromArgs(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            var keySize = args.Length > 1 && int.TryParse(args[1], out var size) ? size : 2048;
            var result = generator.GenerateKeyPair(keySize);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var privateKeyFile = $"private_key_{timestamp}.pem";
            var publicKeyFile = $"public_key_{timestamp}.pem";

            await File.WriteAllTextAsync(privateKeyFile, result.PrivateKeyPem);
            await File.WriteAllTextAsync(publicKeyFile, result.PublicKeyPem);

            Console.WriteLine($"Generated key pair: {privateKeyFile}, {publicKeyFile}");
        }

        private static async Task GenerateLicenseFromArgs(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            // Command line license generation
            // Usage: generate-license --product-id "PROD123" --product-name "MyProduct" --to "Company" --contact "John" --email "john@company.com" --private-key "path/to/key.pem"
            
            var request = new SimplifiedLicenseGenerationRequest();
            
            for (int i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length) break;
                
                var key = args[i].ToLowerInvariant();
                var value = args[i + 1];
                
                switch (key)
                {
                    case "--product-id":
                        request.ProductId = value;
                        break;
                    case "--product-name":
                        request.ProductName = value;
                        break;
                    case "--to":
                        request.LicensedTo = value;
                        break;
                    case "--contact":
                        request.ContactPerson = value;
                        break;
                    case "--email":
                        request.ContactEmail = value;
                        break;
                    case "--tier":
                        request.Tier = Enum.Parse<LicenseTier>(value, true);
                        break;
                    case "--validto":
                        request.ValidTo = DateTime.Parse(value);
                        break;
                    case "--maxapi":
                        request.MaxApiCallsPerMonth = int.Parse(value);
                        break;
                    case "--maxconn":
                        request.MaxConcurrentConnections = int.Parse(value);
                        break;
                    case "--private-key":
                        if (File.Exists(value))
                        {
                            request.PrivateKeyPem = await File.ReadAllTextAsync(value);
                        }
                        else
                        {
                            throw new FileNotFoundException($"Private key file not found: {value}");
                        }
                        break;
                }
            }

            var signedLicense = await generator.GenerateLicenseAsync(request);
            var licenseJson = JsonSerializer.Serialize(signedLicense, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var fileName = $"license_{request.LicensedTo.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.json";
            await File.WriteAllTextAsync(fileName, licenseJson);

            Console.WriteLine($"License generated: {fileName}");
        }

        private static async Task ExtractPublicKeyFromArgs(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: extract-public <private-key-file> [output-file]");
                return;
            }

            var privateKeyFile = args[1];
            var outputFile = args.Length > 2 ? args[2] : $"extracted_public_key_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pem";

            var privateKeyPem = await File.ReadAllTextAsync(privateKeyFile);
            var publicKeyPem = generator.ExtractPublicKeyFromPrivateKey(privateKeyPem);
            await File.WriteAllTextAsync(outputFile, publicKeyPem);

            Console.WriteLine($"Public key extracted: {outputFile}");
        }

        private static async Task ValidatePrivateKeyFromArgs(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: validate-private <private-key-file>");
                return;
            }

            var privateKeyFile = args[1];
            var privateKeyPem = await File.ReadAllTextAsync(privateKeyFile);
            var isValid = generator.ValidatePrivateKey(privateKeyPem);

            Console.WriteLine($"Private key validation: {(isValid ? "VALID" : "INVALID")}");
        }

        private static async Task ValidatePublicKeyFromArgs(string[] args, ILicenseGenerator generator, ILogger logger)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: validate-public <public-key-file>");
                return;
            }

            var publicKeyFile = args[1];
            var publicKeyPem = await File.ReadAllTextAsync(publicKeyFile);
            var isValid = generator.ValidatePublicKey(publicKeyPem);

            Console.WriteLine($"Public key validation: {(isValid ? "VALID" : "INVALID")}");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("TechWayFit Stateless License Generator");
            Console.WriteLine("Usage:");
            Console.WriteLine("  Interactive mode: TechWayFit.Licensing.Generator.exe");
            Console.WriteLine("  Command line:");
            Console.WriteLine("    generate-keys [key-size]");
            Console.WriteLine("    generate-license --product-id \"PROD123\" --product-name \"MyProduct\" --to \"Company\" --contact \"John\" --email \"john@company.com\" --private-key \"path/to/key.pem\"");
            Console.WriteLine("    extract-public <private-key-file> [output-file]");
            Console.WriteLine("    validate-private <private-key-file>");
            Console.WriteLine("    validate-public <public-key-file>");
        }

        private static string ReadPassword()
        {
            var password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
    }
}
