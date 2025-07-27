# TechWayFit.Licensing.Core

A lean and agnostic .NET 8.0 library for cryptographic license validation. This package provides secure license verification with RSA digital signatures and temporal validation, leaving business logic interpretation to consuming applications.

## 🚀 Quick Start

### Installation

```bash
dotnet add package TechWayFit.Licensing.Core
```

### Basic Usage

```csharp
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Services;
using Microsoft.Extensions.DependencyInjection;

// Register service
services.AddSingleton<ILicenseValidationService, LicenseValidationService>();
services.AddMemoryCache();

// Validate license
var result = await licenseValidationService.ValidateAsync(signedLicense, publicKey);

if (result.IsValid && result.License != null)
{
    // License is valid - implement your business logic
    var features = result.License.FeaturesIncluded;
    Console.WriteLine($"License valid for: {result.License.LicensedTo}");
}
```

## 📋 Core Components

### Services

#### `ILicenseValidationService`

Primary interface for license validation operations.

**Methods:**

```csharp
// Validate a signed license object
Task<LicenseValidationResult> ValidateAsync(
    SignedLicense signedLicense, 
    string publicKey, 
    LicenseValidationOptions? options = null);

// Validate from JSON string
Task<LicenseValidationResult> ValidateFromJsonAsync(
    string licenseJson, 
    string publicKey, 
    LicenseValidationOptions? options = null);

// Validate from file
Task<LicenseValidationResult> ValidateFromFileAsync(
    string filePath, 
    string publicKeyPath, 
    LicenseValidationOptions? options = null);
```

## 📊 Models & Classes

### `SignedLicense`

Represents a cryptographically signed license.

```csharp
public class SignedLicense
{
    public string LicenseData { get; set; }           // Base64-encoded license JSON
    public string Signature { get; set; }             // RSA digital signature
    public string SignatureAlgorithm { get; set; }    // "RS256", "RS512", etc.
    public string PublicKeyThumbprint { get; set; }   // Public key identifier
    public string FormatVersion { get; set; }         // License format version
    public DateTime CreatedAt { get; set; }           // Creation timestamp
    public string? Checksum { get; set; }             // Optional integrity check
}
```

### `LicenseValidationResult`

Contains the result of license validation.

```csharp
public class LicenseValidationResult
{
    public LicenseStatus Status { get; set; }              // Overall validation status
    public License? License { get; set; }                  // Decoded license data (null if invalid)
    public List<string> ValidationMessages { get; set; }   // Detailed validation messages
    public bool IsGracePeriod { get; set; }               // Whether in grace period
    public DateTime? GracePeriodExpiry { get; set; }       // Grace period end date
    public DateTime ValidatedAt { get; set; }              // Validation timestamp
    public bool IsSignatureValid { get; set; }             // Cryptographic validation result
    public bool AreDatesValid { get; set; }               // Temporal validation result
    public List<LicenseFeature> AvailableFeatures { get; set; } // Available features
    public bool IsValid { get; }                           // True if Status == Valid or GracePeriod
}
```

### `License`

Contains the decoded license information.

```csharp
public class License
{
    // Product Information
    public string ProductId { get; set; }
    public ProductType ProductType { get; set; }
    
    // Consumer Information  
    public string ConsumerId { get; set; }
    public string LicensedTo { get; set; }
    public string ContactPerson { get; set; }
    public string ContactEmail { get; set; }
    
    // License Details
    public string LicenseId { get; set; }
    public LicenseTier Tier { get; set; }
    public DateTime IssuedOn { get; set; }
    public DateTime ActivatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
    
    // Features
    public List<LicenseFeature> FeaturesIncluded { get; set; }
    
    // Usage Limits
    public Dictionary<string, object> UsageLimits { get; set; }
}
```

### `LicenseFeature`

Represents an individual feature included in the license.

```csharp
public class LicenseFeature
{
    public string Id { get; set; }                    // Unique feature identifier
    public string Name { get; set; }                  // Feature display name
    public string? Description { get; set; }          // Feature description
    public bool IsCurrentlyValid { get; set; }        // Whether feature is active
    public DateTime AddedOn { get; set; }             // When feature was added
    public DateTime? ExpiresOn { get; set; }          // Feature expiry (optional)
}
```

### `LicenseValidationOptions`

Configuration for validation behavior.

```csharp
public class LicenseValidationOptions
{
    public int GracePeriodDays { get; set; } = 30;         // Grace period after expiry
    public bool AllowGracePeriod { get; set; } = true;     // Enable grace period
    public bool EnableCaching { get; set; } = true;        // Cache validation results
    public int CacheDurationMinutes { get; set; } = 60;    // Cache duration
    public bool ValidateSignature { get; set; } = true;    // Verify cryptographic signature
    public bool ValidateDates { get; set; } = true;        // Check license dates
    public bool EnableAuditLogging { get; set; } = true;   // Log validation attempts
}
```

