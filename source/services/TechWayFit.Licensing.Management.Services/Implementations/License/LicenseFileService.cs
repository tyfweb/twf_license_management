using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of license file service for generating downloadable license files
/// </summary>
public class LicenseFileService : ILicenseFileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LicenseFileService> _logger;

    public LicenseFileService(
        IUnitOfWork unitOfWork,
        ILogger<LicenseFileService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate license file in .lic format (human-readable)
    /// </summary>
    public async Task<string> GenerateLicenseFileAsync(ProductLicense license)
    {
        try
        {
            var content = new StringBuilder();
            
            // Header
            content.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            content.AppendLine("║                    TechWayFit License File                   ║");
            content.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            content.AppendLine();
            
            // License Information
            content.AppendLine("═══ LICENSE INFORMATION ═══");
            content.AppendLine($"License ID      : {license.LicenseId}");
            content.AppendLine($"License Code    : {license.LicenseCode}");
            content.AppendLine($"Status          : {license.Status}");
            content.AppendLine($"Issue Date      : {license.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine($"Valid From      : {license.ValidFrom:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine($"Valid To        : {license.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine($"Issued By       : {license.IssuedBy}");
            content.AppendLine($"File Generated  : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine();

            // Product Information
            content.AppendLine("═══ PRODUCT INFORMATION ═══");
            content.AppendLine($"Product Name    : {license.LicenseConsumer.Product.Name}");
            content.AppendLine($"Product ID      : {license.LicenseConsumer.Product.ProductId}");
            content.AppendLine($"Version         : {license.LicenseConsumer.Product.Version}");
            content.AppendLine($"Description     : {license.LicenseConsumer.Product.Description}");
            content.AppendLine($"Support Email   : {license.LicenseConsumer.Product.SupportEmail}");
            content.AppendLine($"Support Phone   : {license.LicenseConsumer.Product.SupportPhone}");
            content.AppendLine();

            // Licensee Information
            content.AppendLine("═══ LICENSEE INFORMATION ═══");
            content.AppendLine($"Company Name    : {license.LicenseConsumer.Consumer.CompanyName}");
            content.AppendLine($"Consumer ID     : {license.LicenseConsumer.Consumer.ConsumerId}");
            content.AppendLine($"Primary Contact : {license.LicenseConsumer.Consumer.PrimaryContact.Name}");
            content.AppendLine($"Contact Email   : {license.LicenseConsumer.Consumer.PrimaryContact.Email}");
            content.AppendLine($"Contact Phone   : {license.LicenseConsumer.Consumer.PrimaryContact.Phone}");
            if (license.LicenseConsumer.Consumer.SecondaryContact != null)
            {
                content.AppendLine($"Secondary Contact: {license.LicenseConsumer.Consumer.SecondaryContact.Name}");
                content.AppendLine($"Secondary Email : {license.LicenseConsumer.Consumer.SecondaryContact.Email}");
            }
            content.AppendLine();

            // Features Information
            content.AppendLine("═══ LICENSED FEATURES ═══");
            foreach (var feature in license.LicenseConsumer.Features)
            {
                var status = feature.IsEnabled ? "✓ ENABLED" : "✗ DISABLED";
                content.AppendLine($"[{status}] {feature.Name}");
                if (!string.IsNullOrEmpty(feature.Description))
                {
                    content.AppendLine($"           Description: {feature.Description}");
                }
                if (!string.IsNullOrEmpty(feature.Code))
                {
                    content.AppendLine($"           Feature Code: {feature.Code}");
                }
            }
            content.AppendLine();

            // Security Information
            content.AppendLine("═══ SECURITY INFORMATION ═══");
            content.AppendLine($"License Key     : {license.LicenseKey}");
            content.AppendLine($"Digital Signature: {license.LicenseSignature}");
            content.AppendLine($"Public Key      : {(!string.IsNullOrEmpty(license.PublicKey) ? "PRESENT" : "NOT AVAILABLE")}");
            content.AppendLine($"Encryption      : {license.Encryption}");
            content.AppendLine($"Signature Algo  : {license.Signature}");
            content.AppendLine($"Checksum        : {GenerateChecksum(license.LicenseKey)}");
            content.AppendLine();

            // Metadata
            if (license.Metadata.Any())
            {
                content.AppendLine("═══ METADATA ═══");
                foreach (var meta in license.Metadata)
                {
                    content.AppendLine($"{meta.Key}: {meta.Value}");
                }
                content.AppendLine();
            }

            // Usage Terms
            content.AppendLine("═══ USAGE TERMS & CONDITIONS ═══");
            content.AppendLine("• This license file is protected by copyright law and international treaties");
            content.AppendLine("• Unauthorized reproduction, distribution, or modification is strictly prohibited");
            content.AppendLine("• This license is valid only for the specified product version and licensed entity");
            content.AppendLine("• Any attempt to modify or tamper with this license file will invalidate it");
            content.AppendLine("• Contact support before transferring this license to another entity");
            content.AppendLine("• Keep this license file secure and do not share it publicly");
            content.AppendLine();
            
            // Footer
            content.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            content.AppendLine("║  © TechWayFit - All Rights Reserved                          ║");
            content.AppendLine("║  This license file was generated automatically              ║");
            content.AppendLine($"║  Generation Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC          ║");
            content.AppendLine("╚══════════════════════════════════════════════════════════════╝");

            await TrackDownloadAsync(license.LicenseId, "System", "lic");
            return content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating license file for license {LicenseId}", license.LicenseId);
            throw;
        }
    }

    /// <summary>
    /// Generate license file in JSON format (machine-readable)
    /// </summary>
    public async Task<string> GenerateJsonLicenseFileAsync(ProductLicense license)
    {
        try
        {
            var licenseData = new
            {
                LicenseFile = new
                {
                    Format = "TechWayFit-JSON-License",
                    Version = "1.0",
                    GeneratedAt = DateTime.UtcNow,
                    Checksum = GenerateChecksum(license.LicenseKey)
                },
                License = new
                {
                    LicenseId = license.LicenseId,
                    LicenseCode = license.LicenseCode,
                    Status = license.Status.ToString(),
                    IssueDate = license.CreatedAt,
                    ValidFrom = license.ValidFrom,
                    ValidTo = license.ValidTo,
                    IssuedBy = license.IssuedBy,
                    Type = license.LicenseModel.ToString()
                },
                Product = new
                {
                    ProductId = license.LicenseConsumer.Product.ProductId,
                    Name = license.LicenseConsumer.Product.Name,
                    Version = license.LicenseConsumer.Product.Version,
                    Description = license.LicenseConsumer.Product.Description,
                    Support = new
                    {
                        Email = license.LicenseConsumer.Product.SupportEmail,
                        Phone = license.LicenseConsumer.Product.SupportPhone
                    }
                },
                Licensee = new
                {
                    ConsumerId = license.LicenseConsumer.Consumer.ConsumerId,
                    CompanyName = license.LicenseConsumer.Consumer.CompanyName,
                    PrimaryContact = new
                    {
                        Name = license.LicenseConsumer.Consumer.PrimaryContact.Name,
                        Email = license.LicenseConsumer.Consumer.PrimaryContact.Email,
                        Phone = license.LicenseConsumer.Consumer.PrimaryContact.Phone
                    },
                    SecondaryContact = license.LicenseConsumer.Consumer.SecondaryContact != null ? new
                    {
                        Name = license.LicenseConsumer.Consumer.SecondaryContact.Name,
                        Email = license.LicenseConsumer.Consumer.SecondaryContact.Email,
                        Phone = license.LicenseConsumer.Consumer.SecondaryContact.Phone
                    } : null
                },
                Features = license.LicenseConsumer.Features.Select(f => new
                {
                    FeatureId = f.FeatureId,
                    Name = f.Name,
                    Description = f.Description,
                    Code = f.Code,
                    IsEnabled = f.IsEnabled,
                    Category = "Standard" // Default category - future enhancement for feature categorization
                }),
                Security = new
                {
                    LicenseKey = license.LicenseKey,
                    DigitalSignature = license.LicenseSignature,
                    HasPublicKey = !string.IsNullOrEmpty(license.PublicKey),
                    PublicKey = license.PublicKey,
                    Encryption = license.Encryption,
                    SignatureAlgorithm = license.Signature,
                    IntegrityHash = GenerateChecksum($"{license.LicenseKey}{license.LicenseSignature}")
                },
                Metadata = license.Metadata,
                UsageTerms = new
                {
                    Copyright = "This license file is protected by copyright law and international treaties",
                    Restrictions = new[]
                    {
                        "Unauthorized reproduction or distribution is prohibited",
                        "Valid only for specified product version and licensed entity",
                        "Modification or tampering will invalidate the license",
                        "Contact support before transferring license"
                    },
                    Compliance = "Keep secure and do not share publicly"
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await TrackDownloadAsync(license.LicenseId, "System", "json");
            return JsonSerializer.Serialize(licenseData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JSON license file for license {LicenseId}", license.LicenseId);
            throw;
        }
    }

    /// <summary>
    /// Generate license file in XML format (structured)
    /// </summary>
    public async Task<string> GenerateXmlLicenseFileAsync(ProductLicense license)
    {
        try
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("TechWayFitLicense",
                    new XAttribute("version", "1.0"),
                    new XAttribute("format", "XML"),
                    new XAttribute("generatedAt", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                    
                    new XElement("LicenseInfo",
                        new XElement("LicenseId", license.LicenseId),
                        new XElement("LicenseCode", license.LicenseCode),
                        new XElement("Status", license.Status.ToString()),
                        new XElement("IssueDate", license.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                        new XElement("ValidFrom", license.ValidFrom.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                        new XElement("ValidTo", license.ValidTo.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                        new XElement("IssuedBy", license.IssuedBy),
                        new XElement("Type", license.LicenseModel.ToString())
                    ),
                    
                    new XElement("Product",
                        new XElement("ProductId", license.LicenseConsumer.Product.ProductId),
                        new XElement("Name", license.LicenseConsumer.Product.Name),
                        new XElement("Version", license.LicenseConsumer.Product.Version),
                        new XElement("Description", license.LicenseConsumer.Product.Description),
                        new XElement("Support",
                            new XElement("Email", license.LicenseConsumer.Product.SupportEmail),
                            new XElement("Phone", license.LicenseConsumer.Product.SupportPhone)
                        )
                    ),
                    
                    new XElement("Licensee",
                        new XElement("ConsumerId", license.LicenseConsumer.Consumer.ConsumerId),
                        new XElement("CompanyName", license.LicenseConsumer.Consumer.CompanyName),
                        new XElement("PrimaryContact",
                            new XElement("Name", license.LicenseConsumer.Consumer.PrimaryContact.Name),
                            new XElement("Email", license.LicenseConsumer.Consumer.PrimaryContact.Email),
                            new XElement("Phone", license.LicenseConsumer.Consumer.PrimaryContact.Phone)
                        ),
                        license.LicenseConsumer.Consumer.SecondaryContact != null ?
                        new XElement("SecondaryContact",
                            new XElement("Name", license.LicenseConsumer.Consumer.SecondaryContact.Name),
                            new XElement("Email", license.LicenseConsumer.Consumer.SecondaryContact.Email),
                            new XElement("Phone", license.LicenseConsumer.Consumer.SecondaryContact.Phone)
                        ) : null
                    ),
                    
                    new XElement("Features",
                        license.LicenseConsumer.Features.Select(f =>
                            new XElement("Feature",
                                new XAttribute("id", f.FeatureId),
                                new XAttribute("enabled", f.IsEnabled),
                                new XElement("Name", f.Name),
                                new XElement("Description", f.Description),
                                new XElement("Code", f.Code)
                            )
                        )
                    ),
                    
                    new XElement("Security",
                        new XElement("LicenseKey", license.LicenseKey),
                        new XElement("DigitalSignature", license.LicenseSignature),
                        new XElement("PublicKey", license.PublicKey),
                        new XElement("Encryption", license.Encryption),
                        new XElement("SignatureAlgorithm", license.Signature),
                        new XElement("Checksum", GenerateChecksum(license.LicenseKey))
                    ),
                    
                    new XElement("Metadata",
                        license.Metadata.Select(m =>
                            new XElement("Item",
                                new XAttribute("key", m.Key),
                                new XAttribute("value", m.Value?.ToString() ?? "")
                            )
                        )
                    ),
                    
                    new XElement("UsageTerms",
                        new XElement("Copyright", "This license file is protected by copyright law and international treaties"),
                        new XElement("Restrictions",
                            new XElement("Item", "Unauthorized reproduction or distribution is prohibited"),
                            new XElement("Item", "Valid only for specified product version and licensed entity"),
                            new XElement("Item", "Modification or tampering will invalidate the license"),
                            new XElement("Item", "Contact support before transferring license")
                        )
                    )
                )
            );

            await TrackDownloadAsync(license.LicenseId, "System", "xml");
            return xml.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating XML license file for license {LicenseId}", license.LicenseId);
            throw;
        }
    }

    /// <summary>
    /// Generate complete license package as ZIP file
    /// </summary>
    public async Task<byte[]> GenerateLicensePackageAsync(ProductLicense license)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add .lic file
                var licenseContent = await GenerateLicenseFileAsync(license);
                var licenseEntry = archive.CreateEntry($"License_{license.LicenseCode}.lic");
                using (var licenseStream = licenseEntry.Open())
                {
                    var licenseBytes = Encoding.UTF8.GetBytes(licenseContent);
                    await licenseStream.WriteAsync(licenseBytes, 0, licenseBytes.Length);
                }

                // Add .json file
                var jsonContent = await GenerateJsonLicenseFileAsync(license);
                var jsonEntry = archive.CreateEntry($"License_{license.LicenseCode}.json");
                using (var jsonStream = jsonEntry.Open())
                {
                    var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
                    await jsonStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                }

                // Add .xml file
                var xmlContent = await GenerateXmlLicenseFileAsync(license);
                var xmlEntry = archive.CreateEntry($"License_{license.LicenseCode}.xml");
                using (var xmlStream = xmlEntry.Open())
                {
                    var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
                    await xmlStream.WriteAsync(xmlBytes, 0, xmlBytes.Length);
                }

                // Add README file
                var readmeContent = await GenerateReadmeAsync(license);
                var readmeEntry = archive.CreateEntry("README.txt");
                using (var readmeStream = readmeEntry.Open())
                {
                    var readmeBytes = Encoding.UTF8.GetBytes(readmeContent);
                    await readmeStream.WriteAsync(readmeBytes, 0, readmeBytes.Length);
                }

                // Add public key file if available
                if (!string.IsNullOrEmpty(license.PublicKey))
                {
                    var keyEntry = archive.CreateEntry($"PublicKey_{license.LicenseCode}.pem");
                    using (var keyStream = keyEntry.Open())
                    {
                        var keyBytes = Encoding.UTF8.GetBytes(license.PublicKey);
                        await keyStream.WriteAsync(keyBytes, 0, keyBytes.Length);
                    }
                }
            }

            await TrackDownloadAsync(license.LicenseId, "System", "zip");
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating license package for license {LicenseId}", license.LicenseId);
            throw;
        }
    }

    /// <summary>
    /// Generate README file for license package
    /// </summary>
    public Task<string> GenerateReadmeAsync(ProductLicense license)
    {
        var content = new StringBuilder();
        
        content.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        content.AppendLine("║                TechWayFit License Package                    ║");
        content.AppendLine("╚══════════════════════════════════════════════════════════════╝");
        content.AppendLine();
        
        content.AppendLine("PACKAGE INFORMATION");
        content.AppendLine("===================");
        content.AppendLine($"License ID      : {license.LicenseId}");
        content.AppendLine($"License Code    : {license.LicenseCode}");
        content.AppendLine($"Product         : {license.LicenseConsumer.Product.Name}");
        content.AppendLine($"Licensed To     : {license.LicenseConsumer.Consumer.CompanyName}");
        content.AppendLine($"Package Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine();
        
        content.AppendLine("PACKAGE CONTENTS");
        content.AppendLine("================");
        content.AppendLine();
        content.AppendLine($"1. License_{license.LicenseCode}.lic");
        content.AppendLine("   └─ Human-readable license file format");
        content.AppendLine("   └─ Use for manual license verification");
        content.AppendLine("   └─ Contains all license details in plain text");
        content.AppendLine();
        content.AppendLine($"2. License_{license.LicenseCode}.json");
        content.AppendLine("   └─ JSON format for API integrations");
        content.AppendLine("   └─ Machine-readable structured data");
        content.AppendLine("   └─ Ideal for automated license validation");
        content.AppendLine();
        content.AppendLine($"3. License_{license.LicenseCode}.xml");
        content.AppendLine("   └─ XML format for enterprise systems");
        content.AppendLine("   └─ Structured markup for complex integrations");
        content.AppendLine("   └─ Standards-compliant format");
        content.AppendLine();
        if (!string.IsNullOrEmpty(license.PublicKey))
        {
            content.AppendLine($"4. PublicKey_{license.LicenseCode}.pem");
            content.AppendLine("   └─ RSA public key for signature verification");
            content.AppendLine("   └─ Use to validate license authenticity");
            content.AppendLine("   └─ Distribute with your application");
            content.AppendLine();
        }
        content.AppendLine("5. README.txt (this file)");
        content.AppendLine("   └─ Package documentation and usage instructions");
        content.AppendLine();
        
        content.AppendLine("USAGE INSTRUCTIONS");
        content.AppendLine("==================");
        content.AppendLine();
        content.AppendLine("1. EXTRACT FILES");
        content.AppendLine("   └─ Extract all files to your application directory");
        content.AppendLine("   └─ Keep files together in same folder");
        content.AppendLine();
        content.AppendLine("2. CHOOSE FORMAT");
        content.AppendLine("   └─ .lic file  : Traditional desktop applications");
        content.AppendLine("   └─ .json file : Web applications and APIs");
        content.AppendLine("   └─ .xml file  : Enterprise and legacy systems");
        content.AppendLine();
        content.AppendLine("3. IMPLEMENT VALIDATION");
        content.AppendLine("   └─ Use TechWayFit License Validation SDK");
        content.AppendLine("   └─ Verify digital signature with public key");
        content.AppendLine("   └─ Check license expiration and status");
        content.AppendLine();
        content.AppendLine("4. SECURITY BEST PRACTICES");
        content.AppendLine("   └─ Keep license files secure");
        content.AppendLine("   └─ Do not share publicly or commit to version control");
        content.AppendLine("   └─ Validate signature on every application startup");
        content.AppendLine("   └─ Handle license expiration gracefully");
        content.AppendLine();
        
        content.AppendLine("INTEGRATION EXAMPLES");
        content.AppendLine("====================");
        content.AppendLine();
        content.AppendLine("C# Example:");
        content.AppendLine("```csharp");
        content.AppendLine("var licenseValidator = new TechWayFitLicenseValidator();");
        content.AppendLine($"var result = licenseValidator.ValidateFromFile(\"License_{license.LicenseCode}.json\");");
        content.AppendLine("if (result.IsValid) { /* License is valid */ }");
        content.AppendLine("```");
        content.AppendLine();
        content.AppendLine("JavaScript Example:");
        content.AppendLine("```javascript");
        content.AppendLine($"const fs = require('fs');");
        content.AppendLine($"const license = JSON.parse(fs.readFileSync('License_{license.LicenseCode}.json'));");
        content.AppendLine("// Implement your validation logic");
        content.AppendLine("```");
        content.AppendLine();
        
        content.AppendLine("SUPPORT INFORMATION");
        content.AppendLine("===================");
        content.AppendLine();
        if (!string.IsNullOrEmpty(license.LicenseConsumer.Product.SupportEmail))
        {
            content.AppendLine($"Technical Support : {license.LicenseConsumer.Product.SupportEmail}");
        }
        if (!string.IsNullOrEmpty(license.LicenseConsumer.Product.SupportPhone))
        {
            content.AppendLine($"Phone Support     : {license.LicenseConsumer.Product.SupportPhone}");
        }
        content.AppendLine($"License Support   : licenses@techway.fit");
        content.AppendLine($"Documentation     : https://docs.techway.fit/licensing");
        content.AppendLine();
        
        content.AppendLine("IMPORTANT NOTICES");
        content.AppendLine("=================");
        content.AppendLine();
        content.AppendLine("⚠️  SECURITY WARNING");
        content.AppendLine("    • This license package contains sensitive information");
        content.AppendLine("    • Protect these files from unauthorized access");
        content.AppendLine("    • Do not share license files publicly");
        content.AppendLine();
        content.AppendLine("📋  LICENSE TERMS");
        content.AppendLine("    • Valid only for specified product and entity");
        content.AppendLine("    • Contact support before transferring license");
        content.AppendLine("    • Modification will invalidate the license");
        content.AppendLine();
        content.AppendLine("🔒  COPYRIGHT NOTICE");
        content.AppendLine("    • Protected by international copyright law");
        content.AppendLine("    • Unauthorized distribution is prohibited");
        content.AppendLine("    • © TechWayFit - All Rights Reserved");
        content.AppendLine();
        
        content.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        content.AppendLine("║  Thank you for choosing TechWayFit Licensing Solution       ║");
        content.AppendLine($"║  Package Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC               ║");
        content.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return Task.FromResult(content.ToString());
    }

    /// <summary>
    /// Generate bulk export for multiple licenses
    /// </summary>
    public async Task<byte[]> GenerateBulkExportAsync(IEnumerable<ProductLicense> licenses, string format = "zip")
    {
        try
        {
            var licenseList = licenses.ToList();
            _logger.LogInformation("Generating bulk export for {Count} licenses in format {Format}", licenseList.Count, format);

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add bulk export info
                var exportInfo = new StringBuilder();
                exportInfo.AppendLine("TechWayFit Bulk License Export");
                exportInfo.AppendLine("==============================");
                exportInfo.AppendLine($"Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                exportInfo.AppendLine($"Total Licenses: {licenseList.Count}");
                exportInfo.AppendLine($"Export Format: {format.ToUpper()}");
                exportInfo.AppendLine();
                exportInfo.AppendLine("Included Licenses:");
                
                foreach (var license in licenseList)
                {
                    exportInfo.AppendLine($"- {license.LicenseCode} | {license.LicenseConsumer.Product.Name} | {license.LicenseConsumer.Consumer.CompanyName}");
                }

                var infoEntry = archive.CreateEntry("BulkExport_Info.txt");
                using (var infoStream = infoEntry.Open())
                {
                    var infoBytes = Encoding.UTF8.GetBytes(exportInfo.ToString());
                    await infoStream.WriteAsync(infoBytes, 0, infoBytes.Length);
                }

                // Add each license in a separate folder
                foreach (var license in licenseList)
                {
                    var folderName = $"License_{license.LicenseCode}";
                    
                    switch (format.ToLower())
                    {
                        case "json":
                            var jsonContent = await GenerateJsonLicenseFileAsync(license);
                            var jsonEntry = archive.CreateEntry($"{folderName}/License_{license.LicenseCode}.json");
                            using (var jsonStream = jsonEntry.Open())
                            {
                                var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
                                await jsonStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                            }
                            break;

                        case "xml":
                            var xmlContent = await GenerateXmlLicenseFileAsync(license);
                            var xmlEntry = archive.CreateEntry($"{folderName}/License_{license.LicenseCode}.xml");
                            using (var xmlStream = xmlEntry.Open())
                            {
                                var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
                                await xmlStream.WriteAsync(xmlBytes, 0, xmlBytes.Length);
                            }
                            break;

                        default: // zip or full package
                            var packageBytes = await GenerateLicensePackageAsync(license);
                            var packageEntry = archive.CreateEntry($"{folderName}/License_{license.LicenseCode}_Package.zip");
                            using (var packageStream = packageEntry.Open())
                            {
                                await packageStream.WriteAsync(packageBytes, 0, packageBytes.Length);
                            }
                            break;
                    }
                }
            }

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk export for {Count} licenses", licenses.Count());
            throw;
        }
    }

    /// <summary>
    /// Track license file download
    /// </summary>
    public async Task<bool> TrackDownloadAsync(Guid licenseId, string downloadedBy, string format)
    {
        try
        {
            // Create audit entry for download tracking
            var auditEntry = new LicenseAuditEntry
            {
                EntryId = Guid.NewGuid().ToString(),
                LicenseId = licenseId.ToString(),
                Action = $"License Downloaded ({format.ToUpper()})",
                ModifiedBy = downloadedBy,
                ModifiedDate = DateTime.UtcNow,
                OldValue = null,
                NewValue = $"Downloaded in {format} format",
                Reason = $"License file downloaded via API in {format} format",
                Metadata = new Dictionary<string, object>
                {
                    { "DownloadFormat", format },
                    { "DownloadTimestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "DownloadSource", "API" }
                }
            };

            // Log the download action
            _logger.LogInformation("License {LicenseId} downloaded by {User} in format {Format}", 
                licenseId, downloadedBy, format);

            // Note: In a full implementation, this would save the audit entry to the database
            // For now, we're using logging as the tracking mechanism
            await Task.CompletedTask;
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking download for license {LicenseId}", licenseId);
            return false;
        }
    }

    /// <summary>
    /// Get download statistics for a license
    /// </summary>
    public async Task<LicenseDownloadStats> GetDownloadStatsAsync(Guid licenseId)
    {
        try
        {
            // For now, return basic stats - in full implementation this would query audit entries
            // This could query audit logs or a dedicated download tracking table
            _logger.LogInformation("Retrieving download statistics for license {LicenseId}", licenseId);
            
            await Task.CompletedTask; // Placeholder for async operations
            
            return new LicenseDownloadStats
            {
                LicenseId = licenseId,
                TotalDownloads = 0, // Would be calculated from audit entries
                LastDownload = null, // Would be from latest audit entry
                LastDownloadedBy = "",
                FormatDownloads = new Dictionary<string, int>
                {
                    { "lic", 0 },
                    { "json", 0 },
                    { "xml", 0 },
                    { "zip", 0 }
                },
                DownloadHistory = new List<LicenseDownloadRecord>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download stats for license {LicenseId}", licenseId);
            throw;
        }
    }

    /// <summary>
    /// Validate license file format and content
    /// </summary>
    public async Task<LicenseFileValidationResult> ValidateLicenseFileAsync(string fileContent, string format)
    {
        try
        {
            var result = new LicenseFileValidationResult();
            
            switch (format.ToLower())
            {
                case "json":
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(fileContent);
                        result.IsValid = true;
                        result.Metadata = new LicenseFileMetadata
                        {
                            Format = "JSON",
                            Version = "1.0",
                            GeneratedAt = DateTime.UtcNow,
                            FileSize = Encoding.UTF8.GetByteCount(fileContent),
                            Checksum = GenerateChecksum(fileContent)
                        };
                    }
                    catch (JsonException ex)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Invalid JSON format: {ex.Message}";
                    }
                    break;

                case "xml":
                    try
                    {
                        var xmlDoc = XDocument.Parse(fileContent);
                        result.IsValid = true;
                        result.Metadata = new LicenseFileMetadata
                        {
                            Format = "XML",
                            Version = "1.0",
                            GeneratedAt = DateTime.UtcNow,
                            FileSize = Encoding.UTF8.GetByteCount(fileContent),
                            Checksum = GenerateChecksum(fileContent)
                        };
                    }
                    catch (Exception ex)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Invalid XML format: {ex.Message}";
                    }
                    break;

                default:
                    result.IsValid = !string.IsNullOrEmpty(fileContent);
                    result.Metadata = new LicenseFileMetadata
                    {
                        Format = "LIC",
                        Version = "1.0",
                        GeneratedAt = DateTime.UtcNow,
                        FileSize = Encoding.UTF8.GetByteCount(fileContent),
                        Checksum = GenerateChecksum(fileContent)
                    };
                    break;
            }

            await Task.CompletedTask; // Placeholder for async operations
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license file in format {Format}", format);
            return new LicenseFileValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Validation error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generate SHA256 checksum for data integrity
    /// </summary>
    private static string GenerateChecksum(string data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
