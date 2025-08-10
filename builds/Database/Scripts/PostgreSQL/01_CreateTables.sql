-- ================================================================================================
-- PostgreSQL CREATE TABLE Script for TechWayFit Licensing Management System
-- Created: Auto-generated from Entity Framework Models
-- Provider: PostgreSQL with snake_case naming convention
-- ================================================================================================

-- Enable UUID extension for PostgreSQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create tenant table first (referenced by other tables)
CREATE TABLE IF NOT EXISTS tenants (
    tenant_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA
);

-- Create products table
CREATE TABLE IF NOT EXISTS products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    release_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    support_email VARCHAR(255) NOT NULL DEFAULT '',
    support_phone VARCHAR(50) NOT NULL DEFAULT '',
    decommission_date TIMESTAMPTZ,
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    metadata_json TEXT NOT NULL DEFAULT '{}',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    -- Workflow fields
    approval_status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    approval_required BOOLEAN NOT NULL DEFAULT false,
    approval_comments TEXT,
    approved_by VARCHAR(100),
    approved_on TIMESTAMPTZ,
    rejected_by VARCHAR(100),
    rejected_on TIMESTAMPTZ,
    submission_date TIMESTAMPTZ,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create product_versions table
CREATE TABLE IF NOT EXISTS product_versions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    product_id UUID NOT NULL,
    version_number VARCHAR(50) NOT NULL,
    version_name VARCHAR(255) NOT NULL DEFAULT '',
    release_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    end_of_life_date TIMESTAMPTZ,
    is_beta BOOLEAN NOT NULL DEFAULT false,
    changelog TEXT NOT NULL DEFAULT '',
    download_url VARCHAR(500) NOT NULL DEFAULT '',
    minimum_requirements TEXT NOT NULL DEFAULT '',
    installation_guide TEXT NOT NULL DEFAULT '',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- Create product_tiers table
CREATE TABLE IF NOT EXISTS product_tiers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    product_id UUID NOT NULL,
    tier_name VARCHAR(100) NOT NULL,
    tier_description TEXT NOT NULL DEFAULT '',
    price DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    currency_code VARCHAR(3) NOT NULL DEFAULT 'USD',
    billing_cycle VARCHAR(50) NOT NULL DEFAULT 'Monthly',
    max_users INTEGER NOT NULL DEFAULT 1,
    max_devices INTEGER NOT NULL DEFAULT 1,
    storage_limit_gb INTEGER NOT NULL DEFAULT 0,
    api_rate_limit INTEGER NOT NULL DEFAULT 1000,
    features_json TEXT NOT NULL DEFAULT '{}',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- Create product_features table
