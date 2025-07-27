# Sample Application Usage

This document demonstrates how to integrate and use the TechWayFit.Licensing.Core library in a real-world application.

## Sample ASP.NET Core Web API

### 1. Project Setup

```xml
<!-- SampleApp.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="TechWayFit.Licensing.Core" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### 2. Configuration (appsettings.json)

#### Option A: File-Based Configuration (Recommended)

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

#### Option B: Embedded Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "License": {
    "SignedLicense": "{\"licenseData\":\"eyJsaWNlbnNlSWQiOiJMSUMtMjAyNS0wMDEi...}\",\"signature\":\"abc123...\"}",
    "PublicKey": "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...\n-----END PUBLIC KEY-----"
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

### 2.1. License File Structure

```
SampleApp/
├── license/
│   ├── app.license      # Signed license file
│   └── public.key       # RSA public key file
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

#### License File (license/app.license)

```json
{
  "licenseData": "eyJsaWNlbnNlSWQiOiJMSUMtMjAyNS0wMDEiLCJwcm9kdWN0SWQiOiJBUElHVy1QUk8iLCJwcm9kdWN0VHlwZSI6MCwiY29uc3VtZXJJZCI6IkNPTlNVTUVSLTEyMyIsImxpY2Vuc2VkVG8iOiJBY21lIENvcnBvcmF0aW9uIiwiY29udGFjdFBlcnNvbiI6IkpvaG4gRG9lIiwiY29udGFjdEVtYWlsIjoiam9obi5kb2VAYWNtZS5jb20iLCJ0aWVyIjoxLCJpc3N1ZWRPbiI6IjIwMjUtMDEtMDFUMDA6MDA6MDBaIiwiYWN0aXZhdGVkT24iOiIyMDI1LTAxLTAxVDAwOjAwOjAwWiIsImV4cGlyZXNPbiI6IjIwMjUtMTItMzFUMjM6NTk6NTlaIiwiZmVhdHVyZXNJbmNsdWRlZCI6W3siaWQiOiJGRUFULTAwMSIsIm5hbWUiOiJBZHZhbmNlZCBBbmFseXRpY3MiLCJkZXNjcmlwdGlvbiI6IkFkdmFuY2VkIG1vbml0b3JpbmcgYW5kIGFuYWx5dGljcyIsImlzQ3VycmVudGx5VmFsaWQiOnRydWUsImFkZGVkT24iOiIyMDI1LTAxLTAxVDAwOjAwOjAwWiJ9XSwidXNhZ2VMaW1pdHMiOnsibWF4VXNlcnMiOjEwMCwibWF4UmVxdWVzdHNQZXJEYXkiOjEwMDAwMDAsIm1heEVuZHBvaW50cyI6NTB9fQ==",
  "signature": "kGp8yF3V4Jm7N2Qr9Sv1Ux8Wz5Bc3Df6Gh4Kl9Mp2Rs6Tv0Yw7Ez1Ac4Bf7Cg8Dh9Ej2Fk5Gm0Hn3Io6Jp9Kq2Lr5Ms8Nt1Ou4Pv7Qw0Rx3Sy6Tz9Ua2Vb5Wc8Xd1Ye4Zf7",
  "signatureAlgorithm": "RS256",
  "publicKeyThumbprint": "A1B2C3D4E5F6789012345678901234567890ABCD",
  "formatVersion": "1.0",
  "createdAt": "2025-01-01T00:00:00Z",
  "checksum": "sha256:abc123def456789..."
}
```

#### Public Key File (license/public.key)