## 🔧 Enumerations

### `LicenseStatus`

```csharp
public enum LicenseStatus
{
    Valid = 0,              // License is valid and active
    Active = 0,             // Alias for Valid
    Pending = 1,            // Generated but not activated
    Expired = 2,            // Past expiry date
    NotYetValid = 3,        // Future activation date
    Invalid = 4,            // Signature verification failed
    Corrupted = 5,          // Unreadable license data
    NotFound = 6,           // License file missing
    ServiceUnavailable = 7, // Validation service error
    GracePeriod = 8,        // Expired but in grace period
    Revoked = 9,            // Manually invalidated
    Suspended = 10,         // Temporarily disabled
    RenewalPending = 11     // Awaiting renewal
}
```

### `LicenseTier`

```csharp
public enum LicenseTier
{
    Community = 0,      // Basic features
    Professional = 1,   // Advanced features
    Enterprise = 2,     // All features
    Custom = 99         // Custom feature set
}
```

### `FeatureCategory`

```csharp
public enum FeatureCategory
{
    Core = 0,                   // Core functionality
    Security = 1,               // Security features
    Monitoring = 2,             // Analytics and monitoring
    Performance = 3,            // Performance optimization
    Integration = 4,            // Connectivity features
    Management = 5,             // Administration tools
    Developer = 6,              // Developer utilities
    BusinessIntelligence = 7,   // Reporting and BI
    Custom = 99                 // Custom features
}
```

### `ProductType`

```csharp
public enum ProductType
{
    ApiGateway = 0,         // API Gateway product
    Microservice = 1,       // Microservice product
    WebApplication = 2,     // Web application
    DesktopApplication = 3, // Desktop software
    MobileApplication = 4,  // Mobile app
    Library = 5,            // Software library
    Plugin = 6,             // Plugin/extension
    Service = 7,            // Background service
    Custom = 99             // Custom product type
}
```

## 💡 Usage Examples

### 1. Basic License Validation

```csharp
public class LicenseService
{
    private readonly ILicenseValidationService _validator;
    
    public LicenseService(ILicenseValidationService validator)
    {
        _validator = validator;
    }
    
    public async Task<bool> IsLicenseValidAsync(SignedLicense license, string publicKey)
    {
        var result = await _validator.ValidateAsync(license, publicKey);
        return result.IsValid;
    }
}
```

### 2. Feature-Based Validation

```csharp
public class FeatureService
{
    private readonly ILicenseValidationService _validator;
    
    public async Task<bool> HasFeatureAsync(SignedLicense license, string publicKey, string featureName)
    {
        var result = await _validator.ValidateAsync(license, publicKey);
        
        if (!result.IsValid || result.License == null)
            return false;
            
        return result.License.FeaturesIncluded
            .Any(f => f.Name == featureName && f.IsCurrentlyValid);
    }
    
    public async Task<T?> GetUsageLimitAsync<T>(SignedLicense license, string publicKey, string limitName)
    {
        var result = await _validator.ValidateAsync(license, publicKey);
        
        if (!result.IsValid || result.License == null)
            return default(T);
            
        if (result.License.UsageLimits.TryGetValue(limitName, out var limit))
        {
            return (T)Convert.ChangeType(limit, typeof(T));
        }
        
        return default(T);
    }
}
```

### 3. Validation from Configuration

```csharp
public class ConfigurationLicenseService
{
    private readonly ILicenseValidationService _validator;
    private readonly IConfiguration _config;
    
    public async Task<LicenseValidationResult> ValidateAppLicenseAsync()
    {
        var licenseJson = _config["License:SignedLicense"];
        var publicKey = _config["License:PublicKey"];
        
        if (string.IsNullOrEmpty(licenseJson) || string.IsNullOrEmpty(publicKey))
        {
            return LicenseValidationResult.Failure(
                LicenseStatus.NotFound, 
                "License configuration not found");
        }
        
        return await _validator.ValidateFromJsonAsync(licenseJson, publicKey);
    }
}
```

### 4. Validation from Files

