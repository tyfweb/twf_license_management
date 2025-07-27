using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Generator.Services;

namespace TechWayFit.Licensing.Generator
{
    /// <summary>
    /// Console application for generating tamper-proof licenses
    /// This is an internal tool for TechWayFit license management
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
                    services.AddSingleton<LicenseGenerator>();
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var licenseGenerator = host.Services.GetRequiredService<LicenseGenerator>();

            try
            {
                logger.LogInformation("TechWayFit License Generator v1.0");
                logger.LogInformation("========================================");

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

        private static async Task InteractiveMode(LicenseGenerator generator, ILogger logger)
        {
            logger.LogInformation("Running in interactive mode...");
            
            while (true)
            {
                Console.WriteLine("\nTechWayFit License Generator");
                Console.WriteLine("1. Generate New License");
                Console.WriteLine("2. Export Public Key");
                Console.WriteLine("3. Load Private Key");
                Console.WriteLine("4. Save Private Key");
                Console.WriteLine("5. Exit");
                Console.Write("Select option (1-5): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await GenerateLicenseInteractive(generator, logger);
                        break;
                    case "2":
                        ExportPublicKey(generator, logger);
                        break;
                    case "3":
                        await LoadPrivateKeyInteractive(generator, logger);
                        break;
                    case "4":
                        await SavePrivateKeyInteractive(generator, logger);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task CommandLineMode(string[] args, LicenseGenerator generator, ILogger logger)
        {
            // Command line arguments parsing
            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "generate":
                    await GenerateLicenseFromArgs(args, generator, logger);
                    break;
                case "export-key":
                    await ExportPublicKeyFromArgs(args, generator, logger);
                    break;
                case "load-key":
                    await LoadPrivateKeyFromArgs(args, generator, logger);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static async Task GenerateLicenseInteractive(LicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.WriteLine("\n=== License Generation ===");
                
                var request = new LicenseGenerationRequest();

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
                        request.MaxApiCallsPerMonth = (int?)apiCalls;
                    }

                    Console.Write("Max Concurrent Connections (optional): ");
                    var connectionsInput = Console.ReadLine();
                    if (!string.IsNullOrEmpty(connectionsInput) && int.TryParse(connectionsInput, out var connections))
                    {
                        request.MaxConcurrentConnections = connections;
                    }
                }

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
                Console.WriteLine($"🔑 License ID: {GetLicenseIdFromSigned(signedLicense)}");
                Console.WriteLine($"📅 Valid until: {request.ValidTo:yyyy-MM-dd}");
                Console.WriteLine($"🏷️  Tier: {request.Tier}");

                Console.WriteLine("\n📋 Add this to your appsettings.json:");
                Console.WriteLine("\"License\": " + licenseJson);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate license");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static void ExportPublicKey(LicenseGenerator generator, ILogger logger)
        {
            try
            {
                var publicKey = generator.ExportPublicKey();
                var fileName = $"public_key_{DateTime.UtcNow:yyyyMMdd}.pem";
                File.WriteAllText(fileName, publicKey);

                Console.WriteLine($"\n✅ Public key exported successfully!");
                Console.WriteLine($"📁 Saved to: {fileName}");
                Console.WriteLine("\n📋 Use this public key for license validation in your applications.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to export public key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static async Task LoadPrivateKeyInteractive(LicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.Write("Private key file path: ");
                var filePath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    Console.WriteLine("❌ File not found.");
                    return;
                }

                Console.Write("Password (optional): ");
                var password = ReadPassword();

                await generator.LoadPrivateKeyAsync(filePath, password);
                Console.WriteLine("✅ Private key loaded successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load private key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static async Task SavePrivateKeyInteractive(LicenseGenerator generator, ILogger logger)
        {
            try
            {
                Console.Write("Save private key to file: ");
                var filePath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("❌ Invalid file path.");
                    return;
                }

                Console.Write("Password for encryption (optional): ");
                var password = ReadPassword();

                await generator.SavePrivateKeyAsync(filePath, password);
                Console.WriteLine("✅ Private key saved successfully!");
                Console.WriteLine("⚠️  Keep this file secure - it's used to generate licenses!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save private key");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private static async Task GenerateLicenseFromArgs(string[] args, LicenseGenerator generator, ILogger logger)
        {
            // Command line license generation
            // Usage: generate --to "Company Name" --contact "John Doe" --email "john@company.com" --tier Enterprise
            
            var request = new LicenseGenerationRequest();
            
            for (int i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length) break;
                
                var key = args[i].ToLowerInvariant();
                var value = args[i + 1];
                
                switch (key)
                {
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
                        request.MaxApiCallsPerMonth = (int?)long.Parse(value);
                        break;
                    case "--maxconn":
                        request.MaxConcurrentConnections = int.Parse(value);
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

        private static async Task ExportPublicKeyFromArgs(string[] args, LicenseGenerator generator, ILogger logger)
        {
            var fileName = args.Length > 1 ? args[1] : $"public_key_{DateTime.UtcNow:yyyyMMdd}.pem";
            var publicKey = generator.ExportPublicKey();
            await File.WriteAllTextAsync(fileName, publicKey);
            Console.WriteLine($"Public key exported: {fileName}");
        }

        private static async Task LoadPrivateKeyFromArgs(string[] args, LicenseGenerator generator, ILogger logger)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: load-key <file-path> [password]");
                return;
            }

            var filePath = args[1];
            var password = args.Length > 2 ? args[2] : null;

            await generator.LoadPrivateKeyAsync(filePath, password);
            Console.WriteLine("Private key loaded successfully.");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("TechWayFit License Generator");
            Console.WriteLine("Usage:");
            Console.WriteLine("  Interactive mode: TechWayFit.Licensing.Generator.exe");
            Console.WriteLine("  Command line:");
            Console.WriteLine("    generate --to \"Company\" --contact \"John Doe\" --email \"john@company.com\" --tier Enterprise");
            Console.WriteLine("    export-key [filename]");
            Console.WriteLine("    load-key <file-path> [password]");
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

        private static string GetLicenseIdFromSigned(SignedLicense signedLicense)
        {
            try
            {
                var licenseData = Convert.FromBase64String(signedLicense.LicenseData);
                var licenseJson = System.Text.Encoding.UTF8.GetString(licenseData);
                var license = JsonSerializer.Deserialize<License>(licenseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return license?.LicenseId ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
