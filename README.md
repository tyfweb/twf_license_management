# TechWayFit Licensing System

## Overview

The TechWayFit Licensing System provides tamper-proof license generation and validation for the API Gateway. It supports tiered licensing (Community, Professional, Enterprise) with feature-based access control.

## Key Features

- ✅ **Tamper-proof licenses** using RSA digital signatures
- ✅ **Tiered licensing** (Community, Professional, Enterprise)
- ✅ **Feature-based access control** with attributes
- ✅ **Grace period support** for expired licenses
- ✅ **Client/Global segregation** from day 1
- ✅ **Usage tracking and compliance reporting**
- ✅ **Caching for performance**

## Project Structure

```
LICENSING/
├── TechWayFit.Licensing.Core/           # Core models and interfaces
│   ├── Models/                          # License, Feature, and validation models
│   └── Services/                        # Service interfaces
├── TechWayFit.Licensing.Generator/      # Internal license generation tool
│   ├── Services/LicenseGenerator.cs     # License generation service
│   └── Program.cs                       # Console application
└── TechWayFit.Licensing.Validation/     # License validation for API/Web
    ├── Services/LicenseValidator.cs     # License validation service
    └── Attributes/FeatureGateAttribute.cs # Controller protection attributes
```

## Quick Start

### 1. Generate a License (Internal Tool)

```bash
# Build the license generator
cd LICENSING/TechWayFit.Licensing.Generator
dotnet build

# Run interactively
dotnet run

# Or use command line
dotnet run -- generate --to "Acme Corp" --contact "John Doe" --email "john@acme.com" --tier Enterprise
```

### 2. Configure API Gateway

Add the generated license to your `appsettings.json`:

```json
{
  "License": {
    "licenseData": "BASE64_ENCODED_LICENSE_DATA",
    "signature": "BASE64_ENCODED_SIGNATURE",
    "signatureAlgorithm": "RS256",
    "publicKeyThumbprint": "KEY_THUMBPRINT",
    "formatVersion": "1.0",
    "createdAt": "2025-07-22T10:00:00Z"
  },
  "LicenseValidation": {
    "PublicKey": "-----BEGIN RSA PUBLIC KEY-----\n...\n-----END RSA PUBLIC KEY-----",
    "GracePeriodDays": 30,
    "AllowGracePeriod": true,
    "EnableCaching": true
  }
}
```

### 3. Protect Controller Actions

```csharp
[RequiresFeature("AdvancedAnalytics", LicenseTier.Professional)]
public class AnalyticsController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        // Only accessible with Professional+ license that includes AdvancedAnalytics
        return Ok();
    }
}

[RequiresLicenseTier(LicenseTier.Enterprise)]
public class AdminController : ControllerBase
{
    // Entire controller requires Enterprise license
}
```

### 4. Register Services

In `Program.cs`:

```csharp
// Add licensing services
builder.Services.AddSingleton<ILicenseValidator, LicenseValidator>();
builder.Services.AddMemoryCache(); // Required for license caching
```

## License Information Structure

Each license contains:

- **LicensedTo**: Organization or person name
- **ContactPerson**: Primary contact
- **ContactEmail**: Primary contact email
- **SecondaryContactPerson**: Optional secondary contact
- **SecondaryContactEmail**: Optional secondary contact email
- **ValidFrom**: License start date
- **ValidTo**: License expiry date
- **FeaturesIncluded**: List of enabled features with limits
- **Tier**: License tier (Community/Professional/Enterprise)
- **Usage Limits**: API calls, connections, data transfer limits

## License Tiers

### Community Tier
- Basic API Gateway functionality
- JWT and API Key authentication
- Simple rate limiting
- 10,000 API calls/month
- 10 concurrent connections

### Professional Tier
- All Community features
- Advanced authentication (OAuth2, SAML, certificates)
- Load balancing strategies
- Request/response transformation
- Basic monitoring and metrics
- 100,000 API calls/month
- 50 concurrent connections

