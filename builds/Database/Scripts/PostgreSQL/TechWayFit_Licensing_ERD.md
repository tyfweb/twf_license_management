# TechWayFit Licensing Management System - Entity Relationship Diagram

## Database Schema Overview
This ERD represents the complete database structure for the TechWayFit Licensing Management System with multi-tenant architecture.

```mermaid
erDiagram
    TENANTS {
        uuid tenant_id PK "Primary Key"
        varchar tenant_name "Unique tenant name"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    PRODUCTS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar name "Product name"
        text description "Product description"
        timestamptz release_date "Release date"
        varchar support_email "Support contact email"
        varchar support_phone "Support contact phone"
        timestamptz decommission_date "End of life date"
        varchar status "Product status"
        text metadata_json "Additional metadata"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
        varchar approval_status "Workflow approval status"
        boolean approval_required "Requires approval flag"
        text approval_comments "Approval comments"
        varchar approved_by "Approval actor"
        timestamptz approved_on "Approval timestamp"
        varchar rejected_by "Rejection actor"
        timestamptz rejected_on "Rejection timestamp"
        timestamptz submission_date "Submission timestamp"
    }

    PRODUCT_VERSIONS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid product_id FK "Product reference"
        varchar version_number "Version identifier"
        varchar version_name "Version display name"
        timestamptz release_date "Version release date"
        timestamptz end_of_life_date "Version EOL date"
        boolean is_beta "Beta version flag"
        text changelog "Version changes"
        varchar download_url "Download location"
        text minimum_requirements "System requirements"
        text installation_guide "Installation instructions"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    PRODUCT_TIERS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid product_id FK "Product reference"
        varchar tier_name "Tier name"
        text tier_description "Tier description"
        decimal price "Tier price"
        varchar currency_code "Currency code"
        varchar billing_cycle "Billing frequency"
        integer max_users "Maximum users"
        integer max_devices "Maximum devices"
        integer storage_limit_gb "Storage limit in GB"
        integer api_rate_limit "API rate limit"
        text features_json "Tier features JSON"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    PRODUCT_FEATURES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid product_id FK "Product reference"
        varchar feature_name "Feature name"
        varchar feature_code "Unique feature code"
        text description "Feature description"
        varchar category "Feature category"
        boolean is_premium "Premium feature flag"
        varchar min_tier_required "Minimum required tier"
        text configuration_json "Feature configuration"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    CONSUMER_ACCOUNTS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar company_name "Company name"
        varchar account_code "Unique account code"
        varchar primary_contact_name "Primary contact name"
        varchar primary_contact_email "Primary contact email"
        varchar primary_contact_phone "Primary contact phone"
        varchar primary_contact_position "Primary contact position"
        varchar secondary_contact_name "Secondary contact name"
        varchar secondary_contact_email "Secondary contact email"
        varchar secondary_contact_phone "Secondary contact phone"
        varchar secondary_contact_position "Secondary contact position"
        timestamptz activated_at "Account activation date"
        timestamptz subscription_end "Subscription end date"
        varchar address_street "Street address"
        varchar address_city "City"
        varchar address_state "State/Province"
        varchar address_postal_code "Postal code"
        varchar address_country "Country"
        text notes "Account notes"
        varchar billing_contact_name "Billing contact name"
        varchar billing_contact_email "Billing contact email"
        varchar billing_contact_phone "Billing contact phone"
        varchar account_manager "Account manager"
        varchar industry "Company industry"
        varchar company_size "Company size"
        varchar website "Company website"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    PRODUCT_CONSUMERS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid product_id FK "Product reference"
        uuid consumer_account_id FK "Consumer account reference"
        timestamptz subscription_start "Subscription start date"
        timestamptz subscription_end "Subscription end date"
        uuid tier_id FK "Product tier reference"
        boolean is_trial "Trial subscription flag"
        timestamptz trial_end_date "Trial end date"
        text custom_features_json "Custom features"
        text custom_limits_json "Custom limits"
        varchar subscription_status "Subscription status"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    PRODUCT_LICENSES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid product_id FK "Product reference"
        uuid consumer_account_id FK "Consumer account reference"
        varchar license_key "License key"
        varchar license_type "License type"
        timestamptz expires_at "Expiration date"
        integer max_activations "Maximum activations"
        integer current_activations "Current activations"
        text features_json "Licensed features"
        text restrictions_json "License restrictions"
        varchar hardware_fingerprint "Hardware fingerprint"
        text activation_data_json "Activation data"
        boolean is_revoked "Revoked status"
        timestamptz revoked_at "Revocation date"
        varchar revoked_reason "Revocation reason"
        timestamptz last_heartbeat "Last heartbeat"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    USER_ROLES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar role_name "Role name"
        text role_description "Role description"
        text permissions_json "Role permissions"
        boolean is_system_role "System role flag"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    USER_PROFILES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar user_name "Username"
        varchar password_hash "Password hash"
        varchar password_salt "Password salt"
        varchar full_name "Full name"
        varchar email "Email address"
        varchar department "Department"
        boolean is_locked "Account locked flag"
        boolean is_admin "Admin flag"
        timestamptz last_login_date "Last login date"
        integer failed_login_attempts "Failed login attempts"
        timestamptz locked_date "Account lock date"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    USER_ROLE_MAPPINGS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid user_id FK "User reference"
        uuid role_id FK "Role reference"
        timestamptz assigned_at "Assignment date"
        varchar assigned_by "Assignment actor"
        timestamptz expires_at "Assignment expiration"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    NOTIFICATION_TEMPLATES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar template_name "Template name"
        varchar template_type "Template type"
        text subject_template "Subject template"
        text body_template "Body template"
        text variables_json "Template variables"
        boolean is_system_template "System template flag"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    NOTIFICATION_HISTORY {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid template_id FK "Template reference"
        varchar recipient_email "Recipient email"
        varchar recipient_name "Recipient name"
        varchar subject "Email subject"
        text body "Email body"
        varchar notification_type "Notification type"
        varchar status "Delivery status"
        timestamptz sent_at "Sent timestamp"
        timestamptz failed_at "Failed timestamp"
        integer retry_count "Retry attempts"
        text error_message "Error message"
        text metadata_json "Additional metadata"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    WORKFLOW_HISTORY {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid entity_id "Entity reference"
        varchar entity_type "Entity type"
        varchar workflow_step "Workflow step"
        varchar previous_status "Previous status"
        varchar new_status "New status"
        varchar action_taken "Action taken"
        uuid actor_id "Actor ID"
        varchar actor_name "Actor name"
        text comments "Comments"
        text metadata_json "Additional metadata"
        timestamptz occurred_at "Occurrence timestamp"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    SETTINGS {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        varchar setting_key "Setting key"
        text setting_value "Setting value"
        varchar setting_type "Value type"
        varchar category "Setting category"
        text description "Setting description"
        boolean is_sensitive "Sensitive data flag"
        boolean is_system_setting "System setting flag"
        text validation_rules_json "Validation rules"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    AUDIT_ENTRIES {
        uuid id PK "Primary Key"
        uuid tenant_id FK "Tenant reference"
        uuid entity_id "Entity reference"
        varchar entity_type "Entity type"
        varchar operation "Operation type"
        varchar user_id "User ID"
        varchar user_name "User name"
        timestamptz timestamp "Operation timestamp"
        text old_values_json "Previous values"
        text new_values_json "New values"
        text changes_summary_json "Change summary"
        varchar ip_address "IP address"
        varchar user_agent "User agent"
        boolean is_read_only "Read-only flag"
        boolean can_delete "Deletable flag"
        boolean is_active "Active status"
        boolean is_deleted "Soft delete flag"
        varchar created_by "Creator identifier"
        timestamptz created_on "Creation timestamp"
        varchar updated_by "Last updater"
        timestamptz updated_on "Last update timestamp"
        varchar deleted_by "Deletion actor"
        timestamptz deleted_on "Deletion timestamp"
        bytea row_version "Concurrency control"
    }

    %% Relationships
    TENANTS ||--o{ PRODUCTS : "hosts"
    TENANTS ||--o{ CONSUMER_ACCOUNTS : "manages"
    TENANTS ||--o{ USER_PROFILES : "contains"
    TENANTS ||--o{ USER_ROLES : "defines"
    TENANTS ||--o{ NOTIFICATION_TEMPLATES : "owns"
    TENANTS ||--o{ SETTINGS : "configures"

    PRODUCTS ||--o{ PRODUCT_VERSIONS : "has_versions"
    PRODUCTS ||--o{ PRODUCT_TIERS : "offers_tiers"
    PRODUCTS ||--o{ PRODUCT_FEATURES : "includes_features"
    PRODUCTS ||--o{ PRODUCT_CONSUMERS : "subscribed_by"
    PRODUCTS ||--o{ PRODUCT_LICENSES : "licensed_under"

    PRODUCT_TIERS ||--o{ PRODUCT_CONSUMERS : "tier_assignment"

    CONSUMER_ACCOUNTS ||--o{ PRODUCT_CONSUMERS : "subscribes_to"
    CONSUMER_ACCOUNTS ||--o{ PRODUCT_LICENSES : "holds_licenses"

    USER_PROFILES ||--o{ USER_ROLE_MAPPINGS : "assigned_roles"
    USER_ROLES ||--o{ USER_ROLE_MAPPINGS : "role_assignments"

    NOTIFICATION_TEMPLATES ||--o{ NOTIFICATION_HISTORY : "generates"

    TENANTS ||--o{ WORKFLOW_HISTORY : "tracks_workflows"
    TENANTS ||--o{ AUDIT_ENTRIES : "audits_changes"
```

