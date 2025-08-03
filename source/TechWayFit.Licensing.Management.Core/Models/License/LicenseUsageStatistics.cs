using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License usage statistics
/// </summary>
public class LicenseUsageStatistics
{
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int RevokedLicenses { get; set; }
    public int SuspendedLicenses { get; set; }
    public int ExpiringInNext30Days { get; set; }
    public Dictionary<LicenseStatus, int> LicensesByStatus { get; set; } = new();
    public Dictionary<string, int> LicensesByProduct { get; set; } = new();

    public Dictionary<string, int> LicensesByConsumer { get; set; } = new();

    public static LicenseUsageStatistics FromModel(LicenseUsageStatistics model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new LicenseUsageStatistics
        {
            TotalLicenses = model.TotalLicenses,
            ActiveLicenses = model.ActiveLicenses,
            ExpiredLicenses = model.ExpiredLicenses,
            RevokedLicenses = model.RevokedLicenses,
            SuspendedLicenses = model.SuspendedLicenses,
            ExpiringInNext30Days = model.ExpiringInNext30Days,
            LicensesByStatus = model.LicensesByStatus.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            LicensesByProduct = model.LicensesByProduct.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            LicensesByConsumer = model.LicensesByConsumer.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}
