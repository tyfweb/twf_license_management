# TechWayFit.Licensing.Validation

Validation components and attributes for TechWayFit licensing system including MVC integration.

## Features

- **License Validation Attributes**: ASP.NET Core MVC attributes for license validation
- **Validation Services**: Implementation of license validation logic
- **MVC Integration**: Seamless integration with ASP.NET Core applications
- **Caching Support**: Built-in caching for improved performance

## Installation

```bash
dotnet add package TechWayFit.Licensing.Validation
```

## Usage

### Using Validation Attributes

```csharp
using TechWayFit.Licensing.Validation.Attributes;

[LicenseRequired("MyFeature")]
public class ProtectedController : Controller
{
    public IActionResult Index()
    {
        // This action requires a valid license for "MyFeature"
        return View();
    }
}
```

### Manual Validation

```csharp
using TechWayFit.Licensing.Validation.Services;

public class MyService
{
    private readonly ILicenseValidationService _validator;
    
    public MyService(ILicenseValidationService validator)
    {
        _validator = validator;
    }
    
    public async Task<bool> CheckLicense(string feature)
    {
        return await _validator.IsFeatureEnabledAsync(feature);
    }
}
```

### Dependency Injection Setup

```csharp
// In Startup.cs or Program.cs
services.AddLicenseValidation(options =>
{
    options.LicenseFilePath = "path/to/license.lic";
    options.EnableCaching = true;
    options.CacheExpirationMinutes = 60;
});
```

## Requirements

- .NET 8.0 or later
- ASP.NET Core 8.0 or later (for MVC features)
- TechWayFit.Licensing.Core package

## License

MIT License - see LICENSE file for details.

## Support

For support and documentation, visit: https://github.com/tyfweb/two_license_management