CREATE TABLE IF NOT EXISTS product_features (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    product_id UUID NOT NULL,
    feature_name VARCHAR(100) NOT NULL,
    feature_code VARCHAR(50) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    category VARCHAR(100) NOT NULL DEFAULT '',
    is_premium BOOLEAN NOT NULL DEFAULT false,
    min_tier_required VARCHAR(50) NOT NULL DEFAULT 'Basic',
    configuration_json TEXT NOT NULL DEFAULT '{}',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- Create consumer_accounts table
CREATE TABLE IF NOT EXISTS consumer_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    company_name VARCHAR(255) NOT NULL DEFAULT '',
    account_code VARCHAR(50),
    primary_contact_name VARCHAR(255) NOT NULL DEFAULT '',
    primary_contact_email VARCHAR(255) NOT NULL DEFAULT '',
    primary_contact_phone VARCHAR(50) NOT NULL DEFAULT '',
    primary_contact_position VARCHAR(100) NOT NULL DEFAULT '',
    secondary_contact_name VARCHAR(255),
    secondary_contact_email VARCHAR(255),
    secondary_contact_phone VARCHAR(50),
    secondary_contact_position VARCHAR(100),
    activated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    subscription_end TIMESTAMPTZ,
    address_street VARCHAR(255) NOT NULL DEFAULT '',
    address_city VARCHAR(100) NOT NULL DEFAULT '',
    address_state VARCHAR(100) NOT NULL DEFAULT '',
    address_postal_code VARCHAR(20) NOT NULL DEFAULT '',
    address_country VARCHAR(100) NOT NULL DEFAULT '',
    notes TEXT NOT NULL DEFAULT '',
    billing_contact_name VARCHAR(255) NOT NULL DEFAULT '',
    billing_contact_email VARCHAR(255) NOT NULL DEFAULT '',
    billing_contact_phone VARCHAR(50) NOT NULL DEFAULT '',
    account_manager VARCHAR(255) NOT NULL DEFAULT '',
    industry VARCHAR(100) NOT NULL DEFAULT '',
    company_size VARCHAR(50) NOT NULL DEFAULT '',
    website VARCHAR(500) NOT NULL DEFAULT '',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create product_consumers table (relationship between products and consumer accounts)
CREATE TABLE IF NOT EXISTS product_consumers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    product_id UUID NOT NULL,
    consumer_account_id UUID NOT NULL,
    subscription_start TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    subscription_end TIMESTAMPTZ,
    tier_id UUID,
    is_trial BOOLEAN NOT NULL DEFAULT false,
    trial_end_date TIMESTAMPTZ,
    custom_features_json TEXT NOT NULL DEFAULT '{}',
    custom_limits_json TEXT NOT NULL DEFAULT '{}',
    subscription_status VARCHAR(50) NOT NULL DEFAULT 'Active',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (consumer_account_id) REFERENCES consumer_accounts(id) ON DELETE CASCADE,
    FOREIGN KEY (tier_id) REFERENCES product_tiers(id)
);

-- Create product_licenses table
CREATE TABLE IF NOT EXISTS product_licenses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    product_id UUID NOT NULL,
    consumer_account_id UUID NOT NULL,
    license_key VARCHAR(500) NOT NULL,
    license_type VARCHAR(50) NOT NULL DEFAULT 'Standard',
    expires_at TIMESTAMPTZ,
    max_activations INTEGER NOT NULL DEFAULT 1,
    current_activations INTEGER NOT NULL DEFAULT 0,
    features_json TEXT NOT NULL DEFAULT '{}',
    restrictions_json TEXT NOT NULL DEFAULT '{}',
    hardware_fingerprint VARCHAR(500) NOT NULL DEFAULT '',
    activation_data_json TEXT NOT NULL DEFAULT '{}',
    is_revoked BOOLEAN NOT NULL DEFAULT false,
    revoked_at TIMESTAMPTZ,
    revoked_reason VARCHAR(500) NOT NULL DEFAULT '',
    last_heartbeat TIMESTAMPTZ,
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (consumer_account_id) REFERENCES consumer_accounts(id) ON DELETE CASCADE
);

-- Create user_roles table
CREATE TABLE IF NOT EXISTS user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    role_name VARCHAR(100) NOT NULL,
    role_description TEXT NOT NULL DEFAULT '',
    permissions_json TEXT NOT NULL DEFAULT '{}',
    is_system_role BOOLEAN NOT NULL DEFAULT false,
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create user_profiles table
CREATE TABLE IF NOT EXISTS user_profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    user_name VARCHAR(255) NOT NULL,
    password_hash VARCHAR(500) NOT NULL DEFAULT '',
    password_salt VARCHAR(500) NOT NULL DEFAULT '',
    full_name VARCHAR(255) NOT NULL DEFAULT '',
    email VARCHAR(255) NOT NULL DEFAULT '',
    department VARCHAR(100),
    is_locked BOOLEAN NOT NULL DEFAULT false,
    is_admin BOOLEAN NOT NULL DEFAULT false,
    last_login_date TIMESTAMPTZ,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    locked_date TIMESTAMPTZ,
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create user_role_mappings table
CREATE TABLE IF NOT EXISTS user_role_mappings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    assigned_by VARCHAR(100) NOT NULL,
    expires_at TIMESTAMPTZ,
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (user_id) REFERENCES user_profiles(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES user_roles(id) ON DELETE CASCADE
);