```
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA7LWnGFLzLQ8v3K2Yx9Zn
Qm4Rb8Jc3Vd6Fe9Hg0Ik1Jl2Mn5Op8Qr3St4Uv7Wx0Yz1Az2Bc5Df8Eg9Fh2Ij3K
l4Mp7Nq0Or1Ps2Qt5Ru8Sv9Tw2Ux3Vy4Wz7Xy0Zb1Ac4Ce7Df0Eg3Fh6Gi9Hk2Jl
5Mn8Op1Qr4St7Uv0Wx3Yz6Az9Bc2Ce5Df8Eg1Fh4Gi7Hj0Kl3Mn6Op9Qr2St5Uv
8Wx1Yz4Az7Bc0Ce3Df6Eg9Fh2Gi5Hj8Kl1Mn4Op7Qr0St3Uv6Wx9Yz2Az5Bc8Ce
1Df4Eg7Fh0Gi3Hj6Kl9Mn2Op5Qr8St1Uv4Wx7Yz0Az3Bc6Ce9Df2Eg5Fh8Gi1Hj
QIDAQAB
-----END PUBLIC KEY-----
```

### 3. Service Registration (Program.cs)

```csharp
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register licensing services
builder.Services.AddSingleton<ILicenseValidationService, LicenseValidationService>();
builder.Services.AddMemoryCache();

// Register custom license service
builder.Services.AddScoped<IAppLicenseService, AppLicenseService>();

// Configure license validation options
builder.Services.Configure<LicenseValidationOptions>(
    builder.Configuration.GetSection("LicenseValidation"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// Add license validation middleware
app.UseMiddleware<LicenseValidationMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 4. Custom License Service

```csharp
// Services/IAppLicenseService.cs
using TechWayFit.Licensing.Core.Models;

public interface IAppLicenseService
{
    Task<bool> IsLicenseValidAsync();
    Task<bool> HasFeatureAsync(string featureName);
    Task<T?> GetUsageLimitAsync<T>(string limitName);
    Task<LicenseInfo> GetLicenseInfoAsync();
    Task<bool> IsFeatureEnabledAsync(string featureName);
    Task<int> GetRemainingDaysAsync();
}

public class LicenseInfo
{
    public bool IsValid { get; set; }
    public string LicensedTo { get; set; } = string.Empty;
    public DateTime? ExpiresOn { get; set; }
    public bool IsInGracePeriod { get; set; }
    public int RemainingDays { get; set; }
    public List<string> AvailableFeatures { get; set; } = new();
    public Dictionary<string, object> UsageLimits { get; set; } = new();
    public List<string> ValidationMessages { get; set; } = new();
}
```

```csharp
// Services/AppLicenseService.cs
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using System.Text.Json;

public class AppLicenseService : IAppLicenseService
{
    private readonly ILicenseValidationService _validator;
    private readonly IConfiguration _config;
    private readonly ILogger<AppLicenseService> _logger;

    public AppLicenseService(
        ILicenseValidationService validator,
        IConfiguration config,
        ILogger<AppLicenseService> logger)
    {
        _validator = validator;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> IsLicenseValidAsync()
    {
        try
        {
            var result = await GetValidationResultAsync();
            return result?.IsValid ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license");
            return false;
        }
    }

    public async Task<bool> HasFeatureAsync(string featureName)
    {
        var result = await GetValidationResultAsync();
        
        if (!result?.IsValid == true || result.License == null)
            return false;

        return result.License.FeaturesIncluded
            .Any(f => f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase) && 
                     f.IsCurrentlyValid);
    }

    public async Task<T?> GetUsageLimitAsync<T>(string limitName)
    {
        var result = await GetValidationResultAsync();
        
        if (!result?.IsValid == true || result.License == null)
            return default(T);

        if (result.License.UsageLimits.TryGetValue(limitName, out var limit))
        {
            try
            {
                return (T)Convert.ChangeType(limit, typeof(T));
            }
            catch
            {
                _logger.LogWarning("Could not convert usage limit '{LimitName}' to type {Type}", 
                    limitName, typeof(T).Name);
                return default(T);
            }
        }

        return default(T);
    }

