# TechWayFit Licensing Management System - PostgreSQL Schema v1.0.0

## Overview
This directory contains the initial PostgreSQL database schema for the TechWayFit Licensing Management System. The schema is designed to support a comprehensive license management platform with user management, operations monitoring, and audit capabilities.

## Files

### v1.0.0-initial-schema.sql
The main PostgreSQL schema creation script that includes:

## Database Entities

### Core Product Management
- **products** - Product definitions and metadata (Primary Key: `id` UUID)
- **product_versions** - Version tracking with release information (Primary Key: `id` UUID)
- **product_tiers** - Pricing tiers and service levels (Primary Key: `id` UUID)
- **product_features** - Individual product features and capabilities (Primary Key: `id` UUID)

### Consumer Management
- **consumer_accounts** - Customer accounts and organization information (Primary Key: `id` UUID)
- **product_consumers** - Many-to-many relationship between products and consumers (Primary Key: `id` UUID)

### License Management
- **product_licenses** - License instances with cryptographic keys and validity periods (Primary Key: `id` UUID)
- **product_license_features** - Junction table for license-feature relationships (Composite Primary Key: `license_id`, `feature_id`)

### User Management & Security
- **user_profiles** - User account information and authentication data (Primary Key: `id` UUID)
- **user_roles** - Role definitions for authorization (Primary Key: `id` UUID)
- **user_role_mappings** - User-role assignments with expiry support (Primary Key: `id` UUID)

### System Configuration
- **settings** - System configuration settings and preferences with validation rules (Primary Key: `id` UUID)

### Operations & Monitoring
- **system_metrics** - Aggregated system performance metrics (Primary Key: `id` UUID)
- **system_health_snapshots** - Point-in-time system health snapshots (Primary Key: `id` UUID)
- **page_performance_metrics** - Web page performance tracking (Primary Key: `id` UUID)
- **query_performance_metrics** - Database query performance metrics (Primary Key: `id` UUID)
- **error_log_summaries** - Aggregated error logs and exception tracking (Primary Key: `id` UUID)

### Audit & Notifications
- **audit_entries** - Comprehensive audit trail for all entity changes (Primary Key: `id` UUID)
- **notification_templates** - Templates for system notifications (Primary Key: `id` UUID)
- **notification_history** - History of sent notifications and delivery status (Primary Key: `id` UUID)

## Features

### Performance Optimization
- Comprehensive indexing strategy for all major query patterns
- Composite indexes for complex filtering scenarios
- Unique constraints for data integrity

### Data Integrity
- Foreign key relationships with appropriate cascade rules
- Unique constraints to prevent data duplication
- Check constraints for data validation

### Soft Delete Support
- All entities include `is_active` flag for soft delete functionality
- Audit trail preservation

### PostgreSQL-Specific Features
- UUID extension enabled for better ID generation
- Timezone-aware timestamps
- JSON columns for flexible metadata storage
- Views for common query patterns

## Views

### active_licenses_view
Shows currently active licenses with product and consumer information.

### product_features_summary
Provides feature counts by product and tier.

### v_user_details
Combines user profiles with their assigned roles.

### v_system_health_summary
Aggregates system health metrics by environment over the last 24 hours.

## Installation

1. Ensure PostgreSQL 12+ is installed
2. Create a new database for the licensing system
3. Run the schema script:
   ```sql
   \i v1.0.0-initial-schema.sql
   ```

## Key Corrections Made

### Primary Key Standardization
- **Corrected Primary Keys**: All tables now use `id` (UUID) as the primary key column, matching the `BaseAuditEntity.Id` property
- **Entity Framework Alignment**: Schema is now fully aligned with C# entity definitions that inherit from `BaseAuditEntity`
- **Foreign Key Updates**: All foreign key references updated to use `id` instead of entity-specific names (e.g., `product_id` references `products.id`)

### Complete Audit Trail Support
- **Added Missing Fields**: All tables now include `is_deleted`, `deleted_by`, and `deleted_on` fields from `BaseAuditEntity`
- **Soft Delete Support**: Full support for soft deletes with both `is_active` and `is_deleted` flags
- **Audit Consistency**: All audit fields (created_by, updated_by, created_on, updated_on, deleted_by, deleted_on) are consistently applied

## Compatibility
- PostgreSQL 12+
- Entity Framework Core compatible with proper naming conventions
- **UUID Primary Keys**: All entities use UUID `id` as primary key matching `BaseAuditEntity.Id`
- Supports both UUID and string-based foreign key relationships as per entity definitions

## Notes
- All timestamp fields use `TIMESTAMP WITH TIME ZONE` for proper timezone handling
- JSON fields are used for flexible metadata storage
- Audit fields (created_by, updated_by, created_on, updated_on) are included on all entities
- The schema supports multi-tenant scenarios through proper foreign key relationships

## Next Steps
After running this schema:
1. Create initial user accounts and roles
2. Configure system settings
3. Set up monitoring and alerting based on the operations dashboard tables
4. Implement regular cleanup procedures for old audit entries and metrics
