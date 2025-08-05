-- TechWayFit Licensing Management System - PostgreSQL Database Creation Script
-- Generated from Entity Framework Core entities and DbContext configurations
-- Compatible with PostgreSQL 12+
-- Version: 1.0.0-initial (Corrected Primary Keys)

-- Enable UUID extension for better ID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- CORE PRODUCT ENTITIES
-- =============================================

-- ProductEntity table
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    release_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    support_email VARCHAR(255),
    support_phone VARCHAR(50),
    decommission_date TIMESTAMP WITH TIME ZONE,
    metadata_json VARCHAR(2000) DEFAULT '{}',
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- ProductVersionEntity table
CREATE TABLE product_versions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL,
    version VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    release_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    end_of_life_date TIMESTAMP WITH TIME ZONE,
    support_end_date TIMESTAMP WITH TIME ZONE,
    release_notes VARCHAR(2000),
    is_current BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_versions_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- ProductTierEntity table
CREATE TABLE product_tiers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    price VARCHAR(10),
    display_order INTEGER NOT NULL DEFAULT 0,
    support_sla_json VARCHAR(1000) DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_tiers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- ProductFeatureEntity table
CREATE TABLE product_features (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL,
    tier_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    code VARCHAR(100) NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT true,
    display_order INTEGER DEFAULT 0,
    support_from_version VARCHAR(20) DEFAULT '1.0.0',
    support_to_version VARCHAR(20) DEFAULT '9999.0.0',
    feature_usage_json VARCHAR(1000) DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_features_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT fk_product_features_tier_id 
        FOREIGN KEY (tier_id) REFERENCES product_tiers(id) ON DELETE CASCADE
);

-- =============================================
-- CONSUMER ENTITIES
-- =============================================

-- ConsumerAccountEntity table
CREATE TABLE consumer_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name VARCHAR(200) NOT NULL,
    account_code VARCHAR(50),
    primary_contact_name VARCHAR(200),
    primary_contact_email VARCHAR(255),
    primary_contact_phone VARCHAR(50),
    primary_contact_position VARCHAR(100),
    secondary_contact_name VARCHAR(100),
    secondary_contact_email VARCHAR(255),
    secondary_contact_phone VARCHAR(50),
    secondary_contact_position VARCHAR(100),
    activated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    subscription_end TIMESTAMP WITH TIME ZONE,
    address_street VARCHAR(500),
    address_city VARCHAR(100),
    address_state VARCHAR(100),
    address_postal_code VARCHAR(20),
    address_country VARCHAR(100),
    notes VARCHAR(2000),
    status VARCHAR(20),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- ProductConsumerEntity table (Many-to-many relationship between products and consumers)
CREATE TABLE product_consumers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL,
    consumer_id UUID NOT NULL,
    account_manager_name VARCHAR(100),
    account_manager_email VARCHAR(255),
    account_manager_phone VARCHAR(50),
    account_manager_position VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_consumers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT fk_product_consumers_consumer_id 
        FOREIGN KEY (consumer_id) REFERENCES consumer_accounts(id) ON DELETE CASCADE
);

-- =============================================
-- LICENSE ENTITIES
-- =============================================

-- ProductLicenseEntity table (Updated for Tier-Based Licensing)
CREATE TABLE product_licenses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    license_code VARCHAR(50) NOT NULL,
    product_id UUID NOT NULL,
    consumer_id UUID NOT NULL,
    -- NEW: Product Tier support for tier-based licensing
    product_tier_id UUID NULL,
    valid_from TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_to TIMESTAMP WITH TIME ZONE NOT NULL,
    -- NEW: Version range support
    valid_product_version_from VARCHAR(20) NOT NULL DEFAULT '1.0.0',
    valid_product_version_to VARCHAR(20) NULL,
    encryption VARCHAR(50) NOT NULL DEFAULT 'AES256',
    signature VARCHAR(50) NOT NULL DEFAULT 'SHA256',
    license_key VARCHAR(2000) NOT NULL,
    public_key VARCHAR(2000),
    license_signature VARCHAR(2000),
    key_generated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    issued_by VARCHAR(100) NOT NULL,
    revoked_at TIMESTAMP WITH TIME ZONE,
    revocation_reason VARCHAR(500),
    metadata_json VARCHAR(2000) DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_licenses_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    CONSTRAINT fk_product_licenses_consumer_id 
        FOREIGN KEY (consumer_id) REFERENCES consumer_accounts(id) ON DELETE RESTRICT,
    -- NEW: Foreign key constraint for tier-based licensing
    CONSTRAINT fk_product_licenses_product_tier_id 
        FOREIGN KEY (product_tier_id) REFERENCES product_tiers(id) ON DELETE SET NULL
);