-- Create notification_templates table
CREATE TABLE IF NOT EXISTS notification_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    template_name VARCHAR(100) NOT NULL,
    template_type VARCHAR(50) NOT NULL DEFAULT 'Email',
    subject_template TEXT NOT NULL DEFAULT '',
    body_template TEXT NOT NULL DEFAULT '',
    variables_json TEXT NOT NULL DEFAULT '{}',
    is_system_template BOOLEAN NOT NULL DEFAULT false,
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create notification_history table
CREATE TABLE IF NOT EXISTS notification_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    template_id UUID,
    recipient_email VARCHAR(255) NOT NULL DEFAULT '',
    recipient_name VARCHAR(255) NOT NULL DEFAULT '',
    subject VARCHAR(500) NOT NULL DEFAULT '',
    body TEXT NOT NULL DEFAULT '',
    notification_type VARCHAR(50) NOT NULL DEFAULT 'Email',
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    sent_at TIMESTAMPTZ,
    failed_at TIMESTAMPTZ,
    retry_count INTEGER NOT NULL DEFAULT 0,
    error_message TEXT NOT NULL DEFAULT '',
    metadata_json TEXT NOT NULL DEFAULT '{}',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id),
    FOREIGN KEY (template_id) REFERENCES notification_templates(id)
);

-- Create workflow_history table
CREATE TABLE IF NOT EXISTS workflow_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    entity_id UUID NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    workflow_step VARCHAR(100) NOT NULL,
    previous_status VARCHAR(50) NOT NULL DEFAULT '',
    new_status VARCHAR(50) NOT NULL DEFAULT '',
    action_taken VARCHAR(100) NOT NULL DEFAULT '',
    actor_id UUID,
    actor_name VARCHAR(255) NOT NULL DEFAULT '',
    comments TEXT NOT NULL DEFAULT '',
    metadata_json TEXT NOT NULL DEFAULT '{}',
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create settings table
CREATE TABLE IF NOT EXISTS settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    setting_key VARCHAR(255) NOT NULL,
    setting_value TEXT NOT NULL DEFAULT '',
    setting_type VARCHAR(50) NOT NULL DEFAULT 'String',
    category VARCHAR(100) NOT NULL DEFAULT 'General',
    description TEXT NOT NULL DEFAULT '',
    is_sensitive BOOLEAN NOT NULL DEFAULT false,
    is_system_setting BOOLEAN NOT NULL DEFAULT false,
    validation_rules_json TEXT NOT NULL DEFAULT '{}',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create audit_entries table
CREATE TABLE IF NOT EXISTS audit_entries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    entity_id UUID NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    operation VARCHAR(50) NOT NULL,
    user_id VARCHAR(100) NOT NULL,
    user_name VARCHAR(255) NOT NULL DEFAULT '',
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    old_values_json TEXT NOT NULL DEFAULT '{}',
    new_values_json TEXT NOT NULL DEFAULT '{}',
    changes_summary_json TEXT NOT NULL DEFAULT '{}',
    ip_address VARCHAR(45) NOT NULL DEFAULT '',
    user_agent VARCHAR(500) NOT NULL DEFAULT '',
    is_read_only BOOLEAN NOT NULL DEFAULT false,
    can_delete BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMPTZ,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMPTZ,
    row_version BYTEA,
    FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_products_tenant_id ON products(tenant_id);