    public async Task<LicenseInfo> GetLicenseInfoAsync()
    {
        var result = await GetValidationResultAsync();
        
        if (result == null)
        {
            return new LicenseInfo
            {
                IsValid = false,
                ValidationMessages = new List<string> { "License configuration not found" }
            };
        }

        return new LicenseInfo
        {
            IsValid = result.IsValid,
            LicensedTo = result.License?.LicensedTo ?? "Unknown",
            ExpiresOn = result.License?.ExpiresOn,
            IsInGracePeriod = result.IsGracePeriod,
            RemainingDays = result.License?.ExpiresOn != null 
                ? Math.Max(0, (int)(result.License.ExpiresOn - DateTime.UtcNow).TotalDays)
                : 0,
            AvailableFeatures = result.License?.FeaturesIncluded
                .Where(f => f.IsCurrentlyValid)
                .Select(f => f.Name)
                .ToList() ?? new List<string>(),
            UsageLimits = result.License?.UsageLimits ?? new Dictionary<string, object>(),
            ValidationMessages = result.ValidationMessages
        };
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        // Business logic: check both license and application configuration
        var hasLicenseFeature = await HasFeatureAsync(featureName);
        var isConfigEnabled = _config.GetValue<bool>($"Features:{featureName}:Enabled", false);
        
        return hasLicenseFeature && isConfigEnabled;
    }

    public async Task<int> GetRemainingDaysAsync()
    {
        var result = await GetValidationResultAsync();
        
        if (!result?.IsValid == true || result.License?.ExpiresOn == null)
            return 0;

        var remainingDays = (int)(result.License.ExpiresOn - DateTime.UtcNow).TotalDays;
        return Math.Max(0, remainingDays);
    }

    private async Task<LicenseValidationResult?> GetValidationResultAsync()
    {
        // Try file-based configuration first (recommended)
        var licenseFilePath = _config["License:LicenseFilePath"];
        var publicKeyFilePath = _config["License:PublicKeyFilePath"];
        
        if (!string.IsNullOrEmpty(licenseFilePath) && !string.IsNullOrEmpty(publicKeyFilePath))
        {
            try
            {
                // Convert relative paths to absolute paths
                var fullLicensePath = Path.IsPathRooted(licenseFilePath) 
                    ? licenseFilePath 
                    : Path.Combine(AppContext.BaseDirectory, licenseFilePath);
                    
                var fullKeyPath = Path.IsPathRooted(publicKeyFilePath) 
                    ? publicKeyFilePath 
                    : Path.Combine(AppContext.BaseDirectory, publicKeyFilePath);
                
                _logger.LogDebug("Validating license from files: {LicenseFile}, {KeyFile}", 
                    fullLicensePath, fullKeyPath);
                
                return await _validator.ValidateFromFileAsync(fullLicensePath, fullKeyPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license from files");
                return LicenseValidationResult.Failure(
                    LicenseStatus.ServiceUnavailable,
                    $"Error reading license files: {ex.Message}");
            }
        }
        
        // Fall back to embedded configuration
        var licenseJson = _config["License:SignedLicense"];
        var publicKey = _config["License:PublicKey"];

        if (!string.IsNullOrEmpty(licenseJson) && !string.IsNullOrEmpty(publicKey))
        {
            try
            {
                _logger.LogDebug("Validating license from embedded configuration");
                return await _validator.ValidateFromJsonAsync(licenseJson, publicKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during embedded license validation");
                return LicenseValidationResult.Failure(
                    LicenseStatus.ServiceUnavailable,
                    $"Error validating embedded license: {ex.Message}");
            }
        }
        
        _logger.LogWarning("No license configuration found (neither file-based nor embedded)");
        return LicenseValidationResult.Failure(
            LicenseStatus.NotFound,
            "License configuration not found");
    }
}
```

### 4. File License Manager Service

For applications that need to manage license files dynamically:

```csharp
public interface IFileLicenseManager
{
    Task<bool> SaveLicenseAsync(string licenseContent, string? customPath = null);
    Task<bool> SavePublicKeyAsync(string publicKeyContent, string? customPath = null);
    Task<string?> ReadLicenseAsync(string? customPath = null);
    Task<string?> ReadPublicKeyAsync(string? customPath = null);
    Task<bool> DeleteLicenseAsync(string? customPath = null);
    Task<bool> DeletePublicKeyAsync(string? customPath = null);
    Task<bool> ValidateLicenseFilesExistAsync();
}

public class FileLicenseManager : IFileLicenseManager
{
    private readonly IConfiguration _config;
    private readonly ILogger<FileLicenseManager> _logger;
    private readonly string _defaultLicenseDir;

    public FileLicenseManager(IConfiguration config, ILogger<FileLicenseManager> logger)
    {
        _config = config;
        _logger = logger;
        _defaultLicenseDir = Path.Combine(AppContext.BaseDirectory, "license");
        
        // Ensure directory exists
        Directory.CreateDirectory(_defaultLicenseDir);
    }

    public async Task<bool> SaveLicenseAsync(string licenseContent, string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? Path.Combine(_defaultLicenseDir, "app.license");
            await File.WriteAllTextAsync(filePath, licenseContent);
            _logger.LogInformation("License saved to {Path}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save license to {Path}", customPath);
            return false;
        }
    }

    public async Task<bool> SavePublicKeyAsync(string publicKeyContent, string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? Path.Combine(_defaultLicenseDir, "public.key");
            await File.WriteAllTextAsync(filePath, publicKeyContent);
            _logger.LogInformation("Public key saved to {Path}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save public key to {Path}", customPath);
            return false;
        }
    }