```csharp
public class FileLicenseService
{
    private readonly ILicenseValidationService _validator;
    private readonly ILogger<FileLicenseService> _logger;
    
    public FileLicenseService(ILicenseValidationService validator, ILogger<FileLicenseService> logger)
    {
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<LicenseValidationResult> ValidateFromFilesAsync(
        string licenseFilePath, 
        string publicKeyFilePath)
    {
        try
        {
            // Use the built-in file validation method
            return await _validator.ValidateFromFileAsync(licenseFilePath, publicKeyFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license from files");
            return LicenseValidationResult.Failure(
                LicenseStatus.ServiceUnavailable, 
                $"Error reading license files: {ex.Message}");
        }
    }
    
    public async Task<LicenseValidationResult> ValidateFromFilePathsAsync()
    {
        // Default file locations
        var licenseFile = Path.Combine(AppContext.BaseDirectory, "license", "app.license");
        var publicKeyFile = Path.Combine(AppContext.BaseDirectory, "license", "public.key");
        
        return await ValidateFromFilesAsync(licenseFile, publicKeyFile);
    }
    
    // Alternative: Read files manually and use JSON validation
    public async Task<LicenseValidationResult> ValidateFromFileContentsAsync(
        string licenseFilePath, 
        string publicKeyFilePath)
    {
        try
        {
            if (!File.Exists(licenseFilePath))
            {
                return LicenseValidationResult.Failure(
                    LicenseStatus.NotFound, 
                    $"License file not found: {licenseFilePath}");
            }
            
            if (!File.Exists(publicKeyFilePath))
            {
                return LicenseValidationResult.Failure(
                    LicenseStatus.NotFound, 
                    $"Public key file not found: {publicKeyFilePath}");
            }
            
            var licenseJson = await File.ReadAllTextAsync(licenseFilePath);
            var publicKey = await File.ReadAllTextAsync(publicKeyFilePath);
            
            return await _validator.ValidateFromJsonAsync(licenseJson, publicKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading license files");
            return LicenseValidationResult.Failure(
                LicenseStatus.ServiceUnavailable, 
                $"Error reading license files: {ex.Message}");
        }
    }
}
```

### 5. Custom Validation Options

```csharp
public async Task<LicenseValidationResult> ValidateWithCustomOptionsAsync(
    SignedLicense license, 
    string publicKey)
{
    var options = new LicenseValidationOptions
    {
        GracePeriodDays = 7,           // Shorter grace period
        AllowGracePeriod = true,
        EnableCaching = false,          // Disable caching
        ValidateSignature = true,
        ValidateDates = true,
        EnableAuditLogging = false      // Disable audit logging
    };
    
    return await _validator.ValidateAsync(license, publicKey, options);
}
```

### 6. Comprehensive License Check

```csharp
public class LicenseManager
{
    private readonly ILicenseValidationService _validator;
    
    public async Task<LicenseInfo> GetLicenseInfoAsync(SignedLicense license, string publicKey)
    {
        var result = await _validator.ValidateAsync(license, publicKey);
        
        return new LicenseInfo
        {
            IsValid = result.IsValid,
            Status = result.Status,
            LicensedTo = result.License?.LicensedTo ?? "Unknown",
            ExpiresOn = result.License?.ExpiresOn,
            IsInGracePeriod = result.IsGracePeriod,
            AvailableFeatures = result.License?.FeaturesIncluded
                .Where(f => f.IsCurrentlyValid)
                .Select(f => f.Name)
                .ToList() ?? new List<string>(),
            UsageLimits = result.License?.UsageLimits ?? new Dictionary<string, object>(),
            ValidationMessages = result.ValidationMessages
        };
    }
}

public class LicenseInfo
{
    public bool IsValid { get; set; }
    public LicenseStatus Status { get; set; }
    public string LicensedTo { get; set; } = string.Empty;
    public DateTime? ExpiresOn { get; set; }
    public bool IsInGracePeriod { get; set; }
    public List<string> AvailableFeatures { get; set; } = new();
    public Dictionary<string, object> UsageLimits { get; set; } = new();
    public List<string> ValidationMessages { get; set; } = new();
}
```

## 📁 File Structure & Configuration

### Recommended File Structure

```
YourApplication/
├── license/
│   ├── app.license      # Signed license file (JSON format)
│   └── public.key       # RSA public key (PEM format)
├── appsettings.json     # Application configuration
└── Program.cs           # Application entry point
```

### License File Format (app.license)

```json
{
  "licenseData": "eyJsaWNlbnNlSWQiOiJMSUMtMjAyNS0wMDEiLCJwcm9kdWN0SWQiOiJBUElHVy1QUk8i...",
  "signature": "kGp8yF3V4Jm7N2Qr9Sv1Ux8Wz5Bc3Df6Gh4Kl9Mp2Rs6Tv0Yw7Ez1Ac4Bf7Cg8Dh9Ej2...",
  "signatureAlgorithm": "RS256",
  "publicKeyThumbprint": "A1B2C3D4E5F6789012345678901234567890ABCD",
  "formatVersion": "1.0",
  "createdAt": "2025-01-01T00:00:00Z",
  "checksum": "sha256:abc123def456..."
}
```

### Public Key File Format (public.key)

