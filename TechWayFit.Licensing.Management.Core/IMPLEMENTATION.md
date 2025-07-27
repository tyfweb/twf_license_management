# Implementation Guide

## Overview

This guide provides step-by-step instructions for implementing the TechWayFit.Licensing.Management.Core system in your enterprise environment.

## Quick Start

### 1. Prerequisites

- **.NET 8.0 or later**
- **Entity Framework Core 8.0+**
- **SQL Server 2019+** or compatible database
- **Visual Studio 2022** or compatible IDE

### 2. Package Installation

```bash
# Core Dependencies
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.Extensions.DependencyInjection
Install-Package Microsoft.Extensions.Logging

# Security
Install-Package System.Security.Cryptography
Install-Package Microsoft.AspNetCore.Cryptography.KeyDerivation

# Validation
Install-Package FluentValidation
Install-Package FluentValidation.DependencyInjectionExtensions
```

### 3. Database Setup

```sql
-- Execute the setup script
-- This creates all required tables and initial data
\i setup_database.sql
```

### 4. Configuration

**appsettings.json**
```json
{
  "ConnectionStrings": {
    "LicensingDatabase": "Server=localhost;Database=TechWayFitLicensing;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "LicensingSettings": {
    "DefaultLicenseExpiryDays": 365,
    "MaxUsersPerLicense": 1000,
    "CryptographicKeyRotationDays": 90,
    "NotificationRetryAttempts": 3
  }
}
```

## Service Implementation

### 1. Service Registration

**Program.cs / Startup.cs**
```csharp
// Register Entity Framework
builder.Services.AddDbContext<LicensingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register core services
builder.Services.AddScoped<IProductLicenseService, ProductLicenseService>();
builder.Services.AddScoped<IConsumerAccountService, ConsumerAccountService>();
builder.Services.AddScoped<IEnterpriseProductService, EnterpriseProductService>();
builder.Services.AddScoped<IProductTierService, ProductTierService>();
builder.Services.AddScoped<IProductFeatureService, ProductFeatureService>();

// Register infrastructure services
builder.Services.AddScoped<ICryptographicService, CryptographicService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportingService, ReportingService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<ProductLicenseValidator>();
```

### 2. Sample Service Implementation

**ProductLicenseService.cs**
```csharp
public class ProductLicenseService : IProductLicenseService
{
    private readonly LicensingDbContext _context;
    private readonly ICryptographicService _cryptoService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProductLicenseService> _logger;

    public ProductLicenseService(
        LicensingDbContext context,
        ICryptographicService cryptoService,
        IAuditService auditService,
        ILogger<ProductLicenseService> logger)
    {
        _context = context;
        _cryptoService = cryptoService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ProductLicense> GenerateLicenseAsync(
        LicenseGenerationRequest request, 
        string generatedBy)
    {
        try
        {
            // Validate request
            var validationResult = await ValidateGenerationRequestAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors));

            // Generate license key using cryptographic service
            var licenseKey = await _cryptoService.GenerateLicenseKeyAsync(
                request.ProductId, 
                request.ConsumerId, 
                request.AdditionalProperties);

            // Create license entity
            var license = new ProductLicense
            {
                Id = Guid.NewGuid().ToString(),
                LicenseKey = licenseKey,
                ProductId = request.ProductId,
                ConsumerId = request.ConsumerId,
                LicenseType = request.LicenseType,
                Status = LicenseStatus.Generated,
                ExpiryDate = request.ExpiryDate,
                MaxUsers = request.MaxUsers,
                MaxDevices = request.MaxDevices,
                Features = request.Features.ToList(),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = generatedBy,
                // ... other properties
            };

            // Generate digital signature
            var licenseData = SerializeLicenseData(license);
            license.DigitalSignature = await _cryptoService.GenerateDigitalSignatureAsync(licenseData);

            // Save to database
            _context.ProductLicenses.Add(license);
            await _context.SaveChangesAsync();

            // Log audit entry
            await _auditService.LogLicenseCreatedAsync(license, generatedBy);

            _logger.LogInformation("License generated successfully: {LicenseId}", license.Id);
            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate license for product {ProductId}", request.ProductId);
            throw;
        }
    }

    public async Task<LicenseValidationResult> ValidateLicenseAsync(
        string licenseKey, 
        string productId, 
        bool checkActivation = true)
    {
        try
        {
            // Find license by key
            var license = await _context.ProductLicenses
                .Include(l => l.Product)
                .Include(l => l.Consumer)
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);

            if (license == null)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorCode = "LICENSE_NOT_FOUND",
                    ErrorMessage = "License key not found"
                };
            }

            // Validate cryptographic integrity
            var isKeyValid = await _cryptoService.ValidateLicenseKeyAsync(
                licenseKey, 
                license.ProductId, 
                license.ConsumerId);

            if (!isKeyValid)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_LICENSE_KEY",
                    ErrorMessage = "License key is cryptographically invalid"
                };
            }

            // Validate digital signature
            var licenseData = SerializeLicenseData(license);
            var isSignatureValid = await _cryptoService.VerifyDigitalSignatureAsync(
                licenseData, 
                license.DigitalSignature);

            if (!isSignatureValid)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_SIGNATURE",
                    ErrorMessage = "License signature is invalid"
                };
            }

            // Check business validations
            var businessValidation = ValidateBusinessRules(license, productId, checkActivation);
            if (!businessValidation.IsValid)
                return businessValidation;

            return new LicenseValidationResult
            {
                IsValid = true,
                License = license,
                ValidationMetadata = new Dictionary<string, object>
                {
                    { "ValidatedAt", DateTime.UtcNow },
                    { "KeyVersion", await _cryptoService.GetCurrentKeyVersionAsync() }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate license: {LicenseKey}", licenseKey);
            return new LicenseValidationResult
            {
                IsValid = false,
                ErrorCode = "VALIDATION_ERROR",
                ErrorMessage = "An error occurred during validation"
            };
        }
    }

    // Additional methods...
}
```

