-- TechWayFit Licensing Management System - PostgreSQL Database Creation Script
-- Generated from Entity Framework Core configurations in LicensingDbContext
-- Compatible with PostgreSQL 12+

-- Create database (run this separately as a superuser if needed)
-- CREATE DATABASE techwayfit_licensing;

-- Connect to the database and create schema
-- \c techwayfit_licensing;

-- Enable UUID extension for better ID generation (optional)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create sequences for auto-incrementing fields (if needed)

-- =============================================
-- CORE PRODUCT ENTITIES
-- =============================================

-- ProductEntity table
CREATE TABLE products (
    product_id VARCHAR(50) PRIMARY KEY,
    product_name VARCHAR(200) NOT NULL,
    product_code VARCHAR(100) NOT NULL,
    description TEXT,
    vendor_name VARCHAR(200),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ProductVersionEntity table
CREATE TABLE product_versions (
    product_version_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    version_number VARCHAR(50) NOT NULL,
    version_name VARCHAR(200),
    description TEXT,
    release_date TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_versions_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);

-- ProductTierEntity table
CREATE TABLE product_tiers (
    product_tier_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    tier_name VARCHAR(200) NOT NULL,
    tier_code VARCHAR(100) NOT NULL,
    description TEXT,
    pricing_model VARCHAR(50),
    base_price DECIMAL(18,2),
    is_active BOOLEAN NOT NULL DEFAULT true,
    display_order INTEGER,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_tiers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);

-- ProductFeatureEntity table
CREATE TABLE product_features (
    product_feature_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    feature_name VARCHAR(200) NOT NULL,
    feature_code VARCHAR(100) NOT NULL,
    description TEXT,
    feature_type VARCHAR(50),
    is_core_feature BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_features_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);

-- =============================================
-- CONSUMER ENTITIES
-- =============================================

-- ConsumerAccountEntity table
CREATE TABLE consumer_accounts (
    consumer_account_id VARCHAR(50) PRIMARY KEY,
    account_name VARCHAR(200) NOT NULL,
    account_code VARCHAR(100) NOT NULL,
    contact_email VARCHAR(255),
    contact_name VARCHAR(200),
    contact_phone VARCHAR(50),
    organization_name VARCHAR(300),
    billing_address TEXT,
    account_status VARCHAR(20) NOT NULL DEFAULT 'Active',
    subscription_start TIMESTAMP WITH TIME ZONE,
    subscription_end TIMESTAMP WITH TIME ZONE,
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
    consumer_account_id VARCHAR(50) NOT NULL,
    assigned_date TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    notes TEXT,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_consumers_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT,
    CONSTRAINT fk_product_consumers_consumer_account_id 
        FOREIGN KEY (consumer_account_id) REFERENCES consumer_accounts(consumer_account_id) ON DELETE RESTRICT
);

-- =============================================
-- LICENSE ENTITIES
-- =============================================

-- ProductLicenseEntity table
CREATE TABLE product_licenses (
    product_license_id VARCHAR(50) PRIMARY KEY,
    product_id VARCHAR(50) NOT NULL,
    consumer_id VARCHAR(50) NOT NULL,
    license_key VARCHAR(500) NOT NULL,
    license_type VARCHAR(50),
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    valid_from TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_to TIMESTAMP WITH TIME ZONE NOT NULL,
    max_users INTEGER,
    max_installations INTEGER,
    license_data_json TEXT,
    notes TEXT,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Foreign key constraints
    CONSTRAINT fk_product_licenses_product_id 
        FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT,
    CONSTRAINT fk_product_licenses_consumer_id 
        FOREIGN KEY (consumer_id) REFERENCES consumer_accounts(consumer_account_id) ON DELETE RESTRICT
);

-- Many-to-many relationship table for ProductLicense and ProductFeature
CREATE TABLE product_license_features (
    product_license_id VARCHAR(50) NOT NULL,
    product_feature_id VARCHAR(50) NOT NULL,
    
    PRIMARY KEY (product_license_id, product_feature_id),
    
    -- Foreign key constraints
    CONSTRAINT fk_license_features_license_id 
        FOREIGN KEY (product_license_id) REFERENCES product_licenses(product_license_id) ON DELETE CASCADE,
    CONSTRAINT fk_license_features_feature_id 
        FOREIGN KEY (product_feature_id) REFERENCES product_features(product_feature_id) ON DELETE CASCADE
);

-- Many-to-many relationship table for ProductTier and ProductFeature
CREATE TABLE product_tier_features (
    product_tier_id VARCHAR(50) NOT NULL,
    product_feature_id VARCHAR(50) NOT NULL,
    
    PRIMARY KEY (product_tier_id, product_feature_id),
    
    -- Foreign key constraints
    CONSTRAINT fk_tier_features_tier_id 
        FOREIGN KEY (product_tier_id) REFERENCES product_tiers(product_tier_id) ON DELETE CASCADE,
    CONSTRAINT fk_tier_features_feature_id 
        FOREIGN KEY (product_feature_id) REFERENCES product_features(product_feature_id) ON DELETE CASCADE
);

-- =============================================
-- AUDIT ENTITIES
-- =============================================

-- AuditEntryEntity table
CREATE TABLE audit_entries (
    audit_entry_id VARCHAR(50) PRIMARY KEY,
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(50) NOT NULL,
    operation_type VARCHAR(20) NOT NULL,
    changes_json TEXT,
    user_id VARCHAR(100),
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    additional_data_json TEXT,
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
CREATE INDEX idx_products_product_code ON products(product_code);
CREATE INDEX idx_products_is_active ON products(is_active);
CREATE INDEX idx_products_vendor_name ON products(vendor_name);

-- Product Version indexes
CREATE INDEX idx_product_versions_product_id ON product_versions(product_id);
CREATE INDEX idx_product_versions_version_number ON product_versions(version_number);
CREATE INDEX idx_product_versions_is_active ON product_versions(is_active);

-- Product Tier indexes
CREATE INDEX idx_product_tiers_product_id ON product_tiers(product_id);
CREATE INDEX idx_product_tiers_tier_code ON product_tiers(tier_code);
CREATE INDEX idx_product_tiers_is_active ON product_tiers(is_active);

-- Product Feature indexes
CREATE INDEX idx_product_features_product_id ON product_features(product_id);
CREATE INDEX idx_product_features_feature_code ON product_features(feature_code);
CREATE INDEX idx_product_features_is_active ON product_features(is_active);

-- Consumer Account indexes
CREATE INDEX idx_consumer_accounts_account_code ON consumer_accounts(account_code);
CREATE INDEX idx_consumer_accounts_contact_email ON consumer_accounts(contact_email);
CREATE INDEX idx_consumer_accounts_account_status ON consumer_accounts(account_status);
CREATE INDEX idx_consumer_accounts_is_active ON consumer_accounts(is_active);

-- Product Consumer indexes
CREATE INDEX idx_product_consumers_product_id ON product_consumers(product_id);
CREATE INDEX idx_product_consumers_consumer_account_id ON product_consumers(consumer_account_id);
CREATE INDEX idx_product_consumers_status ON product_consumers(status);
CREATE INDEX idx_product_consumers_assigned_date ON product_consumers(assigned_date);
CREATE INDEX idx_product_consumers_product_consumer ON product_consumers(product_id, consumer_account_id);

-- Product License indexes
CREATE UNIQUE INDEX idx_product_licenses_license_key ON product_licenses(license_key);
CREATE INDEX idx_product_licenses_product_id ON product_licenses(product_id);
CREATE INDEX idx_product_licenses_consumer_id ON product_licenses(consumer_id);
CREATE INDEX idx_product_licenses_status ON product_licenses(status);
CREATE INDEX idx_product_licenses_valid_to ON product_licenses(valid_to);
CREATE INDEX idx_product_licenses_product_consumer ON product_licenses(product_id, consumer_id);

-- Audit Entry indexes
CREATE INDEX idx_audit_entries_entity_type ON audit_entries(entity_type);
CREATE INDEX idx_audit_entries_entity_id ON audit_entries(entity_id);
CREATE INDEX idx_audit_entries_operation_type ON audit_entries(operation_type);
CREATE INDEX idx_audit_entries_timestamp ON audit_entries(timestamp);
CREATE INDEX idx_audit_entries_user_id ON audit_entries(user_id);
CREATE INDEX idx_audit_entries_entity_operation ON audit_entries(entity_type, entity_id);

-- Notification Template indexes
CREATE INDEX idx_notification_templates_template_name ON notification_templates(template_name);
CREATE INDEX idx_notification_templates_is_active ON notification_templates(is_active);

-- Notification History indexes
CREATE INDEX idx_notification_history_template_id ON notification_history(notification_template_id);
CREATE INDEX idx_notification_history_delivery_status ON notification_history(delivery_status);
CREATE INDEX idx_notification_history_notification_type ON notification_history(notification_type);
CREATE INDEX idx_notification_history_sent_date ON notification_history(sent_date);
CREATE INDEX idx_notification_history_entity ON notification_history(entity_type, entity_id);

-- =============================================
-- UNIQUE CONSTRAINTS
-- =============================================

-- Product unique constraints
ALTER TABLE products ADD CONSTRAINT uk_products_product_code UNIQUE (product_code);

-- Product Version unique constraints
ALTER TABLE product_versions ADD CONSTRAINT uk_product_versions_product_version UNIQUE (product_id, version_number);

-- Product Tier unique constraints  
ALTER TABLE product_tiers ADD CONSTRAINT uk_product_tiers_product_tier UNIQUE (product_id, tier_code);

-- Product Feature unique constraints
ALTER TABLE product_features ADD CONSTRAINT uk_product_features_product_feature UNIQUE (product_id, feature_code);

-- Consumer Account unique constraints
ALTER TABLE consumer_accounts ADD CONSTRAINT uk_consumer_accounts_account_code UNIQUE (account_code);

-- Product Consumer unique constraints
ALTER TABLE product_consumers ADD CONSTRAINT uk_product_consumers_product_consumer UNIQUE (product_id, consumer_account_id);

-- =============================================
-- SAMPLE DATA (Optional - for testing)
-- =============================================

-- Insert sample data for testing
/*
-- Sample Product
INSERT INTO products (product_id, product_name, product_code, description, vendor_name, is_active, created_by, created_on)
VALUES ('prod-001', 'TechWayFit Enterprise', 'TWF-ENT', 'Enterprise fitness management solution', 'TechWayFit Inc.', true, 'system', CURRENT_TIMESTAMP);

-- Sample Product Version
INSERT INTO product_versions (product_version_id, product_id, version_number, version_name, description, release_date, is_active, created_by, created_on)
VALUES ('ver-001', 'prod-001', '1.0.0', 'Initial Release', 'First major release', CURRENT_TIMESTAMP, true, 'system', CURRENT_TIMESTAMP);

-- Sample Product Tier
INSERT INTO product_tiers (product_tier_id, product_id, tier_name, tier_code, description, pricing_model, base_price, is_active, display_order, created_by, created_on)
VALUES ('tier-001', 'prod-001', 'Basic', 'BASIC', 'Basic tier with core features', 'monthly', 99.99, true, 1, 'system', CURRENT_TIMESTAMP);

-- Sample Product Feature
INSERT INTO product_features (product_feature_id, product_id, feature_name, feature_code, description, feature_type, is_core_feature, is_active, created_by, created_on)
VALUES ('feat-001', 'prod-001', 'User Management', 'USER_MGMT', 'Core user management functionality', 'core', true, true, 'system', CURRENT_TIMESTAMP);

-- Sample Consumer Account
INSERT INTO consumer_accounts (consumer_account_id, account_name, account_code, contact_email, contact_name, organization_name, account_status, is_active, created_by, created_on)
VALUES ('cons-001', 'Demo Gym', 'DEMO_GYM', 'admin@demogym.com', 'John Smith', 'Demo Gym LLC', 'Active', true, 'system', CURRENT_TIMESTAMP);
*/

-- =============================================
-- VIEWS (Optional - for common queries)
-- =============================================

-- View for active licenses with product and consumer information
CREATE OR REPLACE VIEW active_licenses_view AS
SELECT 
    pl.product_license_id,
    pl.license_key,
    p.product_name,
    p.product_code,
    ca.account_name,
    ca.contact_email,
    pl.status,
    pl.valid_from,
    pl.valid_to,
    pl.max_users,
    pl.max_installations
FROM product_licenses pl
INNER JOIN products p ON pl.product_id = p.product_id
INNER JOIN consumer_accounts ca ON pl.consumer_id = ca.consumer_account_id
WHERE pl.status = 'Active' 
  AND pl.valid_to > CURRENT_TIMESTAMP
  AND p.is_active = true
  AND ca.is_active = true;

-- View for product feature summary
CREATE OR REPLACE VIEW product_features_summary AS
SELECT 
    p.product_id,
    p.product_name,
    p.product_code,
    COUNT(pf.product_feature_id) as total_features,
    COUNT(CASE WHEN pf.is_core_feature = true THEN 1 END) as core_features,
    COUNT(CASE WHEN pf.is_active = true THEN 1 END) as active_features
FROM products p
LEFT JOIN product_features pf ON p.product_id = pf.product_id
WHERE p.is_active = true
GROUP BY p.product_id, p.product_name, p.product_code;

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
COMMENT ON TABLE audit_entries IS 'Audit trail for all entity changes';
COMMENT ON TABLE notification_templates IS 'Templates for system notifications';
COMMENT ON TABLE notification_history IS 'History of sent notifications and delivery status';

-- =============================================
-- END OF SCRIPT
-- =============================================

-- Script execution completed successfully
-- Database schema created with:
-- - 10 main tables with proper relationships
-- - 2 many-to-many junction tables  
-- - Comprehensive indexes for performance
-- - Unique constraints for data integrity
-- - Audit trail support
-- - Optional views for common queries
-- - Sample data templates (commented out)

COMMIT;