## Key Relationships

### Core Product Management
- **Tenants** serve as the root entity for multi-tenancy
- **Products** belong to tenants and have multiple versions, tiers, and features
- **Product Versions** track different releases of products
- **Product Tiers** define pricing and feature sets
- **Product Features** define individual capabilities

### Customer Management
- **Consumer Accounts** represent customer organizations
- **Product Consumers** link accounts to products with specific tiers
- **Product Licenses** provide activation keys and usage tracking

### User & Security Management
- **User Profiles** store user authentication and profile data
- **User Roles** define permission sets
- **User Role Mappings** assign roles to users

### System Operations
- **Notification Templates** and **Notification History** manage communications
- **Workflow History** tracks approval processes and state changes
- **Settings** store configuration data
- **Audit Entries** provide comprehensive change tracking

## Database Features

### Multi-Tenancy
- All entities include `tenant_id` for data isolation
- Tenant-specific constraints ensure data segregation

### Soft Delete Pattern
- All entities support soft deletion with `is_deleted` and `deleted_on` fields
- Preserves data integrity while allowing logical removal

### Audit Trail
- Comprehensive audit logging with before/after values
- User tracking and timestamp information
- IP address and user agent capture

### Workflow Support
- Approval workflow fields in products
- Workflow history tracking for state changes
- Flexible metadata storage in JSON fields

### Performance Optimization
- Strategic indexes on foreign keys and frequently queried fields
- Unique constraints to prevent data duplication
- Efficient lookup patterns for common operations
