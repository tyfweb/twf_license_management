```mermaid
classDiagram
    %% Core Domain Models
    
    %% Consumer Domain
    class ConsumerAccount {
        +string ConsumerId
        +string CompanyName
        +ContactPerson PrimaryContact
        +ContactPerson? SecondaryContact
        +DateTime CreatedAt
        +DateTime ActivatedAt
        +bool IsActive
        +Address Address
        +string Notes
        +ConsumerStatus Status
    }
    
    class ContactPerson {
        +string Name
        +string Email
        +string Phone
        +string Position
    }
    
    class Address {
        +string Street
        +string City
        +string State
        +string PostalCode
        +string Country
    }
    
    class ProductConsumer {
        +ConsumerAccount Consumer
        +EnterpriseProduct Product
        +ContactPerson AccountManager
    }
    
    class ConsumerStatus {
        <<enumeration>>
        Prospect
        Active
        Inactive
        Suspended
    }
    
    %% Product Domain
    class EnterpriseProduct {
        +string ProductId
        +string Name
        +string Description
        +string Version
        +DateTime ReleaseDate
        +IEnumerable~ProductVersion~ Versions
        +IEnumerable~ProductTier~ Tiers
        +string SupportEmail
        +string SupportPhone
        +DateTime? DecommissionDate
        +ProductStatus Status
    }
    
    class ProductVersion {
        +string ProductId
        +string VersionId
        +bool IsCurrent
        +SemanticVersion Version
        +DateTime ReleaseDate
        +DateTime? EndOfLifeDate
        +DateTime? SupportEndDate
        +string ChangeLog
    }
    
    class ProductTier {
        +string ProductId
        +string TierId
        +string Name
        +string Description
        +List~ProductFeature~ Features
        +ProductSupportSLA SupportSLA
        +Money Price
        +int MaxUsers
        +int MaxDevices
        +bool IsActive
        +Default() ProductTier$
    }
    
    class ProductFeature {
        +string ProductId
        +string TierId
        +string FeatureId
        +string Code
        +int DisplayOrder
        +string Name
        +string Description
        +bool IsEnabled
        +ProductFeatureUsage Usage
        +SemanticVersion SupportFromVersion
        +Default() ProductFeature$
    }
    
    class ProductFeatureUsage {
        +int MaxUsage
        +int MaxConcurrentUsage
        +bool IsUnlimited
        +bool AllowGracePeriod
        +DateTime? ExpirationDate
        +Default() ProductFeatureUsage$
        +NoLimit() ProductFeatureUsage$
    }
    
    class ProductSupportSLA {
        +string SlaId
        +string Name
        +string Description
        +TimeSpan CriticalResponseTime
        +TimeSpan HighPriorityResponseTime
        +TimeSpan MediumPriorityResponseTime
        +TimeSpan LowPriorityResponseTime
        +NoSLA() ProductSupportSLA$
    }
    
    class Money {
        +decimal Amount
        +string Currency
        +ToString() string
        +Parse(string) Money$
        +Zero() Money$
        +operator+(Money, Money) Money
        +operator-(Money, Money) Money
        +operator*(Money, decimal) Money
    }
    
    class ProductStatus {
        <<enumeration>>
        PreRelease
        Active
        Inactive
        Deprecated
        Decommissioned
    }
    
    class FeatureUsageStatistics {
        +int TotalFeatures
        +Dictionary~string,int~ FeaturesByType
        +Dictionary~string,int~ MostUsedFeatures
        +Dictionary~string,int~ FeaturesByTier
    }
    
    %% License Domain
    class ProductLicense {
        +string LicenseId
        +ProductConsumer LicenseConsumer
        +ProductTier Tier
        +DateTime ValidFrom
        +DateTime ValidTo
        +DateTime CreatedAt
        +string Encryption
        +string Signature
        +string LicenseKey
        +string PublicKey
        +string LicenseSignature
        +DateTime KeyGeneratedAt
        +LicenseStatus Status
        +string IssuedBy
        +DateTime? RevokedAt
        +string? RevocationReason
        +Dictionary~string,string~ Metadata
        +bool IsValid
        +bool IsExpired
        +int DaysUntilExpiry
    }
    
    class LicenseGenerationRequest {
        +string ProductId
        +string ConsumerId
        +string? TierId
        +DateTime? ExpiryDate
        +int? MaxUsers
        +int? MaxDevices
        +bool AllowOfflineUsage
        +bool AllowVirtualization
        +string? Notes
        +Dictionary~string,object~ CustomProperties
        +Dictionary~string,object~ Metadata
    }
    
    class LicenseUpdateRequest {
        +DateTime? ExpiryDate
        +int? MaxUsers
        +int? MaxDevices
        +bool? AllowOfflineUsage
        +bool? AllowVirtualization
        +string? Notes
        +Dictionary~string,object~? CustomProperties
        +Dictionary~string,object~? Metadata
    }
    
    class LicenseValidationResult {
        +bool IsValid
        +string? ErrorMessage
        +ProductLicense? License
        +Dictionary~string,object~ ValidationDetails
    }
    
    class LicenseAuditEntry {
        +string EntryId
        +string LicenseId
        +string Action
        +string? OldValue
        +string? NewValue
        +string ModifiedBy
        +DateTime ModifiedDate
        +string? Reason
        +Dictionary~string,object~ Metadata
    }
    
    class LicenseUsageStatistics {
        +int TotalLicenses
        +int ActiveLicenses
        +int ExpiredLicenses
        +int RevokedLicenses
        +int SuspendedLicenses
        +int ExpiringInNext30Days
        +Dictionary~LicenseStatus,int~ LicensesByStatus
        +Dictionary~string,int~ LicensesByProduct
    }
    
    class LicenseStatus {
        <<enumeration>>
        Inactive
        Active
        Expired
        Suspended
        Revoked
        Pending
        Unknown
    }
    
    %% Notification Domain
    class NotificationTemplate {
        +string TemplateId
        +string TemplateName
        +NotificationType NotificationType
        +NotificationPreferences Preferences
        +string Subject
        +string MessageTemplate
        +bool IsActive
        +string CreatedBy
        +DateTime CreatedDate
        +Dictionary~string,object~ TemplateVariables
    }
    
    class NotificationPreferences {
        +NotificationMode Mode
    }
    
    class NotificationHistory {
        +string NotificationId
        +string EntityId
        +string EntityType
        +NotificationMode NotificationMode
        +string NotificationTemplateId
        +NotificationType NotificationType
        +NotificationPreferences Recipients
        +DateTime SentDate
        +DeliveryStatus DeliveryStatus
        +string? DeliveryError
    }
    
    class NotificationStatistics {
        +int TotalNotifications
        +int SuccessfulDeliveries
        +int FailedDeliveries
        +Dictionary~NotificationType,int~ NotificationsByType
        +Dictionary~DateTime,int~ NotificationsByDate
        +double DeliverySuccessRate
    }
    
    class NotificationType {
        <<enumeration>>
        LicenseExpiration
        LicenseActivation
        LicenseRevocation
        LicenseRenewal
        LicenseSuspension
        ValidationFailure
        UsageThreshold
        SystemAlert
        Custom
    }
    
    class NotificationMode {
        <<enumeration>>
        Email
        Sms
        AppNotification
    }
    
    class DeliveryStatus {
        <<enumeration>>
        Pending
        Sent
        Failed
    }
    
    %% Audit Domain
    class AuditEntry {
        +string EntryId
        +string EntityType
        +string EntityId
        +string ActionType
        +string? OldValue
        +string? NewValue
        +string UserName
        +DateTime Timestamp
        +string? IpAddress
        +string? UserAgent
        +string? Reason
        +Dictionary~string,string~ Metadata
    }
    
    class AuditStatistics {
        +int TotalEntries
        +Dictionary~string,int~ EntriesByAction
        +Dictionary~string,int~ EntriesByEntity
        +Dictionary~string,int~ EntriesByUser
        +Dictionary~DateTime,int~ EntriesByDate
        +int UniqueUsers
        +int UniqueEntities
    }
    
    %% Common Domain
    class ValidationResult {
        +bool IsValid
        +List~string~ Errors
        +Success() ValidationResult$
        +Failure(string[]) ValidationResult$
    }
    
    class SemanticVersion {
        +int Major
        +int Minor
        +int Patch
        +string PreRelease
        +ToString() string
        +Parse(string) SemanticVersion$
        +Default() SemanticVersion$
        +Latest() SemanticVersion$
        +Stable() SemanticVersion$
        +operator>(SemanticVersion, SemanticVersion) bool
        +operator<(SemanticVersion, SemanticVersion) bool
    }
    
    %% Report Domain
    class LicenseUsageReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +int TotalActiveLicenses
        +int TotalInactiveLicenses
        +int NewLicensesIssued
        +int LicensesExpired
        +int LicensesRevoked
        +Dictionary~string,int~ UsageByProduct
        +Dictionary~string,int~ UsageByConsumer
        +Dictionary~LicenseStatus,int~ UsageByStatus
    }
    
    class LicenseExpirationReport {
        +DateTime ReportDate
        +int DaysAhead
        +IEnumerable~ExpiringLicense~ ExpiringLicenses
        +Dictionary~int,int~ ExpirationsByDays
        +Dictionary~string,int~ ExpirationsByProduct
    }
    
    class ExpiringLicense {
        +string LicenseId
        +string LicenseKey
        +string ProductName
        +string ConsumerName
        +DateTime ExpiryDate
        +int DaysUntilExpiry
        +LicenseStatus Status
    }
    
    class RevenueReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +decimal TotalRevenue
        +ReportGroupBy GroupBy
        +Dictionary~DateTime,decimal~ RevenueByPeriod
        +Dictionary~string,decimal~ RevenueByProduct
        +Dictionary~string,decimal~ RevenueByTier
    }
    
    class ProductPerformanceReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +IEnumerable~ProductPerformance~ ProductPerformances
    }
    
    class ProductPerformance {
        +string ProductId
        +string ProductName
        +int TotalLicenses
        +int ActiveLicenses
        +int NewLicenses
        +int RenewedLicenses
        +decimal Revenue
        +double CustomerSatisfaction
    }
    
    class ConsumerActivityReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +IEnumerable~ConsumerActivity~ ConsumerActivities
    }
    
    class ConsumerActivity {
        +string ConsumerId
        +string ConsumerName
        +int TotalLicenses
        +int ActiveLicenses
        +DateTime LastActivity
        +int ValidationAttempts
        +int SuccessfulValidations
        +decimal TotalSpent
    }
    
    class ComplianceAuditReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +ComplianceType ComplianceType
        +double ComplianceScore
        +IEnumerable~ComplianceIssue~ ComplianceIssues
        +Dictionary~string,bool~ ComplianceChecks
    }
    
    class ComplianceIssue {
        +string IssueId
        +string IssueType
        +string Description
        +string Severity
        +string EntityId
        +DateTime DetectedDate
        +string? Resolution
        +bool IsResolved
    }
    
    class LicenseViolationsReport {
        +DateTime ReportDate
        +DateTime FromDate
        +DateTime ToDate
        +int TotalViolations
        +IEnumerable~LicenseViolation~ Violations
        +Dictionary~ViolationType,int~ ViolationsByType
    }
    
    class LicenseViolation {
        +string ViolationId
        +ViolationType ViolationType
        +string LicenseId
        +string ConsumerId
        +string Description
        +DateTime DetectedDate
        +string Severity
        +bool IsResolved
        +string? Resolution
    }
    
    class DashboardAnalytics {
        +DateTime ReportDate
        +DateRange DateRange
        +int TotalLicenses
        +int ActiveLicenses
        +int ExpiringLicenses
        +int NewLicenses
        +decimal TotalRevenue
        +int TotalConsumers
        +int ActiveProducts
        +Dictionary~string,int~ TopProducts
        +Dictionary~DateTime,int~ LicensesTrend
        +Dictionary~DateTime,decimal~ RevenueTrend
    }
    
    class CustomReportParameters {
        +string ReportName
        +DateTime FromDate
        +DateTime ToDate
        +List~string~ SelectedFields
        +Dictionary~string,object~ Filters
        +string? GroupBy
        +string? SortBy
        +bool SortDescending
        +int? MaxRecords
    }
    
    class CustomReportData {
        +string ReportName
        +DateTime GeneratedDate
        +CustomReportParameters Parameters
        +IEnumerable~Dictionary~string,object~~ Data
        +int TotalRecords
    }
    
    class ReportSchedule {
        +string ScheduleId
        +string ReportType
        +string ReportName
        +Dictionary~string,object~ Parameters
        +string CronExpression
        +IEnumerable~string~ Recipients
        +ExportFormat ExportFormat
        +bool IsActive
        +string CreatedBy
        +DateTime CreatedDate
        +DateTime? LastExecuted
        +DateTime? NextExecution
    }
    
    class ReportExecution {
        +string ExecutionId
        +string ReportType
        +string? ScheduleId
        +DateTime StartTime
        +DateTime? EndTime
        +string Status
        +string? ErrorMessage
        +int RecordsProcessed
        +string ExecutedBy
        +Dictionary~string,object~ Parameters
    }
    
    %% Report Enums
    class ReportGroupBy {
        <<enumeration>>
        Day
        Week
        Month
        Quarter
        Year
    }
    
    class DateRange {
        <<enumeration>>
        Today
        Yesterday
        Last7Days
        Last30Days
        LastQuarter
        LastYear
        Custom
    }
    
    class ExportFormat {
        <<enumeration>>
        PDF
        Excel
        CSV
        JSON
    }
    
    class ComplianceType {
        <<enumeration>>
        All
        LicenseCompliance
        SecurityCompliance
        AuditCompliance
        DataRetention
    }
    
    class ViolationType {
        <<enumeration>>
        UnauthorizedUsage
        ExceededLimits
        ExpiredLicense
        InvalidActivation
        TamperingDetected
    }
    
    %% Relationships
    ConsumerAccount ||--|| Address : has
    ConsumerAccount ||--|| ContactPerson : primary_contact
    ConsumerAccount ||--o| ContactPerson : secondary_contact
    ConsumerAccount ||--|| ConsumerStatus : status
    
    ProductConsumer ||--|| ConsumerAccount : consumer
    ProductConsumer ||--|| EnterpriseProduct : product
    ProductConsumer ||--|| ContactPerson : account_manager
    
    EnterpriseProduct ||--o{ ProductVersion : versions
    EnterpriseProduct ||--o{ ProductTier : tiers
    EnterpriseProduct ||--|| ProductStatus : status
    
    ProductVersion ||--|| SemanticVersion : version
    
    ProductTier ||--o{ ProductFeature : features
    ProductTier ||--|| ProductSupportSLA : support_sla
    ProductTier ||--|| Money : price
    
    ProductFeature ||--|| ProductFeatureUsage : usage
    ProductFeature ||--|| SemanticVersion : support_from_version
    
    ProductLicense ||--|| ProductConsumer : license_consumer
    ProductLicense ||--|| ProductTier : tier
    ProductLicense ||--|| LicenseStatus : status
    
    NotificationTemplate ||--|| NotificationType : notification_type
    NotificationTemplate ||--|| NotificationPreferences : preferences
    
    NotificationHistory ||--|| NotificationMode : notification_mode
    NotificationHistory ||--|| NotificationType : notification_type
    NotificationHistory ||--|| NotificationPreferences : recipients
    NotificationHistory ||--|| DeliveryStatus : delivery_status
    
    NotificationPreferences ||--|| NotificationMode : mode
    
    LicenseExpirationReport ||--o{ ExpiringLicense : expiring_licenses
    ExpiringLicense ||--|| LicenseStatus : status
    
    ProductPerformanceReport ||--o{ ProductPerformance : product_performances
    
    ConsumerActivityReport ||--o{ ConsumerActivity : consumer_activities
    
    ComplianceAuditReport ||--|| ComplianceType : compliance_type
    ComplianceAuditReport ||--o{ ComplianceIssue : compliance_issues
    
    LicenseViolationsReport ||--o{ LicenseViolation : violations
    LicenseViolation ||--|| ViolationType : violation_type
    
    DashboardAnalytics ||--|| DateRange : date_range
    
    CustomReportData ||--|| CustomReportParameters : parameters
    
    ReportSchedule ||--|| ExportFormat : export_format
    
    RevenueReport ||--|| ReportGroupBy : group_by
    
    LicenseUsageReport ||--|| LicenseStatus : status_mapping
```