```
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA7LWnGFLzLQ8v3K2Yx9Zn
Qm4Rb8Jc3Vd6Fe9Hg0Ik1Jl2Mn5Op8Qr3St4Uv7Wx0Yz1Az2Bc5Df8Eg9Fh2Ij3K
l4Mp7Nq0Or1Ps2Qt5Ru8Sv9Tw2Ux3Vy4Wz7Xy0Zb1Ac4Ce7Df0Eg3Fh6Gi9Hk2Jl
5Mn8Op1Qr4St7Uv0Wx3Yz6Az9Bc2Ce5Df8Eg1Fh4Gi7Hj0Kl3Mn6Op9Qr2St5Uv
QIDAQAB
-----END PUBLIC KEY-----
```

### Configuration Examples

#### Option 1: File Paths in Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "License": {
    "LicenseFilePath": "license/app.license",
    "PublicKeyFilePath": "license/public.key"
  },
  "LicenseValidation": {
    "GracePeriodDays": 30,
    "EnableCaching": true,
    "CacheDurationMinutes": 60,
    "ValidateSignature": true,
    "ValidateDates": true,
    "EnableAuditLogging": true
  }
}
```

#### Option 2: Embedded Content in Configuration

```json
{
  "License": {
    "SignedLicense": "{\"licenseData\":\"eyJsaWNlbnNlSWQi...\",\"signature\":\"abc123...\"}",
    "PublicKey": "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A...\n-----END PUBLIC KEY-----"
  }
}
```

## ⚙️ Dependency Injection Setup

### ASP.NET Core

```csharp
// Program.cs or Startup.cs
services.AddSingleton<ILicenseValidationService, LicenseValidationService>();
services.AddMemoryCache(); // Required for caching
services.AddLogging();     // Required for logging

// Optional: Configure default validation options
services.Configure<LicenseValidationOptions>(options =>
{
    options.GracePeriodDays = 30;
    options.EnableCaching = true;
    options.CacheDurationMinutes = 60;
});
```

### Console Application

```csharp
var services = new ServiceCollection()
    .AddSingleton<ILicenseValidationService, LicenseValidationService>()
    .AddMemoryCache()
    .AddLogging(builder => builder.AddConsole())
    .BuildServiceProvider();

var validator = services.GetRequiredService<ILicenseValidationService>();
```

## 🔒 Security Considerations

1. **Public Key Storage**: Store RSA public keys securely in your application configuration
2. **Signature Validation**: Always enable signature validation in production (`ValidateSignature = true`)
3. **Date Validation**: Enable date validation to prevent expired license usage (`ValidateDates = true`)
4. **Audit Logging**: Enable audit logging for security monitoring (`EnableAuditLogging = true`)
5. **Cache Security**: Cached validation results are stored in memory only

## 🏗️ Architecture Design

This library follows the **lean and agnostic** principle:

- **Core Responsibility**: Cryptographic validation, date validation, and license data decoding
- **Business Logic**: Left to consuming applications (feature interpretation, usage limits, custom rules)
- **Offline Capability**: Complete validation without external dependencies
- **Caching Support**: Optional result caching for performance
- **Extensible**: Consuming applications implement their own business rules

## 📝 License Format

The library expects licenses in this JSON format (Base64 encoded in `SignedLicense.LicenseData`):

```json
{
  "licenseId": "LIC-2025-001",
  "productId": "APIGW-PRO",
  "productType": 0,
  "consumerId": "CONSUMER-123",
  "licensedTo": "Acme Corporation",
  "contactPerson": "John Doe",
  "contactEmail": "john.doe@acme.com",
  "tier": 1,
  "issuedOn": "2025-01-01T00:00:00Z",
  "activatedOn": "2025-01-01T00:00:00Z",
  "expiresOn": "2025-12-31T23:59:59Z",
  "featuresIncluded": [
    {
      "id": "FEAT-001",
      "name": "Advanced Analytics",
      "description": "Advanced monitoring and analytics",
      "isCurrentlyValid": true,
      "addedOn": "2025-01-01T00:00:00Z"
    }
  ],
  "usageLimits": {
    "maxUsers": 100,
    "maxRequestsPerDay": 1000000,
    "maxEndpoints": 50
  }
}
```

## 📚 Additional Resources

- **NuGet Package**: [TechWayFit.Licensing.Core](https://www.nuget.org/packages/TechWayFit.Licensing.Core)
- **Source Code**: [GitHub Repository](https://github.com/tyfweb/two_license_management)
- **Issues & Support**: [GitHub Issues](https://github.com/tyfweb/two_license_management/issues)

## 📄 License

Copyright © 2025 TechWayFit. All rights reserved.
{
    ProductName = "MyProduct",
    LicenseKey = "generated-license-key",
    ExpirationDate = DateTime.UtcNow.AddYears(1),
    // ... other properties
};
```

## Requirements

- .NET 8.0 or later
- Compatible with TechWayFit licensing ecosystem

## License

MIT License - see LICENSE file for details.

## Support

For support and documentation, visit: https://github.com/tyfweb/two_license_management