### 3. Database Context

**LicensingDbContext.cs**
```csharp
public class LicensingDbContext : DbContext
{
    public LicensingDbContext(DbContextOptions<LicensingDbContext> options) : base(options) { }

    // Core entities
    public DbSet<ProductLicense> ProductLicenses { get; set; }
    public DbSet<EnterpriseProduct> EnterpriseProducts { get; set; }
    public DbSet<ConsumerAccount> ConsumerAccounts { get; set; }
    public DbSet<ProductTier> ProductTiers { get; set; }
    public DbSet<ProductFeature> ProductFeatures { get; set; }

    // Infrastructure entities
    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<NotificationHistory> NotificationHistory { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity relationships and constraints
        ConfigureProductLicense(modelBuilder);
        ConfigureEnterpriseProduct(modelBuilder);
        ConfigureConsumerAccount(modelBuilder);
        // ... other configurations
    }

    private void ConfigureProductLicense(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductLicense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicenseKey).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DigitalSignature).IsRequired().HasMaxLength(1000);
            
            entity.HasIndex(e => e.LicenseKey).IsUnique();
            entity.HasIndex(e => new { e.ProductId, e.ConsumerId });
            entity.HasIndex(e => e.ExpiryDate);
            
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Licenses)
                  .HasForeignKey(e => e.ProductId);
                  
            entity.HasOne(e => e.Consumer)
                  .WithMany(c => c.Licenses)
                  .HasForeignKey(e => e.ConsumerId);
        });
    }
}
```

## Cryptographic Service Implementation

### 1. Security Configuration

**CryptographicService.cs**
```csharp
public class CryptographicService : ICryptographicService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CryptographicService> _logger;
    private readonly string _masterKey;

    public CryptographicService(IConfiguration configuration, ILogger<CryptographicService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _masterKey = _configuration["Cryptographic:MasterKey"] ?? throw new InvalidOperationException("Master key not configured");
    }

    public async Task<string> GenerateLicenseKeyAsync(
        string productId, 
        string consumerId, 
        Dictionary<string, object>? additionalData = null)
    {
        try
        {
            var keyData = new
            {
                ProductId = productId,
                ConsumerId = consumerId,
                GeneratedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                RandomSalt = GenerateRandomSalt(),
                AdditionalData = additionalData ?? new Dictionary<string, object>()
            };

            var jsonData = JsonSerializer.Serialize(keyData);
            var encryptedData = await EncryptDataAsync(jsonData);
            var encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedData));

            return $"TWF-{encodedKey}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate license key for product {ProductId}", productId);
            throw new CryptographicException("License key generation failed", ex);
        }
    }

    public async Task<bool> ValidateLicenseKeyAsync(
        string licenseKey, 
        string productId, 
        string consumerId)
    {
        try
        {
            if (!licenseKey.StartsWith("TWF-"))
                return false;

            var encodedData = licenseKey.Substring(4);
            var encryptedData = Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
            var decryptedJson = await DecryptDataAsync(encryptedData);
            
            var keyData = JsonSerializer.Deserialize<Dictionary<string, object>>(decryptedJson);
            
            return keyData.ContainsKey("ProductId") && 
                   keyData["ProductId"].ToString() == productId &&
                   keyData.ContainsKey("ConsumerId") && 
                   keyData["ConsumerId"].ToString() == consumerId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "License key validation failed: {LicenseKey}", licenseKey);
            return false;
        }
    }

    // Additional cryptographic methods...
}
```

## Validation Implementation

### 1. FluentValidation Validators

**ProductLicenseValidator.cs**
```csharp
public class ProductLicenseValidator : AbstractValidator<ProductLicense>
{
    public ProductLicenseValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ConsumerId)
            .NotEmpty()
            .WithMessage("Consumer ID is required");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future");

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000)
            .WithMessage("Max users must be between 1 and 10,000");

        RuleFor(x => x.MaxDevices)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Max devices must be between 1 and 1,000");

        RuleFor(x => x.LicenseKey)
            .NotEmpty()
            .When(x => x.Status != LicenseStatus.Draft)
            .WithMessage("License key is required for non-draft licenses");
    }
}
```

## API Controller Implementation

### 1. License Management Controller

