---
title: Development
nav_order: 7
has_children: true
---

# Development Guide
{: .no_toc }

Comprehensive guide for developers working with the TechWayFit License Management System.

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Overview

This section provides all the information needed for developers to contribute to, extend, or integrate with the TechWayFit License Management System.

## Development Environment Setup

### Prerequisites
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** / **VS Code** / **JetBrains Rider**
- **Git** for version control
- **Docker Desktop** (for containerized development)
- **PostgreSQL** / **SQL Server** (or use Docker)
- **Node.js 18+** (for frontend build tools)

### IDE Configuration

#### Visual Studio 2022
Recommended extensions:
- **SonarLint** - Code quality analysis
- **CodeMaid** - Code cleanup and organization
- **Roslynator** - Additional analyzers and refactorings
- **Entity Framework Core Power Tools** - EF Core tooling

#### VS Code
Recommended extensions:
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.blazorwasm-companion",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-json",
    "ms-azure-devops.azure-pipelines"
  ]
}
```

### Local Development Setup

#### 1. Clone Repository
```bash
git clone https://github.com/TechWayFit/licensing-management.git
cd licensing-management
```

#### 2. Setup Development Database
```bash
# Using Docker (recommended)
docker run --name licensing-dev-db \
  -e POSTGRES_DB=licensing_dev \
  -e POSTGRES_USER=dev_user \
  -e POSTGRES_PASSWORD=dev_password \
  -p 5432:5432 \
  -d postgres:14

# Or install PostgreSQL locally and create database
createdb licensing_dev
```

#### 3. Configure Application
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=licensing_dev;Username=dev_user;Password=dev_password"
  },
  "Infrastructure": {
    "Provider": "PostgreSql"
  },
  "Security": {
    "RequireHttps": false,
    "EncryptionKey": "development_key_not_for_production"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "TechWayFit.Licensing": "Trace"
    }
  }
}
```

#### 4. Run Initial Setup
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run database migrations
cd source/website/TechWayFit.Licensing.Management.Web
dotnet ef database update

# Start the application
dotnet run
```

#### 5. Verify Setup
- Navigate to `https://localhost:5001`
- Access Swagger UI at `https://localhost:5001/swagger`
- Check health endpoint at `https://localhost:5001/health`

---

## Project Structure

### Solution Architecture
```
TechWayFit.Licensing.sln
├── source/
│   ├── core/
│   │   ├── TechWayFit.Licensing.Management.Core/        # Domain models and interfaces
│   │   ├── TechWayFit.Licensing.Management.Infrastructure/ # Infrastructure abstractions
│   │   └── TechWayFit.Licensing.Generator/              # License generation utilities
│   ├── repositories/
│   │   ├── TechWayFit.Licensing.Management.Infrastructure.EntityFramework/ # EF Core implementation
│   │   ├── TechWayFit.Licensing.Management.Infrastructure.PostgreSql/     # PostgreSQL provider
│   │   ├── TechWayFit.Licensing.Management.Infrastructure.SqlServer/      # SQL Server provider
│   │   └── TechWayFit.Licensing.Management.Infrastructure.InMemory/       # In-memory provider
│   ├── services/
│   │   ├── TechWayFit.Licensing.Management.Services/    # Business logic layer
│   │   └── TechWayFit.Licensing.Validation/            # License validation services
│   └── website/
│       └── TechWayFit.Licensing.Management.Web/        # Web application
├── tests/
│   ├── TechWayFit.Licensing.Management.Tests.Unit/     # Unit tests
│   ├── TechWayFit.Licensing.Management.Tests.Integration/ # Integration tests
│   └── TechWayFit.Licensing.Management.Tests.Performance/ # Performance tests
├── docs/                                               # Documentation
└── scripts/                                           # Build and deployment scripts
```

### Core Components

#### Domain Layer (`Core`)
```csharp
// Domain entities with business logic
public class Product : BaseEntity<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ProductStatus Status { get; set; }
    
    // Business methods
    public Result<ProductKey> GenerateKey(string algorithm = "RSA2048")
    {
        // Key generation logic
    }
    
    public bool CanCreateLicense()
    {
        return Status == ProductStatus.Active && HasValidKeys();
    }
}

// Domain services
public interface ILicenseGenerationService
{
    Task<Result<LicenseFile>> GenerateLicenseAsync(GenerateLicenseRequest request);
}
```

