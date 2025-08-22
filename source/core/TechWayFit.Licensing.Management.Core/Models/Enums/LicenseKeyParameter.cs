namespace TechWayFit.Licensing.Management.Core.Models.Enums;

/// <summary>
/// Enum defining all possible license key parameters used across different license strategies
/// </summary>
public enum LicenseKeyParameter
{
    // Core License Properties
    LicenseType,
    KeyFormat,
    ProductKey,
    BaseKey,
    VolumetricKey,
    
    // Activation & Validation
    OnlineActivation,
    OfflineActivation,
    RequiresActivation,
    RequireOnlineValidation,
    SupportOfflineValidation,
    AllowOfflineGracePeriod,
    
    // URLs & Endpoints
    ActivationUrl,
    ValidationUrl,
    UserTrackingUrl,
    UsageReportingUrl,
    
    // Device & Machine Management
    MaxActivations,
    MaxMachineBindings,
    SupportMachineBinding,
    
    // User Management (Volumetric)
    MaxUsers,
    SupportsConcurrentUsers,
    UserSlotAllocation,
    ConcurrentUserLimit,
    SupportUserPooling,
    SupportUserManagement,
    
    // Usage & Tracking
    UsageTracking,
    SupportUsageReporting,
    SupportDynamicScaling,
    
    // License File Properties
    LicenseFileFormat,
    GenerateLicenseFile,
    IncludePublicKey,
    IncludeProductMetadata,
    
    // Key Management Features
    SupportKeyDeactivation,
    SupportKeyTransfer
}
