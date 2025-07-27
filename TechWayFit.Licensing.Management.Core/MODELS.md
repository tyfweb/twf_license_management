# Models Documentation

## Overview

This document provides detailed specifications for all domain models in the TechWayFit.Licensing.Management.Core project. The models are organized by business domain and follow Domain-Driven Design principles.

## Model Organization

### Namespace Structure

```
TechWayFit.Licensing.Management.Core.Models.
├── Audit/              # Audit trail and tracking models
├── Common/             # Shared utilities and base models
├── Consumer/           # Customer and account management
├── License/            # License domain models
├── Notification/       # Notification system models
├── Product/            # Product hierarchy and features
├── Report/             # Reporting and analytics models
└── Services/           # Service-specific models
```

## Core Domain Models

### License Domain (`Models.License`)

#### **ProductLicense**
The central entity representing a software license.

**Key Properties:**
- `LicenseId` - Unique identifier
- `LicenseKey` - Cryptographically generated license key
- `ProductId` - Associated product
- `ConsumerId` - License holder
- `Status` - Current license status
- `ExpiryDate` - License expiration
- `MaxUsers`, `MaxDevices` - Usage limits
- `DigitalSignature` - Cryptographic signature
- `LicenseHash` - Integrity hash

**Security Features:**
- Cryptographic validation support
- Digital signature for tamper detection
- Audit trail integration
- Metadata for extensibility

#### **LicenseStatus Enum**
```csharp
Active, Inactive, Expired, Revoked, Suspended, PendingActivation
```

### Product Domain (`Models.Product`)

#### **EnterpriseProduct**
Represents a licensable software product.

**Key Properties:**
- `ProductId` - Unique identifier
- `ProductName` - Display name
- `ProductVersion` - Current version
- `ProductTiers` - Available licensing tiers
- `Status` - Product lifecycle status
- `ReleaseDate`, `DecommissionDate` - Lifecycle dates

**Relationships:**
- One-to-many with `ProductTier`
- Version management with custom `Version` class

#### **ProductTier**
Represents different licensing levels (e.g., Basic, Professional, Enterprise).

**Key Properties:**
- `TierId` - Unique identifier
- `TierName` - Display name (Basic, Pro, Enterprise)
- `TierLevel` - Hierarchical level
- `Features` - Available features
- `PricingInfo` - Pricing details

**Relationships:**
- Belongs to `EnterpriseProduct`
- One-to-many with `ProductFeature`

#### **ProductFeature**
Individual features available in product tiers.

**Key Properties:**
- `FeatureId` - Unique identifier
- `FeatureCode` - Technical identifier
- `FeatureName` - Display name
- `FeatureType` - Type classification
- `IsEnabled` - Availability flag

**Feature Types:**
```csharp
Core, Premium, Enterprise, Integration, Analytics, Security
```

#### **ProductStatus Enum**
```csharp
Development, Beta, Active, Deprecated, Retired
```

### Consumer Domain (`Models.Consumer`)

#### **ConsumerAccount**
Represents a customer account.

**Key Properties:**
- `ConsumerId` - Unique identifier
- `CompanyName` - Organization name
- `PrimaryContact` - Main contact person
- `SecondaryContact` - Backup contact
- `BusinessAddress` - Primary address
- `Status` - Account status
- `AccountManagerId` - Assigned account manager

**Status Management:**
```csharp
Active, Inactive, Suspended, PendingApproval, Terminated
```

#### **ContactPerson**
Contact information for account representatives.

**Key Properties:**
- `FullName` - Complete name
- `EmailAddress` - Primary email
- `PhoneNumber` - Contact number
- `JobTitle` - Professional title

#### **Address**
Physical address information.

**Properties:**
- Complete address fields including country, postal code
- Address type classification

### Common Domain (`Models.Common`)

#### **Version**
Custom version management with semantic versioning support.

**Features:**
- `Major.Minor.Patch` format
- Pre-release version support
- Version comparison operators
- Parsing and validation

#### **ValidationResult**
Standardized validation response across all services.

**Properties:**
- `IsValid` - Boolean result
- `Errors` - Collection of error messages
- `Warnings` - Non-critical issues
- `Metadata` - Additional context

## Report Domain (`Models.Report`)

### Report Models

#### **LicenseUsageReport**
Comprehensive license usage analytics.

**Metrics:**
- Total active/inactive licenses
- New licenses issued
- Licenses expired/revoked
- Usage breakdown by product/consumer/status

#### **LicenseExpirationReport**
License expiration tracking and forecasting.

**Features:**
- Configurable days-ahead analysis
- Expiring license details
- Breakdown by days until expiry
- Product-wise expiration analysis

#### **RevenueReport**
Financial reporting for license sales.

**Analytics:**
- Total revenue calculations
- Revenue by time period
- Product and tier breakdowns
- Configurable grouping (Day, Week, Month, Quarter, Year)

#### **ProductPerformanceReport**
Product adoption and performance metrics.

**Metrics:**
- License counts by product
- Revenue per product
- Customer satisfaction scores
- Renewal rates

#### **DashboardAnalytics**
Executive dashboard summary data.

**Key Metrics:**
- Total licenses, consumers, products
- Revenue trends
- Top-performing products
- Expiration alerts

### Enums and Supporting Types

#### **ReportGroupBy**
```csharp
Day, Week, Month, Quarter, Year
```

#### **ExportFormat**
```csharp
PDF, Excel, CSV, JSON
```

#### **DateRange**
```csharp
Today, Yesterday, Last7Days, Last30Days, LastQuarter, LastYear, Custom
```

## Notification Domain (`Models.Notification`)

### Core Classes

#### **NotificationTemplate**
Configurable notification templates.

**Properties:**
- Template identification and naming
- Subject and message templates
- Variable substitution support
- Activation status

#### **NotificationPreferences**
User-specific notification settings.

**Features:**
- Email, SMS, webhook preferences
- Notification type filtering
- Contact information management

#### **NotificationHistory**
Delivery tracking and audit trail.

**Tracking:**
- Delivery status and errors
- Recipient information
- Metadata and context

## Model Relationships

### Primary Relationships

```
EnterpriseProduct (1) ←→ (N) ProductTier
ProductTier (1) ←→ (N) ProductFeature
EnterpriseProduct (1) ←→ (N) ProductLicense
ConsumerAccount (1) ←→ (N) ProductLicense
ProductTier (1) ←→ (N) ProductLicense
```

### Cross-Domain Relationships

```
ConsumerAccount ←→ NotificationPreferences
ProductLicense ←→ AuditEntry
EnterpriseProduct ←→ ReportingModels
```

## Validation Patterns

### Built-in Validations

1. **Email Format Validation** - All email properties
2. **Required Field Validation** - Critical business properties
3. **Length Constraints** - String properties with business limits
4. **Date Range Validation** - Logical date relationships
5. **Enum Value Validation** - Type-safe enumeration usage

### Custom Validation Support

- `ValidationResult` pattern for complex business rules
- Service-level validation through dedicated methods
- Extensible metadata for additional validation context

## Usage Guidelines

### Best Practices

1. **Immutability** - Treat models as immutable after creation
2. **Validation** - Always validate through service interfaces
3. **Relationships** - Use proper foreign key relationships
4. **Status Management** - Follow defined status transition rules
5. **Audit Trails** - Maintain audit information for critical entities

### Anti-Patterns to Avoid

1. Direct model manipulation without service layer
2. Bypassing validation mechanisms
3. Hard-coding status transitions
4. Ignoring audit trail requirements
5. Incomplete relationship management

---

**Last Updated**: July 25, 2025  
**Version**: 1.0.0