#### Application Layer (`Services`)
```csharp
// Application services orchestrating business operations
public class ProductLicenseService : IProductLicenseService
{
    private readonly IProductRepository _productRepository;
    private readonly ILicenseRepository _licenseRepository;
    private readonly ILicenseGenerationService _licenseGenerationService;
    
    public async Task<Result<ProductLicense>> CreateLicenseAsync(CreateLicenseRequest request)
    {
        // Validation, business logic, and persistence
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            return Result<ProductLicense>.Failure("Product not found");
            
        // ... business logic
        
        var license = new ProductLicense(/* parameters */);
        await _licenseRepository.AddAsync(license);
        
        return Result<ProductLicense>.Success(license);
    }
}
```

#### Infrastructure Layer
```csharp
// Repository implementations
public class ProductRepository : AuditRepository<Product, ProductEntity>, IProductRepository
{
    public ProductRepository(LicensingDbContext context, IMapper mapper) : base(context, mapper) { }
    
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        var entities = await Context.Products
            .Where(p => p.Status == ProductStatus.Active)
            .ToListAsync();
            
        return Mapper.Map<IEnumerable<Product>>(entities);
    }
}
```

---

## Coding Standards

### C# Coding Guidelines

#### Naming Conventions
```csharp
// Classes and methods: PascalCase
public class LicenseValidationService
{
    public async Task<ValidationResult> ValidateAsync(string licenseKey) { }
}

// Private fields: _camelCase
private readonly ILogger<LicenseValidationService> _logger;

// Parameters and local variables: camelCase
public void ProcessLicense(string licenseKey, DateTime validFrom) { }

// Constants: PascalCase
public const string DefaultAlgorithm = "RSA2048";

// Enums: PascalCase with descriptive names
public enum LicenseStatus
{
    Pending,
    Active,
    Expired,
    Revoked
}
```

#### Code Organization
```csharp
// File organization
namespace TechWayFit.Licensing.Management.Services.Implementations.License
{
    // 1. Using statements (grouped and sorted)
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TechWayFit.Licensing.Management.Core.Interfaces;
    
    // 2. Class declaration with XML documentation
    /// <summary>
    /// Service responsible for product license management operations.
    /// </summary>
    public class ProductLicenseService : IProductLicenseService
    {
        // 3. Private fields
        private readonly ILogger<ProductLicenseService> _logger;
        private readonly IProductRepository _productRepository;
        
        // 4. Constructor
        public ProductLicenseService(
            ILogger<ProductLicenseService> logger,
            IProductRepository productRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }
        
        // 5. Public methods
        public async Task<Result<License>> CreateLicenseAsync(CreateLicenseRequest request)
        {
            // Implementation
        }
        
        // 6. Private methods
        private bool ValidateRequest(CreateLicenseRequest request)
        {
            // Validation logic
        }
    }
}
```

#### Error Handling
```csharp
// Use Result pattern for business operations
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// Usage example
public async Task<Result<License>> ValidateLicenseAsync(string licenseKey)
{
    try
    {
        if (string.IsNullOrEmpty(licenseKey))
            return Result<License>.Failure("License key is required");
            
        var license = await _repository.GetByKeyAsync(licenseKey);
        if (license == null)
            return Result<License>.Failure("License not found");
            
        return Result<License>.Success(license);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error validating license {LicenseKey}", licenseKey);
        return Result<License>.Failure("An error occurred during validation");
    }
}
```

### Frontend Guidelines

#### Component Structure
```typescript
// React component example
import React, { useState, useEffect } from 'react';
import { License, LicenseService } from '../services/LicenseService';

interface LicenseListProps {
  productId: string;
  onLicenseSelected?: (license: License) => void;
}

export const LicenseList: React.FC<LicenseListProps> = ({ 
  productId, 
  onLicenseSelected 
}) => {
  const [licenses, setLicenses] = useState<License[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadLicenses = async () => {
      try {
        setLoading(true);
        const result = await LicenseService.getByProduct(productId);
        setLicenses(result.data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    loadLicenses();
  }, [productId]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="license-list">
      {licenses.map(license => (
        <div 
          key={license.id} 
          className="license-item"
          onClick={() => onLicenseSelected?.(license)}
        >
          {license.licenseKey}
        </div>
      ))}
    </div>
  );
};
```

---

## Testing Guidelines

### Unit Testing

