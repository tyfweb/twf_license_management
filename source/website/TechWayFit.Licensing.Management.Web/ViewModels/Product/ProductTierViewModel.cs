using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    public class ProductTierViewModel
    {
        public Guid Id { get; set; }
        public Guid TierId { get; set; }  // Match the entity property name
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LicenseTier Tier { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Fields that exist in the core entity
        public int DisplayOrder { get; set; }
        public ProductSupportSLA SupportSLA { get; set; } = ProductSupportSLA.NoSLA;
        public int MaxUsers { get; set; }
        public int MaxDevices { get; set; }
        
        // SLA Form Binding Properties
        [StringLength(100)]
        public string SLAName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string SLADescription { get; set; } = string.Empty;
        
        [Display(Name = "Critical Response Time (Hours)")]
        public double CriticalResponseHours { get; set; } = 1.0;
        
        [Display(Name = "High Priority Response Time (Hours)")]
        public double HighPriorityResponseHours { get; set; } = 4.0;
        
        [Display(Name = "Medium Priority Response Time (Hours)")]
        public double MediumPriorityResponseHours { get; set; } = 8.0;
        
        [Display(Name = "Low Priority Response Time (Hours)")]
        public double LowPriorityResponseHours { get; set; } = 24.0;
        
        // Navigation properties - Prices are handled via the Prices collection from the core model
        public List<ProductTierPriceViewModel> Prices { get; set; } = new();
        
        // Form binding helpers for individual price types
        public decimal? MonthlyPriceAmount { get; set; }
        public string MonthlyPriceCurrency { get; set; } = "USD";
        public decimal? AnnualPriceAmount { get; set; }
        public string AnnualPriceCurrency { get; set; } = "USD";
        
        // Display helpers
        public string TierDisplay => Tier.ToString();
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
        public string SupportSLADisplay => SupportSLA?.Name ?? "No SLA";
    }
    
    public class ProductTierPriceViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductTierId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PriceType PriceType { get; set; }
        public string DisplayOrder { get; set; } = string.Empty;
        public ProductSupportSLA SupportSLA { get; set; } = ProductSupportSLA.NoSLA;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Display helpers
        public string FormattedPrice => $"{Currency} {Amount:F2}";
        public string PriceTypeDisplay => PriceType.ToString();
        public string SupportSLADisplay => SupportSLA?.Name ?? "No SLA";
    }
    
    public enum PriceType
    {
        OneTime,
        Monthly,
        Yearly,
        Custom
    }
    
    public class ProductTierListViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<ProductTierViewModel> Tiers { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public bool ShowInactive { get; set; }
        
        // Stats
        public int TotalTiers => Tiers.Count;
        public int ActiveTiers => Tiers.Count(t => t.IsActive);
        public int InactiveTiers => Tiers.Count(t => !t.IsActive);
    }
}
