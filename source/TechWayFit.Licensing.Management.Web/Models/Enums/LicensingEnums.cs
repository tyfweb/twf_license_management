namespace TechWayFit.Licensing.Management.Web.Models.Enums
{
    /// <summary>
    /// Product types available in the licensing system
    /// </summary>
    public enum ProductType
    {
        Software = 1,
        Service = 2,
        API = 3,
        Library = 4,
        Application = 5,
        Platform = 6
    }

    /// <summary>
    /// License tiers for product licensing
    /// </summary>
    public enum LicenseTier
    {
        Community = 1,
        Professional = 2,
        Enterprise = 3,
        Premium = 4
    }

    /// <summary>
    /// License status values
    /// </summary>
    public enum LicenseStatus
    {
        Active = 1,
        Expired = 2,
        Suspended = 3,
        Revoked = 4,
        Pending = 5
    }

    /// <summary>
    /// Feature categories for product features
    /// </summary>
    public enum FeatureCategory
    {
        Core = 1,
        Analytics = 2,
        Integration = 3,
        Security = 4,
        Performance = 5,
        UserInterface = 6,
        Workflow = 7,
        Collaboration = 8,
        Storage = 9,
        Support = 10,
        BusinessIntelligence = 11
    }
}