#### Test Structure
```csharp
// Test class naming: [ClassUnderTest]Tests
public class ProductLicenseServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ILicenseRepository> _mockLicenseRepository;
    private readonly ProductLicenseService _service;

    public ProductLicenseServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockLicenseRepository = new Mock<ILicenseRepository>();
        _service = new ProductLicenseService(_mockProductRepository.Object, _mockLicenseRepository.Object);
    }

    [Fact]
    public async Task CreateLicenseAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateLicenseRequest
        {
            ProductId = Guid.NewGuid(),
            ConsumerId = Guid.NewGuid()
        };
        
        var product = new Product { Id = request.ProductId, Status = ProductStatus.Active };
        _mockProductRepository.Setup(x => x.GetByIdAsync(request.ProductId))
            .ReturnsAsync(product);

        // Act
        var result = await _service.CreateLicenseAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ProductId.Should().Be(request.ProductId);
    }

    [Fact]
    public async Task CreateLicenseAsync_WithInvalidProduct_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateLicenseRequest { ProductId = Guid.NewGuid() };
        _mockProductRepository.Setup(x => x.GetByIdAsync(request.ProductId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _service.CreateLicenseAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Product not found");
    }
}
```

### Integration Testing

#### Test Setup
```csharp
public class LicenseApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LicenseApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory version
                services.RemoveAll<DbContextOptions<LicensingDbContext>>();
                services.AddDbContext<LicensingDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreateLicense_ReturnsSuccessResult()
    {
        // Arrange
        var request = new CreateLicenseRequest
        {
            ProductId = TestData.ValidProductId,
            ConsumerId = TestData.ValidConsumerId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/licenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var license = await response.Content.ReadFromJsonAsync<License>();
        license.Should().NotBeNull();
        license.ProductId.Should().Be(request.ProductId);
    }
}
```

### Performance Testing

#### Benchmark Tests
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class LicenseValidationBenchmarks
{
    private LicenseValidationService _service;
    private string _validLicenseKey;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize service and test data
        _service = new LicenseValidationService(/* dependencies */);
        _validLicenseKey = "VALID-LICENSE-KEY-FOR-TESTING";
    }

    [Benchmark]
    public async Task ValidateLicense()
    {
        await _service.ValidateAsync(_validLicenseKey);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task ValidateMultipleLicenses(int count)
    {
        var tasks = Enumerable.Range(0, count)
            .Select(_ => _service.ValidateAsync(_validLicenseKey));
        
        await Task.WhenAll(tasks);
    }
}
```

---

## API Development

### Controller Design

#### RESTful Controllers
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of products.
    /// </summary>
    /// <param name="request">Search and pagination parameters</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] GetProductsRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific product by ID.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Administrator,ProductManager")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.CreateProductAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value.Id }, result.Value);
    }
}
```

#### Request/Response Models
```csharp
// Request DTOs with validation
public class CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [EmailAddress]
    public string SupportEmail { get; set; }

    [Phone]
    public string SupportPhone { get; set; }

    public DateTime? ReleaseDate { get; set; }

    [ValidateComplexType]
    public ProductMetadata Metadata { get; set; }
}

// Response DTOs
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProductStatus Status { get; set; }
    public string SupportEmail { get; set; }
    public string SupportPhone { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ProductMetadata Metadata { get; set; }
}
```

### API Documentation

#### OpenAPI Configuration
```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechWayFit License Management API",
        Version = "v1",
        Description = "REST API for managing software licenses",
        Contact = new OpenApiContact
        {
            Name = "TechWayFit Support",
            Email = "support@techwayfit.com",
            Url = new Uri("https://www.techwayfit.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Add security definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});
```

---

## Performance Optimization

### MiniProfiler Integration

The system includes comprehensive performance monitoring using MiniProfiler:

#### Configuration
```csharp
// ProfilingExtensions.cs
public static class ProfilingExtensions
{
    public static IServiceCollection AddProfilingServices(this IServiceCollection services)
    {
        services.AddMiniProfiler(options =>
        {
            options.RouteBasePath = "/profiler";
            options.PopupRenderPosition = RenderPosition.BottomLeft;
            options.PopupShowTimeWithChildren = true;
            options.PopupMaxTracesToShow = 20;
            
            // Storage provider
            options.Storage = new MemoryCacheStorage(TimeSpan.FromMinutes(60));
            
            // Sampling - profile 10% of requests in production
            options.ShouldProfile = request => 
            {
                var env = request.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
                return env.IsDevelopment() || Random.Shared.NextDouble() < 0.1;
            };
        });

        // Add Entity Framework Core profiling
        services.AddMiniProfilerEfCore();

        return services;
    }

    public static IApplicationBuilder UseProfilingMiddleware(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
        
        if (env.IsDevelopment() || env.IsStaging())
        {
            app.UseMiniProfiler();
        }

        return app;
    }
}
```

