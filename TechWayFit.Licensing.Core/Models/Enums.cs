namespace TechWayFit.Licensing.Core.Models
{
    /// <summary>
    /// Available license tiers with increasing feature sets
    /// </summary>
    public enum LicenseTier
    {
        /// <summary>
        /// Community edition with basic features
        /// </summary>
        Community = 0,

        /// <summary>
        /// Professional edition with advanced features
        /// </summary>
        Professional = 1,

        /// <summary>
        /// Enterprise edition with all features
        /// </summary>
        Enterprise = 2,

        /// <summary>
        /// Custom license with specific feature combinations
        /// </summary>
        Custom = 99
    }

    /// <summary>
    /// Categories for organizing license features
    /// </summary>
    public enum FeatureCategory
    {
        /// <summary>
        /// Core API Gateway functionality
        /// </summary>
        Core = 0,

        /// <summary>
        /// Security and authentication features
        /// </summary>
        Security = 1,

        /// <summary>
        /// Monitoring and analytics features
        /// </summary>
        Monitoring = 2,

        /// <summary>
        /// Performance and scaling features
        /// </summary>
        Performance = 3,

        /// <summary>
        /// Integration and connectivity features
        /// </summary>
        Integration = 4,

        /// <summary>
        /// Management and administration features
        /// </summary>
        Management = 5,

        /// <summary>
        /// Developer tools and utilities
        /// </summary>
        Developer = 6,

        /// <summary>
        /// Business intelligence and reporting
        /// </summary>
        BusinessIntelligence = 7,

        /// <summary>
        /// Custom or third-party features
        /// </summary>
        Custom = 99
    }

    /// <summary>
    /// Status of license in its lifecycle
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>
        /// License is valid and active
        /// </summary>
        Valid = 0,

        /// <summary>
        /// License is currently active and in use
        /// </summary>
        Active = 0, // Alias for Valid

        /// <summary>
        /// License has been generated but not yet activated
        /// </summary>
        Pending = 1,

        /// <summary>
        /// License has expired
        /// </summary>
        Expired = 2,

        /// <summary>
        /// License is not yet valid (future start date)
        /// </summary>
        NotYetValid = 3,

        /// <summary>
        /// License signature is invalid or tampered
        /// </summary>
        Invalid = 4,

        /// <summary>
        /// License format is corrupted or unreadable
        /// </summary>
        Corrupted = 5,

        /// <summary>
        /// License file not found
        /// </summary>
        NotFound = 6,

        /// <summary>
        /// License validation service unavailable
        /// </summary>
        ServiceUnavailable = 7,

        /// <summary>
        /// License is in grace period after expiry
        /// </summary>
        GracePeriod = 8,

        /// <summary>
        /// License has been manually invalidated/revoked
        /// </summary>
        Revoked = 9,

        /// <summary>
        /// License is temporarily disabled
        /// </summary>
        Suspended = 10,

        /// <summary>
        /// License is awaiting renewal approval
        /// </summary>
        RenewalPending = 11,

        /// <summary>
        /// License has been archived (historical record)
        /// </summary>
        Archived = 12
    }

    /// <summary>
    /// Product types supported by the licensing system
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// API Gateway product
        /// </summary>
        ApiGateway = 0,

        /// <summary>
        /// Web Application product
        /// </summary>
        WebApplication = 1,

        /// <summary>
        /// Enterprise Alerts module
        /// </summary>
        EnterpriseAlerts = 2,

        /// <summary>
        /// Enterprise Backup module
        /// </summary>
        EnterpriseBackup = 3,

        /// <summary>
        /// Enterprise Client Management module
        /// </summary>
        EnterpriseClientMgmt = 4,

        /// <summary>
        /// Enterprise Monitoring module
        /// </summary>
        EnterpriseMonitoring = 5,

        /// <summary>
        /// Enterprise User Management module
        /// </summary>
        EnterpriseUserMgmt = 6
    }

    /// <summary>
    /// License operation types for audit trail
    /// </summary>
    public enum LicenseOperation
    {
        /// <summary>
        /// License was created
        /// </summary>
        Created = 0,

        /// <summary>
        /// License was modified
        /// </summary>
        Modified = 1,

        /// <summary>
        /// License was activated
        /// </summary>
        Activated = 2,

        /// <summary>
        /// License was renewed
        /// </summary>
        Renewed = 3,

        /// <summary>
        /// License was revoked
        /// </summary>
        Revoked = 4,

        /// <summary>
        /// License was suspended
        /// </summary>
        Suspended = 5,

        /// <summary>
        /// License was archived
        /// </summary>
        Archived = 6,

        /// <summary>
        /// License was downloaded
        /// </summary>
        Downloaded = 7,

        /// <summary>
        /// License was validated
        /// </summary>
        Validated = 8
    }

    /// <summary>
    /// Revocation reasons for audit and compliance
    /// </summary>
    public enum RevocationReason
    {
        /// <summary>
        /// Security breach detected
        /// </summary>
        SecurityBreach = 0,

        /// <summary>
        /// Non-payment of fees
        /// </summary>
        NonPayment = 1,

        /// <summary>
        /// Terms and conditions violation
        /// </summary>
        TermsViolation = 2,

        /// <summary>
        /// End of contract period
        /// </summary>
        EndOfContract = 3,

        /// <summary>
        /// License superseded by newer version
        /// </summary>
        Superseded = 4,

        /// <summary>
        /// Technical issue or problem
        /// </summary>
        TechnicalIssue = 5,

        /// <summary>
        /// Other reason not specified above
        /// </summary>
        Other = 99
    }
}
