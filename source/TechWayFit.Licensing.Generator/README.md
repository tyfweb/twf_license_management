# TechWayFit License Generator

A **stateless**, command-line tool for generating cryptographic key pairs and signed licenses for the TechWayFit licensing system. This tool focuses purely on cryptographic operations without storing any data.

## Key Features

- **Stateless Operation**: No data storage - all inputs provided, all outputs returned
- **RSA Key Generation**: Generate secure public/private key pairs (2048-bit to 4096-bit)
- **License Signing**: Create tamper-proof signed licenses using RSA-SHA256
- **Key Management**: Validate, encrypt, and decrypt private keys with password protection
- **Multiple Interfaces**: Interactive mode and comprehensive command-line interface
- **Production Ready**: Optimized for CI/CD pipelines and automated workflows
- **Secure**: Follows cryptographic best practices with proper error handling

## Architecture

This tool is designed to be:

- **Stateless**: No persistence layer, configuration files, or internal state
- **Focused**: Single responsibility - cryptographic operations only
- **Integrable**: Easy to integrate into management systems and automation workflows
- **Secure**: Follows NIST cryptographic standards and best practices
- **Testable**: Pure functions with predictable outputs and comprehensive error handling

## Usage

### Interactive Mode

Run the application without arguments to enter interactive mode with a full menu:

```bash
dotnet run
```

**Interactive Menu Options:**
1. Generate New Key Pair
2. Generate License (requires private key)
3. Extract Public Key from Private Key
4. Validate Private Key
5. Validate Public Key  
6. Encrypt Private Key
7. Decrypt Private Key
8. Exit

### Command Line Interface

#### Generate Key Pair
```bash
# Generate 2048-bit key pair (default)
dotnet run generate-keys

# Generate 4096-bit key pair (recommended for production)
dotnet run generate-keys 4096
```

**Output:**
- `private_key_YYYYMMDD_HHMMSS.pem` - Private key for signing licenses
- `public_key_YYYYMMDD_HHMMSS.pem` - Public key for license validation

#### Generate License
```bash
dotnet run generate-license \
  --product-id "PROD123" \
  --product-name "MyProduct" \
  --to "Company Name" \
  --contact "John Doe" \
  --email "john@company.com" \
  --tier "Enterprise" \
  --private-key "path/to/private_key.pem"
```

**Additional Options:**
- `--validto "2025-12-31"` - License expiration date
- `--maxapi 1000000` - Maximum API calls per month
- `--maxconn 100` - Maximum concurrent connections
- `--secondary-contact "Jane Doe"` - Secondary contact person
- `--secondary-email "jane@company.com"` - Secondary contact email

**Supported Tiers:**
- `Community` - Basic features
- `Professional` - Enhanced features with usage limits
- `Enterprise` - Full features with high limits

#### Extract Public Key from Private Key
```bash
# Extract to auto-generated filename
dotnet run extract-public private_key.pem

# Extract to specific filename
dotnet run extract-public private_key.pem public_key.pem
```

#### Validate Keys
```bash
# Validate private key
dotnet run validate-private private_key.pem

# Validate public key
dotnet run validate-public public_key.pem
```

## Key Management & Security

### Security Best Practices

1. **Private Key Storage**: Store private keys securely, preferably encrypted with strong passwords
2. **Key Rotation**: Generate new key pairs periodically (recommended: every 2-3 years)
3. **Access Control**: Limit access to private keys to authorized personnel only
4. **Environment Separation**: Use different key pairs for development, staging, and production
5. **Backup Strategy**: Maintain secure, encrypted backups of private keys
6. **Audit Trail**: Log all key generation and license creation activities

### Key Generation Recommendations

- **Development/Testing**: 2048-bit keys are sufficient for performance
- **Production**: Use 4096-bit keys for enhanced security and future-proofing
- **High Security Environments**: Consider hardware security modules (HSMs) or Azure Key Vault
- **Compliance**: Ensure key generation meets your industry's regulatory requirements

### Private Key Protection

#### Encryption Commands
```bash
# Interactive mode - encrypt a private key
dotnet run
# Select option 6: Encrypt Private Key

# Decrypt a private key
dotnet run  
# Select option 7: Decrypt Private Key
```

#### Storage Best Practices
1. **Never embed private keys** in source code, configuration files, or Docker images
2. **Use environment variables** or secure key management systems (Azure Key Vault, AWS KMS)
3. **Encrypt private keys** with strong passwords before storage
4. **Implement proper access controls** with role-based permissions
5. **Monitor and audit** all private key usage and access

## License Generation Workflow

### Integration with Management Systems