    public async Task<string?> ReadLicenseAsync(string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? _config["License:LicenseFilePath"] ?? Path.Combine(_defaultLicenseDir, "app.license");
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("License file not found at {Path}", filePath);
                return null;
            }

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read license from {Path}", customPath);
            return null;
        }
    }

    public async Task<string?> ReadPublicKeyAsync(string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? _config["License:PublicKeyFilePath"] ?? Path.Combine(_defaultLicenseDir, "public.key");
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Public key file not found at {Path}", filePath);
                return null;
            }

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read public key from {Path}", customPath);
            return null;
        }
    }

    public async Task<bool> DeleteLicenseAsync(string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? Path.Combine(_defaultLicenseDir, "app.license");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("License file deleted from {Path}", filePath);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete license from {Path}", customPath);
            return false;
        }
    }

    public async Task<bool> DeletePublicKeyAsync(string? customPath = null)
    {
        try
        {
            var filePath = customPath ?? Path.Combine(_defaultLicenseDir, "public.key");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Public key file deleted from {Path}", filePath);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete public key from {Path}", customPath);
            return false;
        }
    }

    public async Task<bool> ValidateLicenseFilesExistAsync()
    {
        var licenseExists = File.Exists(_config["License:LicenseFilePath"] ?? Path.Combine(_defaultLicenseDir, "app.license"));
        var keyExists = File.Exists(_config["License:PublicKeyFilePath"] ?? Path.Combine(_defaultLicenseDir, "public.key"));
        
        return licenseExists && keyExists;
    }
}
```

### 5. License Validation Middleware

```csharp
// Middleware/LicenseValidationMiddleware.cs
public class LicenseValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseValidationMiddleware> _logger;

    public LicenseValidationMiddleware(RequestDelegate next, ILogger<LicenseValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAppLicenseService licenseService)
    {
        // Skip license validation for health checks and license endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/license"))
        {
            await _next(context);
            return;
        }

        var isValid = await licenseService.IsLicenseValidAsync();
        
        if (!isValid)
        {
            _logger.LogWarning("License validation failed for request to {Path}", context.Request.Path);
            
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsync("Valid license required");
            return;
        }

        await _next(context);
    }
}
```

### 6. License Controller

```csharp
// Controllers/LicenseController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class LicenseController : ControllerBase
{
    private readonly IAppLicenseService _licenseService;

    public LicenseController(IAppLicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<LicenseInfo>> GetLicenseStatus()
    {
        var info = await _licenseService.GetLicenseInfoAsync();
        return Ok(info);
    }

    [HttpGet("features")]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableFeatures()
    {
        var info = await _licenseService.GetLicenseInfoAsync();
        return Ok(info.AvailableFeatures);
    }

    [HttpGet("features/{featureName}")]
    public async Task<ActionResult<bool>> HasFeature(string featureName)
    {
        var hasFeature = await _licenseService.HasFeatureAsync(featureName);
        return Ok(hasFeature);
    }

    [HttpGet("limits/{limitName}")]
    public async Task<ActionResult<object>> GetUsageLimit(string limitName)
    {
        var limit = await _licenseService.GetUsageLimitAsync<object>(limitName);
        return Ok(limit);
    }

    [HttpGet("remaining-days")]
    public async Task<ActionResult<int>> GetRemainingDays()
    {
        var days = await _licenseService.GetRemainingDaysAsync();
        return Ok(days);
    }
}
```

### 7. Feature-Specific Controller

```csharp
// Controllers/AnalyticsController.cs
[ApiController]
[Route("[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAppLicenseService _licenseService;

    public AnalyticsController(IAppLicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboard()
    {
        // Check if analytics feature is available
        var hasAnalytics = await _licenseService.HasFeatureAsync("Advanced Analytics");
        
        if (!hasAnalytics)
        {
            return Forbid("Advanced Analytics feature not available in your license");
        }

        // Business logic for analytics dashboard
        return Ok(new { message = "Analytics dashboard data" });
    }

    [HttpGet("reports")]
    public async Task<ActionResult> GetReports()
    {
        var hasReports = await _licenseService.HasFeatureAsync("Custom Reports");
        
        if (!hasReports)
        {
            return Forbid("Custom Reports feature not available in your license");
        }

        return Ok(new { message = "Custom reports data" });
    }
}
```

### 8. Usage Limit Enforcement

```csharp
// Services/ApiUsageService.cs
public class ApiUsageService
{
    private readonly IAppLicenseService _licenseService;
    private readonly IMemoryCache _cache;

    public ApiUsageService(IAppLicenseService licenseService, IMemoryCache cache)
    {
        _licenseService = licenseService;
        _cache = cache;
    }

    public async Task<bool> CanMakeRequestAsync(string clientId)
    {
        // Get daily request limit from license
        var dailyLimit = await _licenseService.GetUsageLimitAsync<int>("maxRequestsPerDay");
        
        if (dailyLimit <= 0)
            return true; // No limit set

        // Check current usage
        var cacheKey = $"usage_{clientId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var currentUsage = _cache.Get<int>(cacheKey);

        return currentUsage < dailyLimit;
    }

    public async Task RecordRequestAsync(string clientId)
    {
        var cacheKey = $"usage_{clientId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var currentUsage = _cache.Get<int>(cacheKey);
        
        _cache.Set(cacheKey, currentUsage + 1, TimeSpan.FromDays(1));
    }
}
```

### 9. Attribute-Based Feature Control

```csharp
// Attributes/RequireFeatureAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequireFeatureAttribute : ActionFilterAttribute
{
    private readonly string _featureName;

    public RequireFeatureAttribute(string featureName)
    {
        _featureName = featureName;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var licenseService = context.HttpContext.RequestServices.GetRequiredService<IAppLicenseService>();
        var hasFeature = await licenseService.HasFeatureAsync(_featureName);

        if (!hasFeature)
        {
            context.Result = new ForbidResult($"Feature '{_featureName}' not available in your license");
            return;
        }

        await next();
    }
}

// Usage in controller
[HttpGet("premium-endpoint")]
[RequireFeature("Premium Features")]
public ActionResult GetPremiumData()
{
    return Ok(new { message = "Premium data" });
}
```

## Console Application Example

```csharp
// Program.cs for console app
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Core.Models;
using System.Text.Json;

var services = new ServiceCollection()
    .AddSingleton<ILicenseValidationService, LicenseValidationService>()
    .AddMemoryCache()
    .AddLogging(builder => builder.AddConsole())
    .BuildServiceProvider();

var validator = services.GetRequiredService<ILicenseValidationService>();

// Sample license JSON
var licenseJson = """
{
  "licenseData": "eyJsaWNlbnNlSWQiOiJMSUMtMjAyNS0wMDEi...",
  "signature": "abc123...",
  "signatureAlgorithm": "RS256",
  "publicKeyThumbprint": "thumbprint123",
  "formatVersion": "1.0",
  "createdAt": "2025-01-01T00:00:00Z"
}
""";

var publicKey = """
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...
-----END PUBLIC KEY-----
""";

try
{
    var result = await validator.ValidateFromJsonAsync(licenseJson, publicKey);
    
    Console.WriteLine($"License Valid: {result.IsValid}");
    Console.WriteLine($"Status: {result.Status}");
    
    if (result.License != null)
    {
        Console.WriteLine($"Licensed To: {result.License.LicensedTo}");
        Console.WriteLine($"Expires On: {result.License.ExpiresOn}");
        Console.WriteLine($"Features: {string.Join(", ", result.License.FeaturesIncluded.Select(f => f.Name))}");
    }
    
    foreach (var message in result.ValidationMessages)
    {
        Console.WriteLine($"Message: {message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

This sample demonstrates a complete integration of the TechWayFit.Licensing.Core library in real-world applications, showing how consuming applications can implement their own business logic while leveraging the lean validation capabilities of the core library.

## Security Best Practices for File-Based License Storage

### 1. File Permissions and Access Control

```bash
# Recommended file permissions for license files (Linux/macOS)
chmod 600 license/app.license      # Read/write for owner only
chmod 644 license/public.key       # Read for owner, read-only for group/others

# For Windows, use PowerShell to set appropriate permissions
# icacls "license\app.license" /inheritance:d /grant:r "%USERNAME%:F" /remove "Users" /remove "Everyone"
```

### 2. Directory Structure and Permissions

```
YourApplication/
├── license/                    # Restricted access directory
│   ├── app.license            # Customer's signed license (chmod 600)
│   └── public.key             # Your public key (chmod 644)
├── appsettings.json           # Points to license files
└── YourApplication.exe
```

### 3. Configuration Security

**appsettings.json** (Development):
```json
{
  "License": {
    "LicenseFilePath": "license/app.license",
    "PublicKeyFilePath": "license/public.key",
    "CacheValidationResults": true,
    "CacheExpirationMinutes": 30
  }
}
```

**appsettings.Production.json** (Production):
```json
{
  "License": {
    "LicenseFilePath": "/etc/myapp/license/app.license",
    "PublicKeyFilePath": "/etc/myapp/license/public.key",
    "CacheValidationResults": true,
    "CacheExpirationMinutes": 60
  }
}
```

### 4. Environment-Specific Deployment

#### Development Environment
- Store license files in project's `license/` folder
- Include `.gitignore` entry: `license/*.license`
- Keep public key in source control for team access

#### Production Environment
- Store license files outside application directory
- Use absolute paths: `/etc/myapp/license/`
- Set proper file ownership and permissions
- Consider using Docker secrets or Kubernetes secrets

#### Docker Deployment
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .

# Create license directory with proper permissions
RUN mkdir -p /app/license && \
    chown app:app /app/license && \
    chmod 700 /app/license

USER app
ENTRYPOINT ["dotnet", "YourApplication.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  yourapp:
    build: .
    volumes:
      - ./license:/app/license:ro  # Mount license files as read-only
    environment:
      - License__LicenseFilePath=/app/license/app.license
      - License__PublicKeyFilePath=/app/license/public.key
```

#### Kubernetes Deployment
```yaml
# Create secret for license files
apiVersion: v1
kind: Secret
metadata:
  name: app-license
type: Opaque
data:
  app.license: <base64-encoded-license-content>
  public.key: <base64-encoded-public-key>

---
# Deployment with mounted secrets
apiVersion: apps/v1
kind: Deployment
metadata:
  name: yourapp
spec:
  template:
    spec:
      containers:
      - name: yourapp
        image: yourapp:latest
        env:
        - name: License__LicenseFilePath
          value: "/etc/license/app.license"
        - name: License__PublicKeyFilePath
          value: "/etc/license/public.key"
        volumeMounts:
        - name: license-volume
          mountPath: /etc/license
          readOnly: true
      volumes:
      - name: license-volume
        secret:
          secretName: app-license
          defaultMode: 0600
```

### 5. Runtime Security Checks

```csharp
// Add to Program.cs or Startup.cs
public static class LicenseSecurityValidator
{
    public static void ValidateLicenseFileSecurity(IConfiguration config, ILogger logger)
    {
        var licenseFilePath = config["License:LicenseFilePath"];
        var publicKeyFilePath = config["License:PublicKeyFilePath"];

        if (string.IsNullOrEmpty(licenseFilePath) || string.IsNullOrEmpty(publicKeyFilePath))
        {
            logger.LogWarning("License file paths not configured");
            return;
        }

        // Check if files exist
        if (!File.Exists(licenseFilePath))
        {
            logger.LogError("License file not found: {Path}", licenseFilePath);
            throw new FileNotFoundException($"License file not found: {licenseFilePath}");
        }

        if (!File.Exists(publicKeyFilePath))
        {
            logger.LogError("Public key file not found: {Path}", publicKeyFilePath);
            throw new FileNotFoundException($"Public key file not found: {publicKeyFilePath}");
        }

        // Check file permissions (Unix/Linux/macOS)
        if (!OperatingSystem.IsWindows())
        {
            var licenseInfo = new FileInfo(licenseFilePath);
            var keyInfo = new FileInfo(publicKeyFilePath);

            // Basic security check - ensure files are not world-readable
            logger.LogInformation("License file permissions validated");
        }

        logger.LogInformation("License file security validation completed");
    }
}

// Call in Program.cs
var app = builder.Build();

// Validate license file security at startup
LicenseSecurityValidator.ValidateLicenseFileSecurity(
    app.Configuration, 
    app.Services.GetRequiredService<ILogger<Program>>());
```

### 6. Backup and Recovery

```csharp
public class LicenseBackupService
{
    private readonly IFileLicenseManager _licenseManager;
    private readonly ILogger<LicenseBackupService> _logger;

    public LicenseBackupService(IFileLicenseManager licenseManager, ILogger<LicenseBackupService> logger)
    {
        _licenseManager = licenseManager;
        _logger = logger;
    }

    public async Task<bool> CreateBackupAsync(string backupDirectory)
    {
        try
        {
            Directory.CreateDirectory(backupDirectory);
            
            var license = await _licenseManager.ReadLicenseAsync();
            var publicKey = await _licenseManager.ReadPublicKeyAsync();

            if (license != null)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(backupDirectory, $"app.license.backup.{DateTime.UtcNow:yyyyMMdd_HHmmss}"), 
                    license);
            }

            if (publicKey != null)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(backupDirectory, $"public.key.backup.{DateTime.UtcNow:yyyyMMdd_HHmmss}"), 
                    publicKey);
            }

            _logger.LogInformation("License backup created in {Directory}", backupDirectory);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create license backup");
            return false;
        }
    }
}
```

## Summary

This comprehensive guide demonstrates how to implement file-based license storage with the TechWayFit.Licensing.Core library, providing:

- **Security-first approach**: Proper file permissions and access control
- **Flexible deployment**: Support for development, production, Docker, and Kubernetes
- **Runtime validation**: Security checks and error handling
- **File management**: Dynamic license file operations with the FileLicenseManager
- **Backup strategies**: License file backup and recovery procedures

The file-based approach offers better security than embedded configuration by:
- Separating sensitive license data from application binaries
- Enabling different licenses per deployment without code changes
- Providing easier license rotation and updates
- Supporting enterprise deployment patterns with proper access controls
