namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    LicenseExpiration,
    LicenseActivation,
    LicenseRevocation,
    LicenseRenewal,
    LicenseSuspension,
    ValidationFailure,
    UsageThreshold,
    SystemAlert,
    Custom
}