The License Generator is designed to integrate seamlessly with your existing management systems:

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Management    │    │    License       │    │   Customer      │
│     System      │───▶│   Generator      │───▶│   Application   │
│                 │    │   (Stateless)    │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
        │                        ▲                        │
        │                        │                        │
        ▼                        │                        ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Product &     │    │   Private Key    │    │    License      │
│   Customer DB   │    │   Management     │    │   Validation    │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Recommended Workflow

1. **Product Configuration**: Configure products, features, and pricing tiers in your management system
2. **Customer Management**: Create and manage customer accounts, subscriptions, and billing
3. **License Request Processing**:
   - Validate customer subscription status
   - Determine appropriate license features and limits
   - Retrieve the secure private key for the product
4. **License Generation**: Call the generator with complete request data
5. **License Storage**: Store the generated license in your customer database
6. **License Distribution**: Provide the license file to your customer via secure channels

### Example Integration Code

```csharp
// In your management system service
public class LicenseManagementService
{
    private readonly ILicenseGenerator _generator;
    private readonly IKeyManagementService _keyManager;
    private readonly ICustomerRepository _customerRepo;
    
    public async Task<SignedLicense> CreateLicenseAsync(
        string customerId, 
        string productId, 
        LicenseSubscription subscription)
    {
        // Retrieve customer and product information
        var customer = await _customerRepo.GetAsync(customerId);
        var product = await _productRepo.GetAsync(productId);
        
        // Get the private key for this product
        var privateKey = await _keyManager.GetPrivateKeyAsync(productId);
        
        // Prepare the license request
        var request = new SimplifiedLicenseGenerationRequest
        {
            ProductId = product.Id,
            ProductName = product.Name,
            LicensedTo = customer.CompanyName,
            ContactEmail = customer.PrimaryEmail,
            ContactPerson = customer.PrimaryContact,
            Tier = subscription.Tier,
            ValidFrom = subscription.StartDate,
            ValidTo = subscription.EndDate,
            MaxApiCallsPerMonth = subscription.ApiLimit,
            MaxConcurrentConnections = subscription.ConnectionLimit,
            PrivateKeyPem = privateKey, // Retrieved securely
            Features = product.Features.ToLicenseFeatures(),
            CustomData = subscription.GetCustomMetadata()
        };
        
        // Generate the license
        var signedLicense = await _generator.GenerateLicenseAsync(request);
        
        // Store in database
        await _customerRepo.SaveLicenseAsync(customerId, signedLicense);
        
        // Audit logging
        _logger.LogInformation("License generated for customer {CustomerId}, product {ProductId}", 
            customerId, productId);
            
        return signedLicense;
    }
}
```

## Output Formats

### Key Pair Generation

When generating key pairs, the tool creates timestamped files:

```
private_key_20250727_143052.pem  # Private key for signing (keep secure!)
public_key_20250727_143052.pem   # Public key for validation (distribute freely)
```

