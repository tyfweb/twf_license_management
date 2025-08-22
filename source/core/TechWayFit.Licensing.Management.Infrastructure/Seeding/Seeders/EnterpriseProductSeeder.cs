using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeds enterprise products and product tiers
/// </summary>
public class EnterpriseProductSeeder : IDataSeeder
{
    private readonly ILogger<EnterpriseProductSeeder> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public string SeederName => "EnterpriseProduct";
    public int Order => 20; // Run after basic data seeders

    public EnterpriseProductSeeder(
        ILogger<EnterpriseProductSeeder> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var existingProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            if (existingProducts.Any())
            {
                _logger.LogInformation("Enterprise products already exist, skipping seeding");
                return true;
            }

            _logger.LogInformation("Seeding enterprise products and tiers...");

            // Create default product
            var product = new EnterpriseProduct
            {
                ProductId = Guid.NewGuid(),
                Name = "TechWayFit Enterprise Suite",
                Description = "Complete enterprise solution with licensing management",
                Version = "1.0.0",
                Audit = new AuditInfo
                {
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.UtcNow
                },
                Workflow = new WorkflowInfo
                {
                    Status = EntityStatus.Approved
                }
            };

            var createdProduct = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Create product tiers
            var tiers = new[]
            {
                new ProductTier
                {
                    TierId = Guid.NewGuid(),
                    ProductId = createdProduct.ProductId,
                    Name = "Basic",
                    Description = "Basic tier with essential features",
                    DisplayOrder = 1,
                    SupportSLA = ProductSupportSLA.NoSLA,
                    Audit = new AuditInfo
                    {
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedOn = DateTime.UtcNow
                    },
                    Workflow = new WorkflowInfo
                    {
                        Status = EntityStatus.Approved
                    }
                },
                new ProductTier
                {
                    TierId = Guid.NewGuid(),
                    ProductId = createdProduct.ProductId,
                    Name = "Professional",
                    Description = "Professional tier with advanced features",
                    DisplayOrder = 2,
                    SupportSLA = new ProductSupportSLA
                    {
                        SlaId = "standard",
                        Name = "Standard Support",
                        Description = "Standard business hours support",
                        CriticalResponseTime = TimeSpan.FromHours(4),
                        HighPriorityResponseTime = TimeSpan.FromHours(8),
                        MediumPriorityResponseTime = TimeSpan.FromHours(24),
                        LowPriorityResponseTime = TimeSpan.FromHours(72)
                    },
                    Audit = new AuditInfo
                    {
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedOn = DateTime.UtcNow
                    },
                    Workflow = new WorkflowInfo
                    {
                        Status = EntityStatus.Approved
                    }
                },
                new ProductTier
                {
                    TierId = Guid.NewGuid(),
                    ProductId = createdProduct.ProductId,
                    Name = "Enterprise",
                    Description = "Enterprise tier with unlimited features",
                    DisplayOrder = 3,
                    SupportSLA = new ProductSupportSLA
                    {
                        SlaId = "premium",
                        Name = "Premium Support",
                        Description = "24/7 premium support with priority response",
                        CriticalResponseTime = TimeSpan.FromHours(1),
                        HighPriorityResponseTime = TimeSpan.FromHours(2),
                        MediumPriorityResponseTime = TimeSpan.FromHours(8),
                        LowPriorityResponseTime = TimeSpan.FromHours(24)
                    },
                    Audit = new AuditInfo
                    {
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedOn = DateTime.UtcNow
                    },
                    Workflow = new WorkflowInfo
                    {
                        Status = EntityStatus.Approved
                    }
                }
            };

            foreach (var tier in tiers)
            {
                await _unitOfWork.ProductTiers.AddAsync(tier);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {ProductCount} products and {TierCount} tiers", 
                1, tiers.Length);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding enterprise products");
            return false;
        }
    }

    public async Task<bool> IsSeededAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            return products.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if enterprise products are seeded");
            return false;
        }
    }
}
