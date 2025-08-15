using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    /// <summary>
    /// View model for displaying tier information in lists and cards
    /// </summary>
    public class TierInfoViewModel
    {
        /// <summary>
        /// Tier ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tier name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tier description
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Formatted price string for display
        /// </summary>
        public string Price { get; set; } = "Free";

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Support SLA information
        /// </summary>
        public TierSupportSlaViewModel? SupportSLA { get; set; }

        /// <summary>
        /// Whether the tier is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Maximum number of users
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Maximum number of devices
        /// </summary>
        public int? MaxDevices { get; set; }

        /// <summary>
        /// Whether the tier can be deleted
        /// </summary>
        public bool CanDelete { get; set; } = true;
    }

    /// <summary>
    /// View model for tier support SLA information
    /// </summary>
    public class TierSupportSlaViewModel
    {
        /// <summary>
        /// SLA name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// SLA description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Critical response time in hours
        /// </summary>
        public double CriticalResponseHours { get; set; }

        /// <summary>
        /// High priority response time in hours
        /// </summary>
        public double HighPriorityResponseHours { get; set; }

        /// <summary>
        /// Medium priority response time in hours
        /// </summary>
        public double MediumPriorityResponseHours { get; set; }

        /// <summary>
        /// Low priority response time in hours
        /// </summary>
        public double LowPriorityResponseHours { get; set; }
    }
}
