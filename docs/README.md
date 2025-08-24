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
- ✅ **Generator-Management Integration** with full tier and feature mapping
- ✅ **Cryptographic license generation** with key management service

## Recent Updates ✨

**Generator Integration Completed** - The Management system now fully integrates with the stateless License Generator:

- **Tier Mapping**: Automatically maps Management tier strings ("community", "professional", "enterprise") to Generator LicenseTier enum
- **Feature Mapping**: Converts Management request properties to Generator LicenseFeature objects:
  - `AllowOfflineUsage` → "OfflineUsage" feature
  - `AllowVirtualization` → "Virtualization" feature  
  - `MaxUsers` → "UserLimit" feature with usage limits
  - `MaxDevices` → "DeviceLimit" feature with device limits
  - Custom properties → Dynamic features with appropriate descriptions
- **Cryptographic Security**: Integrates with KeyManagementService for secure private key handling
- **Stateless Design**: Generator operates independently without database dependencies

## Repository Structure

```
twf_license_management/
├── source/                              # Source Code
│   ├── TechWayFit.Licensing.Core/       # Core models and interfaces
│   ├── TechWayFit.Licensing.Generator/  # License generation service
│   ├── TechWayFit.Licensing.Management.Core/ # Management core logic
│   ├── TechWayFit.Licensing.Management.Infrastructure/ # Infrastructure layer
│   ├── TechWayFit.Licensing.Management.Infrastructure.PostgreSql/ # PostgreSQL provider
│   ├── TechWayFit.Licensing.Management.Services/ # Business services
│   ├── TechWayFit.Licensing.Management.Web/ # Web interface
│   ├── TechWayFit.Licensing.Validation/ # License validation service
│   ├── Database/                        # Database scripts and migrations
│   └── TechWayFit.Licensing.sln        # Solution file
├── docs/                               # Documentation
│   ├── TECHNICAL_DESIGN_DOCUMENT.md    # Technical architecture and design
│   ├── FUNCTIONAL_DESIGN_DOCUMENT.md   # Functional requirements and specifications
│   └── UPCOMING_FEATURES_AND_RELEASES.md # Future enhancements and roadmap
├── builds/                             # Build Scripts & CI/CD
│   ├── build.ps1                       # PowerShell build script
│   ├── build.sh                        # Shell build script
│   └── GITHUB_WORKFLOW.yml            # GitHub Actions workflow
├── docker/                             # Docker Containerization
│   ├── Dockerfile                      # Multi-stage Docker build
│   ├── docker-compose.yml             # Development deployment
│   ├── docker-compose.prod.yml        # Production deployment
│   ├── build.sh / build.ps1           # Docker build scripts
│   ├── deploy.sh                       # Deployment automation
│   └── README.md                       # Docker documentation
└── tests/                              # Test Files & Configurations
    ├── test.html                       # Test web page
    ├── create-postgresql-database.sql  # Database setup script
    ├── sample-license-configuration.json # Sample license config
    └── sample-postgresql-configuration.json # Sample DB config
```

## Quick Start

### Docker Deployment (Recommended)

The fastest way to get started is using Docker:

```bash
# Clone the repository
git clone <repository-url>
cd twf_license_management

# Navigate to docker directory
cd docker

# Copy and configure environment
cp .env.template .env
# Edit .env with your configuration

# Build and deploy
./build.sh
./deploy.sh development
```

Access the application at: http://localhost:8080

### Manual Development Setup

1. **Prerequisites**:
   - .NET 8 SDK
   - PostgreSQL 15+
   - Node.js (for frontend assets)

2. **Database Setup**:
   ```bash
   # Create database using provided script
   psql -U postgres -f tests/create-postgresql-database.sql
   ```

3. **Build and Run**:
   ```bash
   cd source
   dotnet restore
   dotnet build
   dotnet run --project TechWayFit.Licensing.Management.Web
   ```

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