**Private Key Format** (PKCS#1 PEM):
```
-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEA1234567890abcdef...
[Base64 encoded private key data]
-----END RSA PRIVATE KEY-----
```

**Public Key Format** (PKCS#1 PEM):
```
-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEA1234567890abcdef...
[Base64 encoded public key data]
-----END RSA PUBLIC KEY-----
```

### License Generation

Generated licenses are returned in a standardized JSON format:

```json
{
  "licenseData": "eyJsaWNlbnNlSWQiOiI...",
  "signature": "ABC123DEF456GHI789...",
  "signatureAlgorithm": "RS256",
  "publicKeyThumbprint": "SHA256:1a2b3c4d5e6f...",
  "formatVersion": "1.0",
  "createdAt": "2025-07-27T14:30:52.123Z",
  "checksum": "abc123def456..."
}
```

**Field Descriptions:**
- `licenseData`: Base64-encoded license content (JSON)
- `signature`: RSA-SHA256 digital signature
- `signatureAlgorithm`: Always "RS256" for this version
- `publicKeyThumbprint`: SHA256 fingerprint of the public key
- `formatVersion`: License format version for compatibility
- `createdAt`: UTC timestamp of license creation
- `checksum`: Integrity checksum for additional validation

### Decoded License Data

When the `licenseData` is decoded, it contains:

```json
{
  "licenseId": "550e8400-e29b-41d4-a716-446655440000",
  "licensedTo": "Acme Corporation",
  "contactPerson": "John Doe",
  "contactEmail": "john.doe@acme.com",
  "validFrom": "2025-01-01T00:00:00Z",
  "validTo": "2025-12-31T23:59:59Z",
  "tier": "Enterprise",
  "maxApiCallsPerMonth": 1000000,
  "maxConcurrentConnections": 100,
  "featuresIncluded": [
    { "name": "API_ACCESS", "enabled": true },
    { "name": "PREMIUM_SUPPORT", "enabled": true }
  ],
  "issuedAt": "2025-07-27T14:30:52.123Z",
  "metadata": {
    "environment": "production",
    "salesOrderId": "SO-2025-001"
  }
}
```

## Error Handling & Troubleshooting

### Common Error Scenarios

#### Invalid Private Key
```bash
❌ Error: Invalid private key provided
```
**Solution**: Verify the private key file exists and is in valid PEM format

#### Missing Required Fields
```bash
❌ Error: Invalid license generation request: ProductId is required, ContactEmail is required
```
**Solution**: Provide all required fields for license generation

#### File Not Found
```bash
❌ Error: The system cannot find the file specified
```
**Solution**: Check file paths and permissions

#### Cryptographic Errors
```bash
❌ Error: The provided key is not a valid RSA private key
```
**Solution**: Regenerate the key pair or verify key format

### Validation Commands

Use these commands to diagnose issues:

```bash
# Test if a private key is valid and can sign data
dotnet run validate-private my_private_key.pem

# Test if a public key is valid and properly formatted  
dotnet run validate-public my_public_key.pem

# Extract public key to verify private key works
dotnet run extract-public my_private_key.pem test_public.pem
```

### Exit Codes

- **0**: Success
- **1**: General error (invalid arguments, file not found)
- **2**: Cryptographic error (invalid keys, signing failure)
- **3**: Validation error (missing required fields)

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Generate Production License

on:
  workflow_dispatch:
    inputs:
      customer_name:
        description: 'Customer Name'
        required: true
      customer_email:
        description: 'Customer Email'
        required: true
      license_tier:
        description: 'License Tier'
        required: true
        type: choice
        options:
        - Community
        - Professional  
        - Enterprise

jobs:
  generate-license:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore TechWayFit.Licensing.Generator
      
    - name: Generate License
      run: |
        cd TechWayFit.Licensing.Generator
        dotnet run generate-license \
          --product-id "${{ env.PRODUCT_ID }}" \
          --product-name "${{ env.PRODUCT_NAME }}" \
          --to "${{ github.event.inputs.customer_name }}" \
          --contact "${{ github.event.inputs.customer_name }}" \
          --email "${{ github.event.inputs.customer_email }}" \
          --tier "${{ github.event.inputs.license_tier }}" \
          --private-key "${{ secrets.PRIVATE_KEY_PATH }}"
      env:
        PRODUCT_ID: "PROD-001"
        PRODUCT_NAME: "TechWayFit Platform"
        
    - name: Upload License
      uses: actions/upload-artifact@v4
      with:
        name: license-${{ github.event.inputs.customer_name }}
        path: TechWayFit.Licensing.Generator/license_*.json
```

### Azure DevOps Pipeline

```yaml
trigger: none

parameters:
- name: customerName
  displayName: Customer Name
  type: string
- name: customerEmail  
  displayName: Customer Email
  type: string
- name: licenseTier
  displayName: License Tier
  type: string
  default: Professional
  values:
  - Community
  - Professional
  - Enterprise

stages:
- stage: GenerateLicense
  jobs:
  - job: Generate
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: 'TechWayFit.Licensing.Generator'
        
    - task: DotNetCoreCLI@2
      displayName: 'Generate License'
      inputs:
        command: 'run'
        projects: 'TechWayFit.Licensing.Generator'
        arguments: |
          generate-license 
          --product-id "$(PRODUCT_ID)" 
          --product-name "$(PRODUCT_NAME)"
          --to "${{ parameters.customerName }}"
          --contact "${{ parameters.customerName }}"
          --email "${{ parameters.customerEmail }}"
          --tier "${{ parameters.licenseTier }}"
          --private-key "$(PRIVATE_KEY_PATH)"
      env:
        PRODUCT_ID: $(productId)
        PRODUCT_NAME: $(productName)
        PRIVATE_KEY_PATH: $(privateKeySecurePath)
```

## Dependencies & Requirements

### Runtime Requirements

- **.NET 8.0 Runtime** or later
- **Operating System**: Windows, macOS, Linux
- **Memory**: Minimum 50MB RAM
- **Storage**: Minimal (stateless operation)

### NuGet Dependencies

```xml
<PackageReference Include="TechWayFit.Licensing.Core" Version="1.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
<PackageReference Include="System.Security.Cryptography.Algorithms" Version="4.3.1" />
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

### Build Requirements

- **.NET 8.0 SDK**
- **C# 12** language features
- **NuGet package restore** capability

## License Validation

The generated licenses are designed to work with the TechWayFit.Licensing.Core validation library:

```csharp
// In your application
using TechWayFit.Licensing.Core.Services;

// Load the public key (distribute this with your application)
var publicKeyPem = await File.ReadAllTextAsync("public_key.pem");

// Load the license (provided by customer)
var licenseJson = await File.ReadAllTextAsync("customer_license.json");
var signedLicense = JsonSerializer.Deserialize<SignedLicense>(licenseJson);

// Validate the license
var validator = new LicenseValidationService();
var validationResult = await validator.ValidateLicenseAsync(signedLicense, publicKeyPem);

if (validationResult.IsValid)
{
    Console.WriteLine($"License is valid for: {validationResult.License.LicensedTo}");
    Console.WriteLine($"Tier: {validationResult.License.Tier}");
    Console.WriteLine($"Valid until: {validationResult.License.ValidTo}");
}
else
{
    Console.WriteLine($"License validation failed: {validationResult.Error}");
}
```

## Contributing & Development

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TechWayFit.Licensing.Generator
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run tests** (if available)
   ```bash
   dotnet test
   ```

### Contribution Guidelines

When contributing to this project:

1. **Maintain Stateless Design**: Ensure all operations remain stateless - no internal data storage
2. **Add Comprehensive Error Handling**: Include proper exception handling and user-friendly error messages
3. **Security First**: Follow cryptographic best practices and security guidelines
4. **Include Unit Tests**: Add tests for new features and bug fixes
5. **Update Documentation**: Keep README and code comments up to date
6. **Performance Considerations**: Optimize for both memory usage and execution speed

### Code Quality Standards

- **C# Coding Standards**: Follow Microsoft C# coding conventions
- **Security Review**: All cryptographic operations must be reviewed
- **Input Validation**: Validate all inputs thoroughly
- **Logging**: Use structured logging for operational visibility
- **Error Messages**: Provide clear, actionable error messages

### Testing

```bash
# Run the tool in development
dotnet run

# Test key generation
dotnet run generate-keys 2048

# Test license generation with sample data
dotnet run generate-license \
  --product-id "TEST-001" \
  --product-name "Test Product" \
  --to "Test Company" \
  --contact "Test User" \
  --email "test@example.com" \
  --tier "Community" \
  --private-key "path/to/test_private_key.pem"
```

## Deployment

### Standalone Executable

Build a self-contained executable for distribution:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS  
dotnet publish -c Release -r osx-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

### Docker Container

Create a Dockerfile for containerized deployment:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechWayFit.Licensing.Generator.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechWayFit.LicenseGenerator.dll"]
```

### Usage in Container

```bash
# Build the image
docker build -t techwayfit-license-generator .

# Generate keys
docker run --rm -v $(pwd)/keys:/app/keys \
  techwayfit-license-generator generate-keys 4096

# Generate license
docker run --rm -v $(pwd)/keys:/app/keys \
  techwayfit-license-generator generate-license \
  --product-id "PROD-001" \
  --product-name "My Product" \
  --to "Customer Name" \
  --contact "Contact Person" \
  --email "contact@customer.com" \
  --tier "Professional" \
  --private-key "/app/keys/private_key.pem"
```

## Support & Troubleshooting

### Common Issues

1. **"dotnet command not found"**
   - Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

2. **"Package 'TechWayFit.Licensing.Core' not found"**
   - Ensure NuGet package source is configured correctly
   - Run `dotnet restore` to restore packages

3. **"Access denied" errors on key files**
   - Check file permissions on the key directory
   - Ensure the application has read/write access

4. **"Invalid private key format"**
   - Verify the private key is in PEM format
   - Check for file corruption or encoding issues

### Getting Help

For support and questions:

1. **Documentation**: Check this README for comprehensive usage information
2. **Error Messages**: Read error messages carefully - they provide specific guidance
3. **Validation Commands**: Use built-in validation commands to diagnose key issues
4. **Logs**: Enable verbose logging to see detailed operation information

### Performance Considerations

- **Key Generation**: 4096-bit key generation takes ~1-2 seconds
- **License Signing**: Signing operations are typically < 100ms
- **Memory Usage**: Tool uses minimal memory (< 50MB) due to stateless design
- **Parallel Processing**: Safe to run multiple instances simultaneously

---

## License

This tool is part of the TechWayFit licensing system. All rights reserved.

**Version**: 2.0  
**Build**: Stateless Architecture  
**Last Updated**: July 27, 2025
