using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Report;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Report
{
    /// <summary>
    /// ViewModel for generating and viewing license usage reports
    /// </summary>
    public class LicenseUsageReportViewModel
    {
        public Guid ReportId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Report date is required")]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "From date is required")]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-1);

        [Required(ErrorMessage = "To date is required")]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Total Active Licenses")]
        public int TotalActiveLicenses { get; set; }

        [Display(Name = "Total Expired Licenses")]
        public int TotalExpiredLicenses { get; set; }

        [Display(Name = "Total Trial Licenses")]
        public int TotalTrialLicenses { get; set; }

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Top Product by Usage")]
        public string? TopProductByUsage { get; set; }

        [Display(Name = "Top Consumer by Licenses")]
        public string? TopConsumerByLicenses { get; set; }

        [Display(Name = "Average License Duration (Days)")]
        public double AverageLicenseDuration { get; set; }

        public List<ProductUsageSummary> ProductUsage { get; set; } = new();
        public List<ConsumerUsageSummary> ConsumerUsage { get; set; } = new();
        public List<TierUsageSummary> TierUsage { get; set; } = new();

        [Display(Name = "Date Range")]
        public string DateRange => $"{FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}";

        [Display(Name = "Total Licenses")]
        public int TotalLicenses => TotalActiveLicenses + TotalExpiredLicenses + TotalTrialLicenses;
    }

    /// <summary>
    /// ViewModel for license expiration reports
    /// </summary>
    public class LicenseExpirationReportViewModel
    {
        public Guid ReportId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Report date is required")]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        [Range(1, 365, ErrorMessage = "Days ahead must be between 1 and 365")]
        [Display(Name = "Days Ahead")]
        public int DaysAhead { get; set; } = 30;

        [Display(Name = "Expiring Soon Count")]
        public int ExpiringSoonCount { get; set; }

        [Display(Name = "Already Expired Count")]
        public int AlreadyExpiredCount { get; set; }

        [Display(Name = "Critical Expirations (≤7 days)")]
        public int CriticalExpirations { get; set; }

        [Display(Name = "Total Potential Revenue at Risk")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenueAtRisk { get; set; }

        public List<ExpiringLicenseSummary> ExpiringLicenses { get; set; } = new();
        public List<ExpiredLicenseSummary> ExpiredLicenses { get; set; } = new();

        [Display(Name = "Report Period")]
        public string ReportPeriod => $"Next {DaysAhead} days from {ReportDate:yyyy-MM-dd}";
    }

    /// <summary>
    /// ViewModel for revenue reports
    /// </summary>
    public class RevenueReportViewModel
    {
        public Guid ReportId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Report date is required")]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "From date is required")]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-12);

        [Required(ErrorMessage = "To date is required")]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Recurring Revenue")]
        [DataType(DataType.Currency)]
        public decimal RecurringRevenue { get; set; }

        [Display(Name = "New Customer Revenue")]
        [DataType(DataType.Currency)]
        public decimal NewCustomerRevenue { get; set; }

        [Display(Name = "Trial Conversion Revenue")]
        [DataType(DataType.Currency)]
        public decimal TrialConversionRevenue { get; set; }

        [Display(Name = "Top Revenue Product")]
        public string? TopRevenueProduct { get; set; }

        [Display(Name = "Top Revenue Consumer")]
        public string? TopRevenueConsumer { get; set; }

        [Display(Name = "Average Revenue Per Customer")]
        [DataType(DataType.Currency)]
        public decimal AverageRevenuePerCustomer { get; set; }

        public List<MonthlyRevenueSummary> MonthlyRevenue { get; set; } = new();
        public List<ProductRevenueSummary> ProductRevenue { get; set; } = new();
        public List<TierRevenueSummary> TierRevenue { get; set; } = new();

        [Display(Name = "Date Range")]
        public string DateRange => $"{FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}";

        [Display(Name = "Growth Rate")]
        public string GrowthRate { get; set; } = "0%";
    }

    /// <summary>
    /// ViewModel for dashboard analytics
    /// </summary>
    public class DashboardAnalyticsViewModel
    {
        [Display(Name = "Total Active Licenses")]
        public int TotalActiveLicenses { get; set; }

        [Display(Name = "Total Products")]
        public int TotalProducts { get; set; }

        [Display(Name = "Total Consumers")]
        public int TotalConsumers { get; set; }

        [Display(Name = "Total Revenue (This Month)")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenueThisMonth { get; set; }

        [Display(Name = "Licenses Expiring Soon")]
        public int LicensesExpiringSoon { get; set; }

        [Display(Name = "New Licenses This Month")]
        public int NewLicensesThisMonth { get; set; }

        [Display(Name = "Trial Licenses")]
        public int TrialLicenses { get; set; }

        [Display(Name = "Active Users")]
        public int ActiveUsers { get; set; }

        // Recent activity
        public List<RecentActivityItem> RecentActivities { get; set; } = new();

        // Charts data
        public List<ChartDataPoint> LicenseStatusChart { get; set; } = new();
        public List<ChartDataPoint> RevenueChart { get; set; } = new();
        public List<ChartDataPoint> ProductUsageChart { get; set; } = new();

        [Display(Name = "License Health Score")]
        public int LicenseHealthScore { get; set; }

        [Display(Name = "Revenue Growth")]
        public string RevenueGrowth { get; set; } = "0%";
    }

    // Supporting classes
    public class ProductUsageSummary
    {
        public string ProductName { get; set; } = string.Empty;
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ConsumerUsageSummary
    {
        public string ConsumerName { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TierUsageSummary
    {
        public string TierName { get; set; } = string.Empty;
        public int LicenseCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ExpiringLicenseSummary
    {
        public Guid LicenseId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ExpiredLicenseSummary
    {
        public Guid LicenseId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysExpired { get; set; }
        public decimal PotentialLostRevenue { get; set; }
    }

    public class MonthlyRevenueSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }

    public class ProductRevenueSummary
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int LicenseCount { get; set; }
    }

    public class TierRevenueSummary
    {
        public string TierName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int LicenseCount { get; set; }
    }

    public class RecentActivityItem
    {
        public string Activity { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