#### Usage in Controllers
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
{
    using (MiniProfiler.Current.Step("Get Product Details"))
    {
        using (MiniProfiler.Current.Step("Load Product from Database"))
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(result.Error);
        }

        using (MiniProfiler.Current.Step("Map to DTO"))
        {
            return Ok(result.Value);
        }
    }
}
```

### Database Performance

#### Query Optimization
```csharp
// Efficient queries with proper indexing
public async Task<IEnumerable<Product>> GetActiveProductsWithLicenseCountAsync()
{
    return await Context.Products
        .Where(p => p.Status == ProductStatus.Active)
        .Select(p => new ProductWithLicenseCount
        {
            Id = p.Id,
            Name = p.Name,
            LicenseCount = p.Licenses.Count(l => l.Status == LicenseStatus.Active)
        })
        .AsNoTracking() // Read-only operations
        .ToListAsync();
}

// Bulk operations for better performance
public async Task<int> BulkUpdateLicenseStatusAsync(IEnumerable<Guid> licenseIds, LicenseStatus newStatus)
{
    return await Context.Licenses
        .Where(l => licenseIds.Contains(l.Id))
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(l => l.Status, newStatus)
            .SetProperty(l => l.UpdatedAt, DateTime.UtcNow));
}
```

#### Caching Strategy
```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedProductService> _logger;

    public async Task<Result<Product>> GetProductByIdAsync(Guid id)
    {
        var cacheKey = $"product:{id}";
        
        if (_cache.TryGetValue(cacheKey, out Product cachedProduct))
        {
            _logger.LogDebug("Product {ProductId} found in cache", id);
            return Result<Product>.Success(cachedProduct);
        }

        var result = await _innerService.GetProductByIdAsync(id);
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromMinutes(15));
        }

        return result;
    }
}
```

---

## Build and Deployment

### CI/CD Pipeline

#### GitHub Actions Workflow
```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '18.x'

jobs:
  test:
    name: Run Tests
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:14
        env:
          POSTGRES_DB: licensing_test
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_password
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run unit tests
      run: dotnet test tests/TechWayFit.Licensing.Management.Tests.Unit --no-build --verbosity normal
    
    - name: Run integration tests
      run: dotnet test tests/TechWayFit.Licensing.Management.Tests.Integration --no-build --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=licensing_test;Username=test_user;Password=test_password"

  build:
    name: Build and Push Docker Image
    runs-on: ubuntu-latest
    needs: test
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2
    
    - name: Login to Docker Hub
      uses: docker/login-action@v2
      with:
        username: ${{ secrets.DOCKERHUB_USERNAME }}
        password: ${{ secrets.DOCKERHUB_TOKEN }}
    
    - name: Build and push
      uses: docker/build-push-action@v4
      with:
        context: .
        push: true
        tags: |
          techwayfit/licensing:latest
          techwayfit/licensing:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy:
    name: Deploy to Production
    runs-on: ubuntu-latest
    needs: build
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'licensing-production'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        images: 'techwayfit/licensing:${{ github.sha }}'
```

### Docker Configuration

#### Multi-stage Dockerfile
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["source/website/TechWayFit.Licensing.Management.Web/*.csproj", "source/website/TechWayFit.Licensing.Management.Web/"]
COPY ["source/core/TechWayFit.Licensing.Management.Core/*.csproj", "source/core/TechWayFit.Licensing.Management.Core/"]
COPY ["source/services/TechWayFit.Licensing.Management.Services/*.csproj", "source/services/TechWayFit.Licensing.Management.Services/"]
RUN dotnet restore "source/website/TechWayFit.Licensing.Management.Web/TechWayFit.Licensing.Management.Web.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/source/website/TechWayFit.Licensing.Management.Web"
RUN dotnet build -c Release -o /app/build

# Publish application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "TechWayFit.Licensing.Management.Web.dll"]
```

---

## Contributing Guidelines

### Development Workflow

1. **Fork the Repository**: Create a personal fork on GitHub
2. **Create Feature Branch**: `git checkout -b feature/license-validation-improvements`
3. **Make Changes**: Implement your feature or bug fix
4. **Write Tests**: Ensure comprehensive test coverage
5. **Run Quality Checks**: Linting, testing, security scans
6. **Submit Pull Request**: With detailed description and testing instructions

### Code Review Process

