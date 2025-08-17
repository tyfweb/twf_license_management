using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for managing license files separately from the core license data
/// This entity handles file storage, download tracking, and file metadata
/// </summary>
[Table("license_files")]
public class LicenseFileEntity : AuditWorkflowEntity
{
    #region BaseProperties
    /// <summary>
    /// Foreign key to the ProductLicense
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Name of the license file
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Full path to the license file in storage
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long FileSizeBytes { get; set; } = 0;

    /// <summary>
    /// MIME content type of the file
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// SHA-256 hash of the file for integrity verification
    /// </summary>
    public string FileHash { get; set; } = string.Empty;
    #endregion

    #region DownloadManagement
    /// <summary>
    /// Number of times this file has been downloaded
    /// </summary>
    public int DownloadCount { get; set; } = 0;

    /// <summary>
    /// Last time this file was downloaded
    /// </summary>
    public DateTime? LastDownloadedAt { get; set; }

    /// <summary>
    /// User who last downloaded this file
    /// </summary>
    public string? LastDownloadedBy { get; set; }

    /// <summary>
    /// Expiration time for file access (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Maximum number of downloads allowed (null = unlimited)
    /// </summary>
    public int? DownloadLimit { get; set; }

    /// <summary>
    /// Whether downloads are currently enabled for this file
    /// </summary>
    public bool IsDownloadEnabled { get; set; } = true;
    #endregion

    #region FileContent
    /// <summary>
    /// Version of the license file format
    /// </summary>
    public string FileVersion { get; set; } = "1.0";

    /// <summary>
    /// Type of license file (JSON, XML, Binary, etc.)
    /// </summary>
    public string FileType { get; set; } = "JSON";

    /// <summary>
    /// Encryption method used for the license file
    /// </summary>
    public string Encryption { get; set; } = "AES256";

    /// <summary>
    /// Signature algorithm used for the license file
    /// </summary>
    public string Signature { get; set; } = "SHA256";

    /// <summary>
    /// License file signature for integrity verification
    /// </summary>
    public string LicenseSignature { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about the file (JSON)
    /// </summary>
    public string FileMetadata { get; set; } = "{}";
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to the ProductLicense
    /// </summary>
    public virtual ProductLicenseEntity? License { get; set; }
    #endregion
}