### Enterprise Tier
- All Professional features
- Advanced analytics and reporting
- Multi-region deployment
- Custom policy development
- Advanced security (WAF, DDoS protection)
- Unlimited API calls
- 500+ concurrent connections
- Priority support

## Security Features

### Tamper Protection
- RSA 2048-bit digital signatures
- SHA-256 hash verification
- Base64 encoding for transport
- Checksum validation

### Grace Period
- 30-day grace period after expiry (configurable)
- Gradual feature degradation
- Warning notifications

### Audit Trail
- All license validation attempts logged
- Feature usage tracking
- Compliance reporting
- Usage forecasting

## Client/Global Segregation

The licensing system supports different access patterns from day 1:

### Client Access (Filtered)
- Client-specific API endpoints (`/client/api/*`)
- Filtered feature access based on client license
- Resource quotas per client
- Isolated monitoring and analytics

### Global Access (Full)
- Global admin endpoints (`/admin/api/*`)
- System-wide monitoring
- Cross-client analytics
- License management
- Platform administration

## Usage Examples

### Check Feature Availability

```csharp
public class SomeService
{
    private readonly ILicenseValidator _licenseValidator;

    public async Task<bool> CanUseAdvancedFeature()
    {
        return await _licenseValidator.IsFeatureAvailableAsync("AdvancedAnalytics");
    }

    public async Task<FeatureLimits?> GetRateLimits()
    {
        return await _licenseValidator.GetFeatureLimitsAsync("BasicApiGateway");
    }
}
```

### Validate License Programmatically

```csharp
public class LicenseService
{
    private readonly ILicenseValidator _validator;

    public async Task<LicenseInfo> GetLicenseInfo()
    {
        var validation = await _validator.ValidateFromConfigurationAsync();
        
        if (!validation.IsValid)
        {
            throw new UnauthorizedAccessException($"License invalid: {validation.Status}");
        }

        return new LicenseInfo
        {
            LicensedTo = validation.License.LicensedTo,
            Tier = validation.License.Tier,
            ExpiresOn = validation.License.ValidTo,
            Features = validation.AvailableFeatures,
            IsGracePeriod = validation.IsGracePeriod
        };
    }
}
```

## Deployment Notes

1. **Keep Private Keys Secure**: The private key used for license generation must be stored securely and never distributed.

2. **Public Key Distribution**: Include the public key in the application configuration for license validation.

3. **License Distribution**: Licenses are JSON files that can be distributed via email, download portal, or embedded in installation packages.

4. **Grace Period**: Configure appropriate grace periods to provide good customer experience during license renewals.

5. **Caching**: Enable license validation caching to improve performance, especially for high-traffic applications.

6. **Monitoring**: Implement alerts for license expiry warnings and validation failures.

## Troubleshooting

### Common Issues

1. **"License signature validation failed"**
   - Verify the public key is correctly configured
   - Ensure the license hasn't been modified
   - Check the signature algorithm matches

2. **"Feature not available"**
   - Verify the feature is included in the license
   - Check the license tier supports the feature
   - Ensure the license is still valid

3. **"License service unavailable"**
   - Check the license validation configuration
   - Verify all required dependencies are registered
   - Review application logs for detailed errors

### Debugging

Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "TechWayFit.Licensing": "Debug"
    }
  }
}
```

This will provide detailed information about license validation attempts and feature checks.

## License Generation Workflow

1. **Customer Request**: Customer purchases license or requests trial
2. **Generate License**: Use internal tool to create signed license
3. **Distribute License**: Send license file to customer
4. **Customer Configuration**: Customer adds license to appsettings.json
5. **Validation**: Application validates license on startup and each request
6. **Monitoring**: Track usage and provide renewal notifications

The licensing system ensures your API Gateway can scale from community projects to enterprise deployments while maintaining proper access control and compliance.