CREATE INDEX IF NOT EXISTS idx_products_status ON products(status);
CREATE INDEX IF NOT EXISTS idx_product_versions_product_id ON product_versions(product_id);
CREATE INDEX IF NOT EXISTS idx_product_tiers_product_id ON product_tiers(product_id);
CREATE INDEX IF NOT EXISTS idx_product_features_product_id ON product_features(product_id);
CREATE INDEX IF NOT EXISTS idx_consumer_accounts_tenant_id ON consumer_accounts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_product_consumers_product_id ON product_consumers(product_id);
CREATE INDEX IF NOT EXISTS idx_product_consumers_consumer_account_id ON product_consumers(consumer_account_id);
CREATE INDEX IF NOT EXISTS idx_product_licenses_product_id ON product_licenses(product_id);
CREATE INDEX IF NOT EXISTS idx_product_licenses_consumer_account_id ON product_licenses(consumer_account_id);
CREATE INDEX IF NOT EXISTS idx_product_licenses_license_key ON product_licenses(license_key);
CREATE INDEX IF NOT EXISTS idx_user_profiles_tenant_id ON user_profiles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_user_profiles_user_name ON user_profiles(user_name);
CREATE INDEX IF NOT EXISTS idx_user_profiles_email ON user_profiles(email);
CREATE INDEX IF NOT EXISTS idx_user_role_mappings_user_id ON user_role_mappings(user_id);
CREATE INDEX IF NOT EXISTS idx_user_role_mappings_role_id ON user_role_mappings(role_id);
CREATE INDEX IF NOT EXISTS idx_notification_history_recipient_email ON notification_history(recipient_email);
CREATE INDEX IF NOT EXISTS idx_workflow_history_entity_id ON workflow_history(entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_entries_entity_id ON audit_entries(entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_entries_timestamp ON audit_entries(timestamp);

-- Add unique constraints
ALTER TABLE tenants ADD CONSTRAINT uk_tenants_tenant_name UNIQUE (tenant_name);
ALTER TABLE products ADD CONSTRAINT uk_products_name_tenant UNIQUE (name, tenant_id);
ALTER TABLE product_versions ADD CONSTRAINT uk_product_versions_number_product UNIQUE (version_number, product_id);
ALTER TABLE product_tiers ADD CONSTRAINT uk_product_tiers_name_product UNIQUE (tier_name, product_id);
ALTER TABLE product_features ADD CONSTRAINT uk_product_features_code_product UNIQUE (feature_code, product_id);
ALTER TABLE consumer_accounts ADD CONSTRAINT uk_consumer_accounts_account_code UNIQUE (account_code) WHERE account_code IS NOT NULL;
ALTER TABLE user_profiles ADD CONSTRAINT uk_user_profiles_user_name_tenant UNIQUE (user_name, tenant_id);
ALTER TABLE user_profiles ADD CONSTRAINT uk_user_profiles_email_tenant UNIQUE (email, tenant_id);
ALTER TABLE user_roles ADD CONSTRAINT uk_user_roles_name_tenant UNIQUE (role_name, tenant_id);
ALTER TABLE user_role_mappings ADD CONSTRAINT uk_user_role_mappings_user_role UNIQUE (user_id, role_id);
ALTER TABLE notification_templates ADD CONSTRAINT uk_notification_templates_name_tenant UNIQUE (template_name, tenant_id);
ALTER TABLE settings ADD CONSTRAINT uk_settings_key_tenant UNIQUE (setting_key, tenant_id);

-- Add comments to tables for documentation
COMMENT ON TABLE tenants IS 'Tenant information for multi-tenancy support';
COMMENT ON TABLE products IS 'Enterprise products available for licensing';
COMMENT ON TABLE product_versions IS 'Version information for products';
COMMENT ON TABLE product_tiers IS 'Pricing tiers and feature sets for products';
COMMENT ON TABLE product_features IS 'Individual features available in products';
COMMENT ON TABLE consumer_accounts IS 'Customer accounts that consume licensed products';
COMMENT ON TABLE product_consumers IS 'Relationship between products and consumer accounts';
COMMENT ON TABLE product_licenses IS 'License keys and activation data for products';
COMMENT ON TABLE user_profiles IS 'User accounts for system access';
COMMENT ON TABLE user_roles IS 'Role definitions for access control';
COMMENT ON TABLE user_role_mappings IS 'Assignment of roles to users';
COMMENT ON TABLE notification_templates IS 'Templates for system notifications';
COMMENT ON TABLE notification_history IS 'History of sent notifications';
COMMENT ON TABLE workflow_history IS 'Audit trail for workflow state changes';
COMMENT ON TABLE settings IS 'System and tenant-specific configuration settings';
COMMENT ON TABLE audit_entries IS 'Comprehensive audit log for all entity changes';
