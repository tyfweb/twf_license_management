# Database Schema Scripts

This directory contains CREATE TABLE scripts for the TechWayFit Licensing Management System database schema, supporting multiple database providers.

## Overview

The database schema is designed to support a comprehensive licensing management system with multi-tenant architecture, featuring:

- **Multi-tenancy**: All entities include tenant isolation
- **Audit tracking**: Complete audit trail for all entities
- **Workflow management**: Approval workflows for key entities
- **User management**: Role-based access control system
- **Product management**: Hierarchical product/version/tier structure
- **License management**: Flexible licensing with activation tracking
- **Notification system**: Template-based notification management

## Database Providers

### PostgreSQL (`PostgreSQL/`)
- **Naming Convention**: snake_case for all table and column names
- **Data Types**: PostgreSQL-specific types (TIMESTAMPTZ, TEXT, BYTEA)
- **Features**: Uses UUID extension, JSONB support for metadata
- **Configuration**: Designed for use with EF Core's `UseSnakeCaseNamingConvention()`

### SQL Server (`SqlServer/`)
- **Naming Convention**: PascalCase for all table and column names
- **Data Types**: SQL Server-specific types (DATETIMEOFFSET, NVARCHAR, ROWVERSION)
- **Features**: Uses UNIQUEIDENTIFIER, extended properties for documentation
- **Configuration**: Standard SQL Server naming with Entity Framework

## Schema Structure

### Core Entities

#### Tenants
- Root entity for multi-tenant isolation
- Referenced by all other entities

#### Products Hierarchy
- **Products**: Main product definitions with workflow support
- **ProductVersions**: Version-specific information and release management
- **ProductTiers**: Pricing tiers with feature limitations
- **ProductFeatures**: Individual feature definitions and configurations

#### Consumer Management
- **ConsumerAccounts**: Customer account information with billing details
- **ProductConsumers**: Product subscriptions and tier assignments

#### Licensing
- **ProductLicenses**: License keys, activation tracking, and restrictions

#### User Management
- **UserProfiles**: System user accounts with authentication
- **UserRoles**: Role definitions with permissions
- **UserRoleMappings**: User-to-role assignments

#### System Features
- **NotificationTemplates**: Template-based notification system
- **NotificationHistory**: Notification delivery tracking
- **WorkflowHistory**: Audit trail for workflow state changes
- **Settings**: System and tenant configuration
- **AuditEntries**: Comprehensive audit logging

### Key Features

#### Audit Trail
All entities inherit from `BaseEntity` which includes:
- `IsActive`: Soft delete support
- `IsDeleted`: Logical deletion flag
- `CreatedBy/CreatedOn`: Creation tracking
- `UpdatedBy/UpdatedOn`: Modification tracking
- `DeletedBy/DeletedOn`: Deletion tracking
- `RowVersion`: Concurrency control

#### Multi-Tenancy
- Every entity includes `TenantId` for data isolation
- Foreign key relationships respect tenant boundaries
- Supports both single-tenant and multi-tenant deployments

#### Workflow Support
Key entities include workflow fields:
- `ApprovalStatus`: Current approval state
- `ApprovalRequired`: Whether approval is needed
- `ApprovedBy/ApprovedOn`: Approval tracking
- `RejectedBy/RejectedOn`: Rejection tracking

## Performance Optimizations

### Indexes
- Primary keys on all entities (UUID/UNIQUEIDENTIFIER)
- Foreign key indexes for relationship navigation
- Query-specific indexes for common search patterns
- Composite indexes for multi-column queries

### Constraints
- Unique constraints for business keys
- Foreign key constraints for referential integrity
- Check constraints for data validation (where applicable)

## Deployment Notes

### PostgreSQL
1. Ensure the `uuid-ossp` extension is available
2. Configure connection with appropriate privileges
3. Set up proper schemas if using schema-based tenancy

### SQL Server
1. Ensure proper collation settings for international support
2. Configure appropriate database recovery model
3. Consider partitioning for large audit tables

## Maintenance Scripts

Additional scripts are provided for:
- Initial data seeding
- Index maintenance
- Statistics updates
- Backup and restore procedures

## Migration Strategy

When upgrading the schema:
1. Always backup existing data
2. Test migration scripts in development environment
3. Consider downtime requirements for production deployment
4. Use appropriate migration tools (EF Migrations, Flyway, etc.)

## Naming Convention Summary

| Database | Tables | Columns | Indexes | Constraints |
|----------|--------|---------|---------|-------------|
| PostgreSQL | snake_case | snake_case | snake_case | snake_case |
| SQL Server | PascalCase | PascalCase | PascalCase | PascalCase |

## Entity Relationships

```
Tenants (1) ──→ (∞) Products
Products (1) ──→ (∞) ProductVersions
Products (1) ──→ (∞) ProductTiers
Products (1) ──→ (∞) ProductFeatures
Products (1) ──→ (∞) ProductLicenses
ConsumerAccounts (1) ──→ (∞) ProductConsumers
Products (1) ──→ (∞) ProductConsumers
UserProfiles (1) ──→ (∞) UserRoleMappings
UserRoles (1) ──→ (∞) UserRoleMappings
```

This schema supports the complete lifecycle of licensing management from product definition through customer onboarding to license activation and ongoing management.