#### Pull Request Requirements
- [ ] **Tests**: All new code covered by tests
- [ ] **Documentation**: Updated documentation for new features
- [ ] **Performance**: No performance regressions
- [ ] **Security**: Security review for sensitive changes
- [ ] **Breaking Changes**: Documented and justified

#### Review Checklist
```markdown
## Code Review Checklist

### Functionality
- [ ] Code solves the intended problem
- [ ] Edge cases are handled appropriately
- [ ] Error handling is comprehensive

### Code Quality
- [ ] Code follows established patterns
- [ ] Names are descriptive and meaningful
- [ ] Methods are focused and single-purpose
- [ ] Comments explain complex logic

### Testing
- [ ] Unit tests cover new functionality
- [ ] Integration tests validate end-to-end scenarios
- [ ] Test names clearly describe what's being tested

### Performance
- [ ] No obvious performance issues
- [ ] Database queries are efficient
- [ ] Memory usage is reasonable

### Security
- [ ] Input validation is present
- [ ] Authorization checks are correct
- [ ] Sensitive data is properly handled
```

---

## Best Practices

### Security Development

#### Secure Coding Practices
```csharp
// Input validation
public async Task<Result<License>> ValidateLicenseAsync(string licenseKey)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(licenseKey))
        return Result<License>.Failure("License key is required");
    
    if (licenseKey.Length > 100) // Prevent DoS attacks
        return Result<License>.Failure("License key is too long");
    
    // Sanitize input
    licenseKey = licenseKey.Trim().ToUpperInvariant();
    
    // Use parameterized queries (handled by EF Core)
    var license = await _context.Licenses
        .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
    
    if (license == null)
    {
        // Don't reveal whether license exists
        _logger.LogWarning("License validation attempt with invalid key from {IPAddress}", 
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress);
        return Result<License>.Failure("License validation failed");
    }
    
    return Result<License>.Success(license);
}

// Secure configuration handling
public class LicenseConfiguration
{
    [Required]
    public string EncryptionKey { get; set; }
    
    [Required]
    public string SigningKey { get; set; }
    
    // Validate configuration on startup
    public void Validate()
    {
        if (string.IsNullOrEmpty(EncryptionKey) || EncryptionKey.Length < 32)
            throw new InvalidOperationException("EncryptionKey must be at least 32 characters");
            
        if (string.IsNullOrEmpty(SigningKey) || SigningKey.Length < 64)
            throw new InvalidOperationException("SigningKey must be at least 64 characters");
    }
}
```

### Performance Best Practices

#### Async/Await Patterns
```csharp
// Correct async usage
public async Task<IEnumerable<License>> GetLicensesAsync(Guid productId)
{
    // Don't use .Result or .Wait()
    var licenses = await _repository.GetByProductIdAsync(productId);
    
    // Use ConfigureAwait(false) in library code
    var validationTasks = licenses.Select(l => ValidateLicenseAsync(l.LicenseKey));
    var validationResults = await Task.WhenAll(validationTasks).ConfigureAwait(false);
    
    return licenses.Where((license, index) => validationResults[index].IsValid);
}

// Efficient data access
public async Task<PagedResult<Product>> GetProductsAsync(int page, int pageSize)
{
    var query = _context.Products.AsQueryable();
    
    // Count total before applying pagination
    var total = await query.CountAsync();
    
    // Apply pagination and projection in single query
    var products = await query
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Status = p.Status
        })
        .AsNoTracking() // Read-only
        .ToListAsync();
    
    return new PagedResult<Product>
    {
        Data = products,
        Page = page,
        PageSize = pageSize,
        Total = total
    };
}
```

---

## Next Steps

For more specific development topics:

1. **[API Development](api-development.html)** - Detailed API development guidelines
2. **[Frontend Development](frontend-development.html)** - UI development with React/Blazor
3. **[Database Development](database-development.html)** - Database schema and migrations
4. **[Testing Strategies](testing.html)** - Comprehensive testing approaches
5. **[Performance Tuning](performance.html)** - Advanced performance optimization
6. **[Security Implementation](security-implementation.html)** - Security development practices

---

## Resources

### Documentation
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [MiniProfiler Documentation](https://miniprofiler.com/dotnet/)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [VS Code](https://code.visualstudio.com/)
- [Docker](https://www.docker.com/)
- [Postman](https://www.postman.com/)

### Community
- [GitHub Repository](https://github.com/TechWayFit/licensing-management)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/techwayfit-licensing)
- [Discord Community](https://discord.gg/techwayfit)
- [Developer Blog](https://blog.techwayfit.com/)