-- Many-to-many relationship table for ProductLicense and ProductFeature
CREATE TABLE product_license_features (
    license_id UUID NOT NULL,
    feature_id UUID NOT NULL,
    
    PRIMARY KEY (license_id, feature_id),
    
    -- Foreign key constraints
    CONSTRAINT fk_license_features_license_id 
        FOREIGN KEY (license_id) REFERENCES product_licenses(id) ON DELETE CASCADE,
    CONSTRAINT fk_license_features_feature_id 
        FOREIGN KEY (feature_id) REFERENCES product_features(id) ON DELETE CASCADE
);

-- =============================================
-- AUDIT ENTITIES
-- =============================================

-- AuditEntryEntity table
CREATE TABLE audit_entries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(50) NOT NULL,
    action_type VARCHAR(20) NOT NULL,
    old_value VARCHAR(4000),
    new_value VARCHAR(4000),
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    reason VARCHAR(1000),
    metadata VARCHAR(1000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- =============================================
-- NOTIFICATION ENTITIES
-- =============================================

-- NotificationTemplateEntity table
CREATE TABLE notification_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    template_name VARCHAR(200) NOT NULL,
    message_template VARCHAR(4000) NOT NULL,
    notification_type VARCHAR(20),
    subject VARCHAR(500),
    template_variable_json VARCHAR(1000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    notification_mode VARCHAR(20),
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- NotificationHistoryEntity table
CREATE TABLE notification_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_id VARCHAR(50),
    entity_type VARCHAR(20),
    recipients_json VARCHAR(1000),
    notification_mode VARCHAR(20),
    notification_template_id UUID,
    notification_type VARCHAR(20),
    sent_date TIMESTAMP WITH TIME ZONE NOT NULL,
    delivery_status VARCHAR(20),
    delivery_error VARCHAR(2000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_notification_history_template_id 
        FOREIGN KEY (notification_template_id) REFERENCES notification_templates(id) ON DELETE SET NULL
);

-- =============================================
-- USER MANAGEMENT ENTITIES
-- =============================================

-- UserRoleEntity table
CREATE TABLE user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_name VARCHAR(50) NOT NULL,
    role_description VARCHAR(500),
    is_admin BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- UserProfileEntity table
CREATE TABLE user_profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_name VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(256) NOT NULL,
    password_salt VARCHAR(128) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    department VARCHAR(100),
    is_locked BOOLEAN NOT NULL DEFAULT false,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    is_admin BOOLEAN NOT NULL DEFAULT false,
    last_login_date TIMESTAMP WITH TIME ZONE,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    locked_date TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- UserRoleMappingEntity table
CREATE TABLE user_role_mappings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    assigned_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expiry_date TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_user_role_mappings_user_id 
        FOREIGN KEY (user_id) REFERENCES user_profiles(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_role_mappings_role_id 
        FOREIGN KEY (role_id) REFERENCES user_roles(id) ON DELETE CASCADE,
    
    -- Unique constraint to prevent duplicate role assignments
    CONSTRAINT uk_user_role_mappings_user_role 
        UNIQUE (user_id, role_id)
);

-- =============================================
-- SETTINGS ENTITIES
-- =============================================

-- SettingEntity table
CREATE TABLE settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    category VARCHAR(100) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value VARCHAR(4000),
    default_value VARCHAR(4000),
    data_type VARCHAR(50) NOT NULL DEFAULT 'string',
    display_name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    group_name VARCHAR(100),
    display_order INTEGER NOT NULL DEFAULT 0,
    is_editable BOOLEAN NOT NULL DEFAULT true,
    is_required BOOLEAN NOT NULL DEFAULT false,
    is_sensitive BOOLEAN NOT NULL DEFAULT false,
    validation_rules VARCHAR(2000),
    possible_values VARCHAR(2000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- =============================================
-- OPERATIONS DASHBOARD ENTITIES
-- =============================================

-- SystemMetricEntity table
CREATE TABLE system_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    metric_type VARCHAR(50) NOT NULL,
    controller VARCHAR(100),
    action VARCHAR(100),
    http_method VARCHAR(10),
    status_code INTEGER,
    request_count INTEGER NOT NULL DEFAULT 0,
    total_response_time_ms BIGINT NOT NULL DEFAULT 0,
    avg_response_time_ms INTEGER NOT NULL DEFAULT 0,
    min_response_time_ms INTEGER NOT NULL DEFAULT 0,
    max_response_time_ms INTEGER NOT NULL DEFAULT 0,
    error_count INTEGER NOT NULL DEFAULT 0,
    timeout_count INTEGER NOT NULL DEFAULT 0,
    unique_users INTEGER NOT NULL DEFAULT 0,
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- SystemHealthSnapshotEntity table
CREATE TABLE system_health_snapshots (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    snapshot_timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total_requests_last_hour INTEGER NOT NULL DEFAULT 0,
    total_errors_last_hour INTEGER NOT NULL DEFAULT 0,
    error_rate_percent DECIMAL(5,2) NOT NULL DEFAULT 0,
    avg_response_time_ms INTEGER NOT NULL DEFAULT 0,
    cpu_usage_percent DECIMAL(5,2),
    memory_usage_mb DECIMAL(10,2),
    disk_usage_percent DECIMAL(5,2),
    active_connections INTEGER,
    database_response_time_ms INTEGER,
    cache_hit_rate_percent DECIMAL(5,2),
    queue_length INTEGER,
    system_load_average DECIMAL(5,2),
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- PagePerformanceMetricEntity table
CREATE TABLE page_performance_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    page_name VARCHAR(200) NOT NULL,
    controller VARCHAR(100),
    action VARCHAR(100),
    load_count INTEGER NOT NULL DEFAULT 0,
    total_load_time_ms BIGINT NOT NULL DEFAULT 0,
    avg_load_time_ms INTEGER NOT NULL DEFAULT 0,
    min_load_time_ms INTEGER NOT NULL DEFAULT 0,
    max_load_time_ms INTEGER NOT NULL DEFAULT 0,
    bounce_count INTEGER NOT NULL DEFAULT 0,
    unique_visitors INTEGER NOT NULL DEFAULT 0,
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- QueryPerformanceMetricEntity table
CREATE TABLE query_performance_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    query_name VARCHAR(200) NOT NULL,
    query_type VARCHAR(50),
    table_name VARCHAR(100),
    execution_count INTEGER NOT NULL DEFAULT 0,
    total_execution_time_ms BIGINT NOT NULL DEFAULT 0,
    avg_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    min_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    max_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    failure_count INTEGER NOT NULL DEFAULT 0,
    timeout_count INTEGER NOT NULL DEFAULT 0,
    rows_affected INTEGER NOT NULL DEFAULT 0,
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- ErrorLogSummaryEntity table
CREATE TABLE error_log_summaries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    error_type VARCHAR(100) NOT NULL,
    error_message VARCHAR(2000),
    controller VARCHAR(100),
    action VARCHAR(100),
    source_file VARCHAR(500),
    line_number INTEGER,
    occurrence_count INTEGER NOT NULL DEFAULT 1,
    first_occurrence TIMESTAMP WITH TIME ZONE NOT NULL,
    last_occurrence TIMESTAMP WITH TIME ZONE NOT NULL,
    severity_level VARCHAR(20) NOT NULL DEFAULT 'Error',
    user_ids_affected VARCHAR(1000),
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Product indexes
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_products_is_active ON products(is_active);
CREATE INDEX idx_products_is_deleted ON products(is_deleted);

-- Product Version indexes
CREATE INDEX idx_product_versions_product_id ON product_versions(product_id);
CREATE INDEX idx_product_versions_version ON product_versions(version);
CREATE INDEX idx_product_versions_is_current ON product_versions(is_current);
CREATE INDEX idx_product_versions_is_active ON product_versions(is_active);
CREATE INDEX idx_product_versions_is_deleted ON product_versions(is_deleted);

-- Product Tier indexes
CREATE INDEX idx_product_tiers_product_id ON product_tiers(product_id);
CREATE INDEX idx_product_tiers_name ON product_tiers(name);
CREATE INDEX idx_product_tiers_is_active ON product_tiers(is_active);
CREATE INDEX idx_product_tiers_is_deleted ON product_tiers(is_deleted);

-- Product Feature indexes
CREATE INDEX idx_product_features_product_id ON product_features(product_id);
CREATE INDEX idx_product_features_tier_id ON product_features(tier_id);
CREATE INDEX idx_product_features_code ON product_features(code);
CREATE INDEX idx_product_features_is_enabled ON product_features(is_enabled);
CREATE INDEX idx_product_features_is_active ON product_features(is_active);
CREATE INDEX idx_product_features_is_deleted ON product_features(is_deleted);

-- Consumer Account indexes
CREATE INDEX idx_consumer_accounts_account_code ON consumer_accounts(account_code);
CREATE INDEX idx_consumer_accounts_primary_contact_email ON consumer_accounts(primary_contact_email);
CREATE INDEX idx_consumer_accounts_status ON consumer_accounts(status);
CREATE INDEX idx_consumer_accounts_is_active ON consumer_accounts(is_active);
CREATE INDEX idx_consumer_accounts_is_deleted ON consumer_accounts(is_deleted);
CREATE INDEX idx_consumer_accounts_company_name ON consumer_accounts(company_name);

-- Product Consumer indexes
CREATE INDEX idx_product_consumers_product_id ON product_consumers(product_id);
CREATE INDEX idx_product_consumers_consumer_id ON product_consumers(consumer_id);
CREATE INDEX idx_product_consumers_is_active ON product_consumers(is_active);
CREATE INDEX idx_product_consumers_is_deleted ON product_consumers(is_deleted);
CREATE INDEX idx_product_consumers_created_on ON product_consumers(created_on);

-- Product License indexes (Enhanced for tier-based licensing)
CREATE UNIQUE INDEX idx_product_licenses_license_key ON product_licenses(license_key);
CREATE INDEX idx_product_licenses_license_code ON product_licenses(license_code);
CREATE INDEX idx_product_licenses_product_id ON product_licenses(product_id);
CREATE INDEX idx_product_licenses_consumer_id ON product_licenses(consumer_id);
CREATE INDEX idx_product_licenses_status ON product_licenses(status);
CREATE INDEX idx_product_licenses_valid_to ON product_licenses(valid_to);
CREATE INDEX idx_product_licenses_is_active ON product_licenses(is_active);
CREATE INDEX idx_product_licenses_is_deleted ON product_licenses(is_deleted);
CREATE INDEX idx_product_licenses_product_consumer ON product_licenses(product_id, consumer_id);
-- NEW: Tier-based licensing indexes
CREATE INDEX idx_product_licenses_tier_id ON product_licenses(product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_product_tier ON product_licenses(product_id, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_status_tier ON product_licenses(status, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_version_range ON product_licenses(valid_product_version_from, valid_product_version_to);

-- Audit Entry indexes
CREATE INDEX idx_audit_entries_entity_type ON audit_entries(entity_type);
CREATE INDEX idx_audit_entries_entity_id ON audit_entries(entity_id);
CREATE INDEX idx_audit_entries_action_type ON audit_entries(action_type);
CREATE INDEX idx_audit_entries_created_on ON audit_entries(created_on);
CREATE INDEX idx_audit_entries_created_by ON audit_entries(created_by);
CREATE INDEX idx_audit_entries_is_active ON audit_entries(is_active);
CREATE INDEX idx_audit_entries_is_deleted ON audit_entries(is_deleted);
CREATE INDEX idx_audit_entries_entity_operation ON audit_entries(entity_type, entity_id);

-- Notification Template indexes
CREATE INDEX idx_notification_templates_template_name ON notification_templates(template_name);
CREATE INDEX idx_notification_templates_is_active ON notification_templates(is_active);
CREATE INDEX idx_notification_templates_is_deleted ON notification_templates(is_deleted);
CREATE INDEX idx_notification_templates_notification_type ON notification_templates(notification_type);

-- Notification History indexes
CREATE INDEX idx_notification_history_template_id ON notification_history(notification_template_id);
CREATE INDEX idx_notification_history_delivery_status ON notification_history(delivery_status);
CREATE INDEX idx_notification_history_notification_type ON notification_history(notification_type);
CREATE INDEX idx_notification_history_sent_date ON notification_history(sent_date);
CREATE INDEX idx_notification_history_is_active ON notification_history(is_active);
CREATE INDEX idx_notification_history_is_deleted ON notification_history(is_deleted);
CREATE INDEX idx_notification_history_entity ON notification_history(entity_type, entity_id);

-- User Management indexes
CREATE INDEX idx_user_profiles_user_name ON user_profiles(user_name);
CREATE INDEX idx_user_profiles_email ON user_profiles(email);
CREATE INDEX idx_user_profiles_is_active ON user_profiles(is_active);
CREATE INDEX idx_user_profiles_is_locked ON user_profiles(is_locked);
CREATE INDEX idx_user_profiles_is_deleted ON user_profiles(is_deleted);
CREATE INDEX idx_user_profiles_created_on ON user_profiles(created_on);

CREATE INDEX idx_user_roles_role_name ON user_roles(role_name);
CREATE INDEX idx_user_roles_is_active ON user_roles(is_active);
CREATE INDEX idx_user_roles_is_deleted ON user_roles(is_deleted);

CREATE INDEX idx_user_role_mappings_user_id ON user_role_mappings(user_id);
CREATE INDEX idx_user_role_mappings_role_id ON user_role_mappings(role_id);
CREATE INDEX idx_user_role_mappings_assigned_date ON user_role_mappings(assigned_date);
CREATE INDEX idx_user_role_mappings_is_active ON user_role_mappings(is_active);
CREATE INDEX idx_user_role_mappings_is_deleted ON user_role_mappings(is_deleted);

-- Settings indexes
CREATE INDEX idx_settings_category ON settings(category);
CREATE INDEX idx_settings_key ON settings(key);
CREATE INDEX idx_settings_category_key ON settings(category, key);
CREATE INDEX idx_settings_is_active ON settings(is_active);
CREATE INDEX idx_settings_is_deleted ON settings(is_deleted);
CREATE INDEX idx_settings_display_order ON settings(display_order);

-- Operations Dashboard indexes
CREATE INDEX idx_system_metrics_timestamp_hour ON system_metrics(timestamp_hour);
CREATE INDEX idx_system_metrics_metric_type ON system_metrics(metric_type);
CREATE INDEX idx_system_metrics_controller ON system_metrics(controller);
CREATE INDEX idx_system_metrics_status_code ON system_metrics(status_code);
CREATE INDEX idx_system_metrics_environment ON system_metrics(environment);
CREATE INDEX idx_system_metrics_is_active ON system_metrics(is_active);
CREATE INDEX idx_system_metrics_is_deleted ON system_metrics(is_deleted);

CREATE INDEX idx_system_health_snapshots_timestamp ON system_health_snapshots(snapshot_timestamp);
CREATE INDEX idx_system_health_snapshots_environment ON system_health_snapshots(environment);
CREATE INDEX idx_system_health_snapshots_is_active ON system_health_snapshots(is_active);
CREATE INDEX idx_system_health_snapshots_is_deleted ON system_health_snapshots(is_deleted);

CREATE INDEX idx_page_performance_metrics_timestamp_hour ON page_performance_metrics(timestamp_hour);
CREATE INDEX idx_page_performance_metrics_page_name ON page_performance_metrics(page_name);
CREATE INDEX idx_page_performance_metrics_environment ON page_performance_metrics(environment);
CREATE INDEX idx_page_performance_metrics_is_active ON page_performance_metrics(is_active);
CREATE INDEX idx_page_performance_metrics_is_deleted ON page_performance_metrics(is_deleted);

CREATE INDEX idx_query_performance_metrics_timestamp_hour ON query_performance_metrics(timestamp_hour);
CREATE INDEX idx_query_performance_metrics_query_name ON query_performance_metrics(query_name);
CREATE INDEX idx_query_performance_metrics_table_name ON query_performance_metrics(table_name);
CREATE INDEX idx_query_performance_metrics_environment ON query_performance_metrics(environment);
CREATE INDEX idx_query_performance_metrics_is_active ON query_performance_metrics(is_active);
CREATE INDEX idx_query_performance_metrics_is_deleted ON query_performance_metrics(is_deleted);

CREATE INDEX idx_error_log_summaries_timestamp_hour ON error_log_summaries(timestamp_hour);
CREATE INDEX idx_error_log_summaries_error_type ON error_log_summaries(error_type);
CREATE INDEX idx_error_log_summaries_severity_level ON error_log_summaries(severity_level);
CREATE INDEX idx_error_log_summaries_environment ON error_log_summaries(environment);
CREATE INDEX idx_error_log_summaries_is_active ON error_log_summaries(is_active);
CREATE INDEX idx_error_log_summaries_is_deleted ON error_log_summaries(is_deleted);

-- =============================================
-- UNIQUE CONSTRAINTS
-- =============================================

-- Product unique constraints
ALTER TABLE products ADD CONSTRAINT uk_products_name UNIQUE (name);

-- Product Version unique constraints
ALTER TABLE product_versions ADD CONSTRAINT uk_product_versions_product_version UNIQUE (product_id, version);

-- Product Tier unique constraints  
ALTER TABLE product_tiers ADD CONSTRAINT uk_product_tiers_product_tier UNIQUE (product_id, name);

-- Product Feature unique constraints
ALTER TABLE product_features ADD CONSTRAINT uk_product_features_tier_code UNIQUE (tier_id, code);

-- Consumer Account unique constraints
ALTER TABLE consumer_accounts ADD CONSTRAINT uk_consumer_accounts_account_code UNIQUE (account_code);
ALTER TABLE consumer_accounts ADD CONSTRAINT uk_consumer_accounts_primary_email UNIQUE (primary_contact_email);

-- Product Consumer unique constraints
ALTER TABLE product_consumers ADD CONSTRAINT uk_product_consumers_product_consumer UNIQUE (consumer_id, product_id);

-- Product License unique constraints
ALTER TABLE product_licenses ADD CONSTRAINT uk_product_licenses_license_code UNIQUE (license_code);

-- Settings unique constraints
ALTER TABLE settings ADD CONSTRAINT uk_settings_category_key UNIQUE (category, key);

-- =============================================
-- VIEWS (Optional - for common queries)
-- =============================================

-- View for active licenses with product and consumer information (Enhanced for tier-based licensing)
CREATE OR REPLACE VIEW active_licenses_view AS
SELECT 
    pl.id,
    pl.license_code,
    pl.license_key,
    p.name as product_name,
    ca.company_name,
    ca.primary_contact_email,
    pl.status,
    pl.valid_from,
    pl.valid_to,
    -- NEW: Tier-based licensing fields
    pl.product_tier_id,
    pt.name as tier_name,
    pl.valid_product_version_from,
    pl.valid_product_version_to,
    -- License type classification
    CASE 
        WHEN pl.product_tier_id IS NOT NULL THEN 'Tier-Based'
        ELSE 'Feature-Based'
    END AS license_type
FROM product_licenses pl
INNER JOIN products p ON pl.product_id = p.id
INNER JOIN consumer_accounts ca ON pl.consumer_id = ca.id
LEFT JOIN product_tiers pt ON pl.product_tier_id = pt.id
WHERE pl.status = 'Active' 
  AND pl.valid_to > CURRENT_TIMESTAMP
  AND ca.is_active = true
  AND pl.is_active = true
  AND ca.is_deleted = false
  AND pl.is_deleted = false;

-- View for product feature summary
CREATE OR REPLACE VIEW product_features_summary AS
SELECT 
    p.id as product_id,
    p.name as product_name,
    pt.id as tier_id,
    pt.name as tier_name,
    COUNT(pf.id) as total_features,
    COUNT(CASE WHEN pf.is_enabled = true THEN 1 END) as active_features
FROM products p
LEFT JOIN product_tiers pt ON p.id = pt.product_id
LEFT JOIN product_features pf ON pt.id = pf.tier_id
WHERE p.status = 'Active' 
  AND p.is_active = true 
  AND p.is_deleted = false
  AND (pt.is_deleted = false OR pt.is_deleted IS NULL)
  AND (pf.is_deleted = false OR pf.is_deleted IS NULL)
GROUP BY p.id, p.name, pt.id, pt.name;

-- View for user details with roles
CREATE OR REPLACE VIEW v_user_details AS
SELECT 
    up.id as user_id,
    up.user_name,
    up.full_name,
    up.email,
    up.department,
    up.is_locked,
    up.is_deleted,
    up.is_admin,
    up.last_login_date,
    up.failed_login_attempts,
    up.locked_date,
    up.is_active,
    up.created_on,
    ur.id as role_id,
    ur.role_name,
    ur.role_description,
    urm.assigned_date,
    urm.expiry_date
FROM user_profiles up
LEFT JOIN user_role_mappings urm ON up.id = urm.user_id AND urm.is_active = true AND urm.is_deleted = false
LEFT JOIN user_roles ur ON urm.role_id = ur.id AND ur.is_active = true AND ur.is_deleted = false
WHERE up.is_active = true AND up.is_deleted = false;

-- View for system health summary
CREATE OR REPLACE VIEW v_system_health_summary AS
SELECT 
    environment,
    AVG(error_rate_percent) as avg_error_rate,
    AVG(avg_response_time_ms) as avg_response_time,
    AVG(cpu_usage_percent) as avg_cpu_usage,
    AVG(memory_usage_mb) as avg_memory_usage,
    COUNT(*) as snapshot_count,
    MAX(snapshot_timestamp) as latest_snapshot
FROM system_health_snapshots
WHERE snapshot_timestamp >= CURRENT_TIMESTAMP - INTERVAL '24 hours'
  AND is_active = true
  AND is_deleted = false
GROUP BY environment;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON TABLE products IS 'Core product definitions and metadata';         
COMMENT ON TABLE product_versions IS 'Product version tracking with release information';
COMMENT ON TABLE product_tiers IS 'Product pricing tiers and service levels';
COMMENT ON TABLE product_features IS 'Individual product features and capabilities';
COMMENT ON TABLE consumer_accounts IS 'Customer accounts and organization information';
COMMENT ON TABLE product_consumers IS 'Assignment of products to consumer accounts';
COMMENT ON TABLE product_licenses IS 'License instances with keys and validity periods - Enhanced for tier-based licensing';
COMMENT ON TABLE product_license_features IS 'Many-to-many relationship between licenses and features (backward compatibility)';

-- NEW: Comments for tier-based licensing columns
COMMENT ON COLUMN product_licenses.product_tier_id IS 'Foreign key to product_tiers table for tier-based licensing (NULL for feature-based licenses)';
COMMENT ON COLUMN product_licenses.valid_product_version_from IS 'Minimum product version that this license supports';
COMMENT ON COLUMN product_licenses.valid_product_version_to IS 'Maximum product version that this license supports (NULL means no upper limit)';

COMMENT ON TABLE audit_entries IS 'Audit trail for all entity changes';
COMMENT ON TABLE notification_templates IS 'Templates for system notifications';
COMMENT ON TABLE notification_history IS 'History of sent notifications and delivery status';
COMMENT ON TABLE user_profiles IS 'User account information and authentication data';
COMMENT ON TABLE user_roles IS 'Role definitions for user authorization';
COMMENT ON TABLE user_role_mappings IS 'Assignment of roles to users';
COMMENT ON TABLE settings IS 'System configuration settings and preferences';
COMMENT ON TABLE system_metrics IS 'Aggregated system performance metrics';
COMMENT ON TABLE system_health_snapshots IS 'Point-in-time system health snapshots';
COMMENT ON TABLE page_performance_metrics IS 'Web page performance tracking';
COMMENT ON TABLE query_performance_metrics IS 'Database query performance metrics';
COMMENT ON TABLE error_log_summaries IS 'Aggregated error logs and exception tracking';

-- =============================================
-- END OF SCRIPT
-- =============================================

-- Script execution completed successfully
-- Database schema created with:
-- - 18 main tables with proper relationships
-- - All entities use UUID 'id' as primary key (aligned with BaseAuditEntity)
-- - 2 many-to-many junction tables  
-- - Comprehensive indexes for performance (including is_deleted)
-- - Unique constraints for data integrity
-- - Audit trail support with is_active and is_deleted soft delete
-- - Views for common queries (updated with correct column references)
-- - User management system with role-based access control
-- - Settings management system
-- - Operations dashboard metrics and monitoring
-- - All entities aligned with actual C# entity definitions
-- - ENHANCED: Tier-based licensing support with backward compatibility
-- - NEW: Version range support for product compatibility
-- - NEW: Enhanced views and indexes for tier-based licensing

COMMIT;
