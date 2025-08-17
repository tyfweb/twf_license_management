namespace TechWayFit.Licensing.Management.Core.Models.Enums;

/// <summary>
/// Enumeration of license types supported by the system
/// </summary>
public enum LicenseType
{
    /// <summary>
    /// Product License File - Offline activation with downloadable license files
    /// </summary>
    ProductLicenseFile = 1,

    /// <summary>
    /// Product Key - Online activation with XXXX-XXXX-XXXX-XXXX format keys
    /// </summary>
    ProductKey = 2,

    /// <summary>
    /// Volumetric License - Multi-user keys with usage limits (XXXX-XXXX-XXXX-0001 to 9999)
    /// </summary>
    VolumetricLicense = 3
}
