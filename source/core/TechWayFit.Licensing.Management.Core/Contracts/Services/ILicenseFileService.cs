using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for license file generation and management
/// Handles the creation of downloadable license files in various formats
/// </summary>
public interface ILicenseFileService
{
    /// <summary>
    /// Generate license file in .lic format (human-readable)
    /// </summary>
    /// <param name="license">License to generate file for</param>
    /// <returns>License file content as string</returns>
    Task<string> GenerateLicenseFileAsync(ProductLicense license);

    /// <summary>
    /// Generate license file in JSON format (machine-readable)
    /// </summary>
    /// <param name="license">License to generate file for</param>
    /// <returns>JSON license content as string</returns>
    Task<string> GenerateJsonLicenseFileAsync(ProductLicense license);

    /// <summary>
    /// Generate license file in XML format (structured)
    /// </summary>
    /// <param name="license">License to generate file for</param>
    /// <returns>XML license content as string</returns>
    Task<string> GenerateXmlLicenseFileAsync(ProductLicense license);

    /// <summary>
    /// Generate complete license package as ZIP file
    /// </summary>
    /// <param name="license">License to generate package for</param>
    /// <returns>ZIP file content as byte array</returns>
    Task<byte[]> GenerateLicensePackageAsync(ProductLicense license);

    /// <summary>
    /// Generate README file for license package
    /// </summary>
    /// <param name="license">License to generate README for</param>
    /// <returns>README content as string</returns>
    Task<string> GenerateReadmeAsync(ProductLicense license);

    /// <summary>
    /// Generate bulk export for multiple licenses
    /// </summary>
    /// <param name="licenses">Collection of licenses to export</param>
    /// <param name="format">Export format (zip, json, xml)</param>
    /// <returns>Bulk export file content as byte array</returns>
    Task<byte[]> GenerateBulkExportAsync(IEnumerable<ProductLicense> licenses, string format = "zip");

    /// <summary>
    /// Track license file download
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="downloadedBy">User who downloaded the file</param>
    /// <param name="format">File format downloaded</param>
    /// <returns>Success status</returns>
    Task<bool> TrackDownloadAsync(Guid licenseId, string downloadedBy, string format);

    /// <summary>
    /// Get download statistics for a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>Download statistics</returns>
    Task<LicenseDownloadStats> GetDownloadStatsAsync(Guid licenseId);

    /// <summary>
    /// Validate license file format and content
    /// </summary>
    /// <param name="fileContent">License file content</param>
    /// <param name="format">File format to validate</param>
    /// <returns>Validation result</returns>
    Task<LicenseFileValidationResult> ValidateLicenseFileAsync(string fileContent, string format);
}

/// <summary>
/// License download statistics
/// </summary>
public class LicenseDownloadStats
{
    public Guid LicenseId { get; set; }
    public int TotalDownloads { get; set; }
    public DateTime? LastDownload { get; set; }
    public string LastDownloadedBy { get; set; } = string.Empty;
    public Dictionary<string, int> FormatDownloads { get; set; } = new();
    public List<LicenseDownloadRecord> DownloadHistory { get; set; } = new();
}

/// <summary>
/// Individual download record
/// </summary>
public class LicenseDownloadRecord
{
    public DateTime DownloadDate { get; set; }
    public string DownloadedBy { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// License file validation result
/// </summary>
public class LicenseFileValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public LicenseFileMetadata? Metadata { get; set; }
}

/// <summary>
/// License file metadata
/// </summary>
public class LicenseFileMetadata
{
    public string Format { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}
