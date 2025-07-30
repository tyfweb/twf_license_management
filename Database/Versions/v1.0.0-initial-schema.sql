-- TechWayFit Licensing Management System - PostgreSQL Database Creation Script
-- Generated from actual Entity Framework Core entities and DbContext configurations
-- Compatible with PostgreSQL 12+
-- Version: 1.0.0-corrected

-- Enable UUID extension for better ID generation (optional)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- CORE PRODUCT ENTITIES
-- =============================================

-- ProductEntity table
CREATE TABLE products (
    product_id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    release_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    support_email VARCHAR(255),
    support_phone VARCHAR(50),
    decommission_date TIMESTAMP WITH TIME ZONE,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ProductVersionEntity table
CREATE TABLE product_versions (
    version_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    version VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    release_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    end_of_life_date TIMESTAMP WITH TIME ZONE,
    support_end_date TIMESTAMP WITH TIME ZONE,
    release_notes VARCHAR(2000),
    is_current BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_versions_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- ProductTierEntity table
CREATE TABLE product_tiers (
    tier_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    price VARCHAR(10),
    display_order INTEGER NOT NULL DEFAULT 0,
    support_sla_json VARCHAR(1000) DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_tiers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- ProductFeatureEntity table
CREATE TABLE product_features (
    feature_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    tier_id VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    code VARCHAR(100) NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT true,
    display_order INTEGER DEFAULT 0,
    support_from_version VARCHAR(20) DEFAULT '1.0.0',
    support_to_version VARCHAR(20) DEFAULT '9999.0.0',
    feature_usage_json VARCHAR(1000) DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_features_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    CONSTRAINT fk_product_features_tier_id 
        FOREIGN KEY (tier_id) REFERENCES product_tiers(tier_id) ON DELETE CASCADE
);

-- =============================================
-- CONSUMER ENTITIES
-- =============================================

-- ConsumerAccountEntity table
CREATE TABLE consumer_accounts (
    consumer_id VARCHAR(50) PRIMARY KEY,
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
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ProductConsumerEntity table (Many-to-many relationship between products and consumers)
CREATE TABLE product_consumers (
    product_consumer_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    consumer_id VARCHAR(50) NOT NULL,
    account_manager_name VARCHAR(100),
    account_manager_email VARCHAR(255),
    account_manager_phone VARCHAR(50),
    account_manager_position VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_consumers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    CONSTRAINT fk_product_consumers_consumer_id 
        FOREIGN KEY (consumer_id) REFERENCES consumer_accounts(consumer_id) ON DELETE CASCADE
);

-- =============================================
-- LICENSE ENTITIES
-- =============================================

-- ProductLicenseEntity table
CREATE TABLE product_licenses (
    license_id VARCHAR(50) PRIMARY KEY,
    license_code VARCHAR(50) NOT NULL,
    product_id VARCHAR(50) NOT NULL,
    consumer_id VARCHAR(50) NOT NULL,
    valid_from TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_to TIMESTAMP WITH TIME ZONE NOT NULL,
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
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_licenses_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT,
    CONSTRAINT fk_product_licenses_consumer_id 
        FOREIGN KEY (consumer_id) REFERENCES consumer_accounts(consumer_id) ON DELETE RESTRICT
);

-- Many-to-many relationship table for ProductLicense and ProductFeature
CREATE TABLE product_license_features (
    license_id VARCHAR(50) NOT NULL,
    feature_id VARCHAR(50) NOT NULL,
    
    PRIMARY KEY (license_id, feature_id),
    
    -- Foreign key constraints
    CONSTRAINT fk_license_features_license_id 
        FOREIGN KEY (license_id) REFERENCES product_licenses(license_id) ON DELETE CASCADE,
    CONSTRAINT fk_license_features_feature_id 
        FOREIGN KEY (feature_id) REFERENCES product_features(feature_id) ON DELETE CASCADE
);

-- =============================================
-- AUDIT ENTITIES
-- =============================================

-- AuditEntryEntity table
CREATE TABLE audit_entries (
    audit_entry_id VARCHAR(50) PRIMARY KEY,
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
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- =============================================
-- NOTIFICATION ENTITIES
-- =============================================

-- NotificationTemplateEntity table
CREATE TABLE notification_templates (
    notification_template_id VARCHAR(50) PRIMARY KEY,
    template_name VARCHAR(200) NOT NULL,
    message_template VARCHAR(4000) NOT NULL,
    notification_type VARCHAR(20),
    subject VARCHAR(500),
    template_variable_json VARCHAR(1000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    notification_mode VARCHAR(20),
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- NotificationHistoryEntity table
CREATE TABLE notification_history (
    notification_id VARCHAR(50) PRIMARY KEY,
    entity_id VARCHAR(50),
    entity_type VARCHAR(20),
    recipients_json VARCHAR(1000),
    notification_mode VARCHAR(20),
    notification_template_id VARCHAR(50),
    notification_type VARCHAR(20),
    sent_date TIMESTAMP WITH TIME ZONE NOT NULL,
    delivery_status VARCHAR(20),
    delivery_error VARCHAR(2000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_notification_history_template_id 
        FOREIGN KEY (notification_template_id) REFERENCES notification_templates(notification_template_id) ON DELETE SET NULL
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Product indexes
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_products_is_active ON products(is_active);

-- Product Version indexes
CREATE INDEX idx_product_versions_product_id ON product_versions(product_id);
CREATE INDEX idx_product_versions_version ON product_versions(version);
CREATE INDEX idx_product_versions_is_current ON product_versions(is_current);
CREATE INDEX idx_product_versions_is_active ON product_versions(is_active);

-- Product Tier indexes
CREATE INDEX idx_product_tiers_product_id ON product_tiers(product_id);
CREATE INDEX idx_product_tiers_name ON product_tiers(name);
CREATE INDEX idx_product_tiers_is_active ON product_tiers(is_active);

-- Product Feature indexes
CREATE INDEX idx_product_features_product_id ON product_features(product_id);
CREATE INDEX idx_product_features_tier_id ON product_features(tier_id);
CREATE INDEX idx_product_features_code ON product_features(code);
CREATE INDEX idx_product_features_is_enabled ON product_features(is_enabled);
CREATE INDEX idx_product_features_is_active ON product_features(is_active);

-- Consumer Account indexes
CREATE INDEX idx_consumer_accounts_account_code ON consumer_accounts(account_code);
CREATE INDEX idx_consumer_accounts_primary_contact_email ON consumer_accounts(primary_contact_email);
CREATE INDEX idx_consumer_accounts_status ON consumer_accounts(status);
CREATE INDEX idx_consumer_accounts_is_active ON consumer_accounts(is_active);
CREATE INDEX idx_consumer_accounts_company_name ON consumer_accounts(company_name);

-- Product Consumer indexes
CREATE INDEX idx_product_consumers_product_id ON product_consumers(product_id);
CREATE INDEX idx_product_consumers_consumer_id ON product_consumers(consumer_id);
CREATE INDEX idx_product_consumers_is_active ON product_consumers(is_active);
CREATE INDEX idx_product_consumers_created_on ON product_consumers(created_on);

-- Product License indexes
CREATE UNIQUE INDEX idx_product_licenses_license_key ON product_licenses(license_key);
CREATE INDEX idx_product_licenses_license_code ON product_licenses(license_code);
CREATE INDEX idx_product_licenses_product_id ON product_licenses(product_id);
CREATE INDEX idx_product_licenses_consumer_id ON product_licenses(consumer_id);
CREATE INDEX idx_product_licenses_status ON product_licenses(status);
CREATE INDEX idx_product_licenses_valid_to ON product_licenses(valid_to);
CREATE INDEX idx_product_licenses_is_active ON product_licenses(is_active);
CREATE INDEX idx_product_licenses_product_consumer ON product_licenses(product_id, consumer_id);

-- Audit Entry indexes
CREATE INDEX idx_audit_entries_entity_type ON audit_entries(entity_type);
CREATE INDEX idx_audit_entries_entity_id ON audit_entries(entity_id);
CREATE INDEX idx_audit_entries_action_type ON audit_entries(action_type);
CREATE INDEX idx_audit_entries_created_on ON audit_entries(created_on);
CREATE INDEX idx_audit_entries_created_by ON audit_entries(created_by);
CREATE INDEX idx_audit_entries_is_active ON audit_entries(is_active);
CREATE INDEX idx_audit_entries_entity_operation ON audit_entries(entity_type, entity_id);

-- Notification Template indexes
CREATE INDEX idx_notification_templates_template_name ON notification_templates(template_name);
CREATE INDEX idx_notification_templates_is_active ON notification_templates(is_active);
CREATE INDEX idx_notification_templates_notification_type ON notification_templates(notification_type);

-- Notification History indexes
CREATE INDEX idx_notification_history_template_id ON notification_history(notification_template_id);
CREATE INDEX idx_notification_history_delivery_status ON notification_history(delivery_status);
CREATE INDEX idx_notification_history_notification_type ON notification_history(notification_type);
CREATE INDEX idx_notification_history_sent_date ON notification_history(sent_date);
CREATE INDEX idx_notification_history_is_active ON notification_history(is_active);
CREATE INDEX idx_notification_history_entity ON notification_history(entity_type, entity_id);

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

-- =============================================
-- VIEWS (Optional - for common queries)
-- =============================================

-- View for active licenses with product and consumer information
CREATE OR REPLACE VIEW active_licenses_view AS
SELECT 
    pl.license_id,
    pl.license_code,
    pl.license_key,
    p.name as product_name,
    ca.company_name,
    ca.primary_contact_email,
    pl.status,
    pl.valid_from,
    pl.valid_to
FROM product_licenses pl
INNER JOIN products p ON pl.product_id = p.product_id
INNER JOIN consumer_accounts ca ON pl.consumer_id = ca.consumer_id
WHERE pl.status = 'Active' 
  AND pl.valid_to > CURRENT_TIMESTAMP
  AND ca.is_active = true
  AND pl.is_active = true;

-- View for product feature summary
CREATE OR REPLACE VIEW product_features_summary AS
SELECT 
    p.product_id,
    p.name as product_name,
    pt.tier_id,
    pt.name as tier_name,
    COUNT(pf.feature_id) as total_features,
    COUNT(CASE WHEN pf.is_enabled = true THEN 1 END) as active_features
FROM products p
LEFT JOIN product_tiers pt ON p.product_id = pt.product_id
LEFT JOIN product_features pf ON pt.tier_id = pf.tier_id
WHERE p.status = 'Active' AND p.is_active = true
GROUP BY p.product_id, p.name, pt.tier_id, pt.name;

-- =============================================
-- USER MANAGEMENT ENTITIES
-- =============================================

-- User roles table
CREATE TABLE user_roles (
    role_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_name VARCHAR(50) NOT NULL,
    role_description VARCHAR(500),
    is_admin BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- User profiles table
CREATE TABLE user_profiles (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
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
    updated_on TIMESTAMP WITH TIME ZONE
);

-- User role mappings table
CREATE TABLE user_role_mappings (
    mapping_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    assigned_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expiry_date TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_user_role_mappings_user_id 
        FOREIGN KEY (user_id) REFERENCES user_profiles(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_role_mappings_role_id 
        FOREIGN KEY (role_id) REFERENCES user_roles(role_id) ON DELETE CASCADE,
    
    -- Unique constraint to prevent duplicate role assignments
    CONSTRAINT uk_user_role_mappings_user_role 
        UNIQUE (user_id, role_id)
);

-- =============================================
-- USER MANAGEMENT INDEXES
-- =============================================

-- Indexes for user_profiles
CREATE INDEX idx_user_profiles_user_name ON user_profiles(user_name);
CREATE INDEX idx_user_profiles_email ON user_profiles(email);
CREATE INDEX idx_user_profiles_is_active ON user_profiles(is_active);
CREATE INDEX idx_user_profiles_is_locked ON user_profiles(is_locked);
CREATE INDEX idx_user_profiles_is_deleted ON user_profiles(is_deleted);
CREATE INDEX idx_user_profiles_created_on ON user_profiles(created_on);

-- Indexes for user_roles
CREATE INDEX idx_user_roles_role_name ON user_roles(role_name);
CREATE INDEX idx_user_roles_is_active ON user_roles(is_active);

-- Indexes for user_role_mappings
CREATE INDEX idx_user_role_mappings_user_id ON user_role_mappings(user_id);
CREATE INDEX idx_user_role_mappings_role_id ON user_role_mappings(role_id);
CREATE INDEX idx_user_role_mappings_assigned_date ON user_role_mappings(assigned_date);
CREATE INDEX idx_user_role_mappings_is_active ON user_role_mappings(is_active);

-- =============================================
-- USER MANAGEMENT VIEWS
-- =============================================

-- View for user details with roles
CREATE VIEW v_user_details AS
SELECT 
    up.user_id,
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
    ur.role_id,
    ur.role_name,
    ur.role_description,
    urm.assigned_date,
    urm.expiry_date
FROM user_profiles up
LEFT JOIN user_role_mappings urm ON up.user_id = urm.user_id AND urm.is_active = true
LEFT JOIN user_roles ur ON urm.role_id = ur.role_id AND ur.is_active = true
WHERE up.is_active = true AND up.is_deleted = false;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON TABLE products IS 'Core product definitions and metadata';         
COMMENT ON TABLE product_versions IS 'Product version tracking with release information';
COMMENT ON TABLE product_tiers IS 'Product pricing tiers and service levels';
COMMENT ON TABLE product_features IS 'Individual product features and capabilities';
COMMENT ON TABLE consumer_accounts IS 'Customer accounts and organization information';
COMMENT ON TABLE product_consumers IS 'Assignment of products to consumer accounts';
COMMENT ON TABLE product_licenses IS 'License instances with keys and validity periods';
COMMENT ON TABLE product_license_features IS 'Many-to-many relationship between licenses and features';
COMMENT ON TABLE audit_entries IS 'Audit trail for all entity changes';
COMMENT ON TABLE notification_templates IS 'Templates for system notifications';
COMMENT ON TABLE notification_history IS 'History of sent notifications and delivery status';
COMMENT ON TABLE user_profiles IS 'User account information and authentication data';
COMMENT ON TABLE user_roles IS 'Role definitions for user authorization';
COMMENT ON TABLE user_role_mappings IS 'Assignment of roles to users';

-- =============================================
-- END OF SCRIPT
-- =============================================

-- Script execution completed successfully
-- Database schema created with:
-- - 13 main tables with proper relationships (including user management)
-- - 2 many-to-many junction tables  
-- - Comprehensive indexes for performance
-- - Unique constraints for data integrity
-- - Audit trail support with is_active soft delete
-- - Views for common queries
-- - User management system with role-based access control
-- - All entities aligned with actual C# entity definitions

COMMIT;