**LicensesController.cs**
```csharp
[ApiController]
[Route("api/[controller]")]
public class LicensesController : ControllerBase
{
    private readonly IProductLicenseService _licenseService;
    private readonly ILogger<LicensesController> _logger;

    public LicensesController(
        IProductLicenseService licenseService,
        ILogger<LicensesController> logger)
    {
        _licenseService = licenseService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ProductLicense>> GenerateLicense(
        [FromBody] LicenseGenerationRequest request)
    {
        try
        {
            var currentUser = User.Identity?.Name ?? "System";
            var license = await _licenseService.GenerateLicenseAsync(request, currentUser);
            return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, license);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate license");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPost("{licenseKey}/validate")]
    public async Task<ActionResult<LicenseValidationResult>> ValidateLicense(
        string licenseKey,
        [FromQuery] string productId,
        [FromQuery] bool checkActivation = true)
    {
        var result = await _licenseService.ValidateLicenseAsync(licenseKey, productId, checkActivation);
        
        if (!result.IsValid)
            return BadRequest(result);
            
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductLicense>> GetLicense(string id)
    {
        var license = await _licenseService.GetLicenseByIdAsync(id);
        
        if (license == null)
            return NotFound();
            
        return Ok(license);
    }

    // Additional endpoints...
}
```

## Testing Strategy

### 1. Unit Tests

**ProductLicenseServiceTests.cs**
```csharp
public class ProductLicenseServiceTests
{
    private readonly Mock<LicensingDbContext> _mockContext;
    private readonly Mock<ICryptographicService> _mockCryptoService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly ProductLicenseService _service;

    public ProductLicenseServiceTests()
    {
        _mockContext = new Mock<LicensingDbContext>();
        _mockCryptoService = new Mock<ICryptographicService>();
        _mockAuditService = new Mock<IAuditService>();
        
        _service = new ProductLicenseService(
            _mockContext.Object,
            _mockCryptoService.Object,
            _mockAuditService.Object,
            Mock.Of<ILogger<ProductLicenseService>>());
    }

    [Fact]
    public async Task GenerateLicenseAsync_ValidRequest_ReturnsLicense()
    {
        // Arrange
        var request = new LicenseGenerationRequest
        {
            ProductId = "PROD001",
            ConsumerId = "CONS001",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            MaxUsers = 100,
            MaxDevices = 10
        };

        _mockCryptoService
            .Setup(x => x.GenerateLicenseKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync("TWF-GENERATED-KEY");

        _mockCryptoService
            .Setup(x => x.GenerateDigitalSignatureAsync(It.IsAny<string>()))
            .ReturnsAsync("DIGITAL-SIGNATURE");

        // Act
        var result = await _service.GenerateLicenseAsync(request, "TestUser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TWF-GENERATED-KEY", result.LicenseKey);
        Assert.Equal("PROD001", result.ProductId);
        Assert.Equal("CONS001", result.ConsumerId);
    }

    // Additional tests...
}
```

### 2. Integration Tests

**LicenseIntegrationTests.cs**
```csharp
public class LicenseIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LicenseIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GenerateLicense_ValidRequest_ReturnsCreatedLicense()
    {
        // Arrange
        var request = new LicenseGenerationRequest
        {
            ProductId = "PROD001",
            ConsumerId = "CONS001",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            MaxUsers = 100,
            MaxDevices = 10
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/licenses", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var license = JsonSerializer.Deserialize<ProductLicense>(responseContent);
        
        Assert.NotNull(license);
        Assert.Equal("PROD001", license.ProductId);
    }
}
```

## Deployment Considerations

### 1. Environment Configuration

**Production appsettings.json**
```json
{
  "ConnectionStrings": {
    "LicensingDatabase": "Server=prod-server;Database=TechWayFitLicensing;Integrated Security=true;Encrypt=true;"
  },
  "LicensingSettings": {
    "DefaultLicenseExpiryDays": 365,
    "MaxUsersPerLicense": 1000,
    "CryptographicKeyRotationDays": 30,
    "NotificationRetryAttempts": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "TechWayFit.Licensing": "Debug"
    }
  }
}
```

### 2. Database Migration

```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Production deployment
dotnet ef database update --connection "YourProductionConnectionString"
```

### 3. Security Hardening

- Store master keys in Azure Key Vault or similar secure storage
- Enable database encryption at rest
- Implement API rate limiting
- Add comprehensive logging and monitoring
- Regular security audits and penetration testing

## Monitoring and Maintenance

### 1. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<LicensingDbContext>()
    .AddCheck<CryptographicServiceHealthCheck>("cryptographic-service")
    .AddCheck<LicenseValidationHealthCheck>("license-validation");
```

### 2. Performance Metrics

- License generation time
- Validation response time
- Database query performance
- Cryptographic operation duration
- API endpoint response times

### 3. Regular Maintenance Tasks

- Key rotation (quarterly)
- Audit log archival (monthly)
- Performance optimization review
- Security vulnerability assessment
- License expiration notifications

---

**Implementation Support**: For additional assistance, refer to the technical documentation or contact the development team.

**Last Updated**: July 25, 2025  
**Version**: 1.0.0
