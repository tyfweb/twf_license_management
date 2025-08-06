using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    /// <summary>
    /// Product Feature View Model
    /// </summary>
    public class ProductFeatureViewModel
    {
        public Guid FeatureId { get; set; }
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public FeatureCategory Category { get; set; }
        public bool IsEnabled { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int? MaxUsage { get; set; }

        public string? ConfigurationSchema { get; set; }
        public LicenseTier MinimumTier { get; set; } = LicenseTier.Community;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        
        public bool CanDelete { get; set; } = true;
    }    }
