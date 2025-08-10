-- TechWayFit Licensing Management System - PostgreSQL Database Creation Script
-- Updated for Tier-Based Licensing Support
-- Compatible with PostgreSQL 12+
-- Version: 1.2.0-initial (Tier-Based Licensing)

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
-- LICENSE ENTITIES (Updated for Tier-Based Licensing)
-- =============================================

-- ProductLicenseEntity table with tier-based licensing support
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
-- (Kept for backward compatibility with direct feature associations)
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

-- UserAccountEntity table
CREATE TABLE user_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(100) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    password_salt VARCHAR(500) NOT NULL,
    phone VARCHAR(50),
    last_login TIMESTAMP WITH TIME ZONE,
    failed_login_attempts INTEGER DEFAULT 0,
    is_locked BOOLEAN NOT NULL DEFAULT false,
    locked_until TIMESTAMP WITH TIME ZONE,
    must_change_password BOOLEAN NOT NULL DEFAULT false,
    password_last_changed TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- UserRoleEntity table
CREATE TABLE user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    is_system_role BOOLEAN NOT NULL DEFAULT false,
    permissions_json VARCHAR(4000) DEFAULT '[]',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE
);

-- UserAccountRoleEntity table (Many-to-many relationship)
CREATE TABLE user_account_roles (
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    assigned_by VARCHAR(100) NOT NULL,
    assigned_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    PRIMARY KEY (user_id, role_id),
    
    -- Foreign key constraints
    CONSTRAINT fk_user_account_roles_user_id 
        FOREIGN KEY (user_id) REFERENCES user_accounts(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_account_roles_role_id 
        FOREIGN KEY (role_id) REFERENCES user_roles(id) ON DELETE CASCADE
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Product indexes
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_status ON products(status) WHERE is_deleted = false;
CREATE INDEX idx_products_release_date ON products(release_date);

-- Product version indexes
CREATE INDEX idx_product_versions_product_id ON product_versions(product_id);
CREATE INDEX idx_product_versions_version ON product_versions(version);
CREATE INDEX idx_product_versions_current ON product_versions(is_current) WHERE is_current = true;

-- Product tier indexes
CREATE INDEX idx_product_tiers_product_id ON product_tiers(product_id);
CREATE INDEX idx_product_tiers_display_order ON product_tiers(product_id, display_order);
CREATE INDEX idx_product_tiers_active ON product_tiers(product_id, is_active) WHERE is_active = true;

-- Product feature indexes
CREATE INDEX idx_product_features_tier_id ON product_features(tier_id);
CREATE INDEX idx_product_features_product_id ON product_features(product_id);
CREATE INDEX idx_product_features_code ON product_features(code);
CREATE INDEX idx_product_features_enabled ON product_features(tier_id, is_enabled) WHERE is_enabled = true;

-- Consumer account indexes
CREATE INDEX idx_consumer_accounts_company_name ON consumer_accounts(company_name);
CREATE INDEX idx_consumer_accounts_account_code ON consumer_accounts(account_code);
CREATE INDEX idx_consumer_accounts_primary_email ON consumer_accounts(primary_contact_email);
CREATE INDEX idx_consumer_accounts_status ON consumer_accounts(status) WHERE is_deleted = false;

-- Product license indexes (Enhanced for tier-based licensing)
CREATE INDEX idx_product_licenses_product_id ON product_licenses(product_id);
CREATE INDEX idx_product_licenses_consumer_id ON product_licenses(consumer_id);
CREATE INDEX idx_product_licenses_license_code ON product_licenses(license_code);
CREATE INDEX idx_product_licenses_status ON product_licenses(status) WHERE is_deleted = false;
CREATE INDEX idx_product_licenses_valid_dates ON product_licenses(valid_from, valid_to);
-- NEW: Tier-based licensing indexes
CREATE INDEX idx_product_licenses_tier_id ON product_licenses(product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_product_tier ON product_licenses(product_id, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_status_tier ON product_licenses(status, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_version_range ON product_licenses(valid_product_version_from, valid_product_version_to);

-- User account indexes
CREATE INDEX idx_user_accounts_username ON user_accounts(username);
CREATE INDEX idx_user_accounts_email ON user_accounts(email);
CREATE INDEX idx_user_accounts_active ON user_accounts(is_active) WHERE is_deleted = false;
CREATE INDEX idx_user_accounts_last_login ON user_accounts(last_login);

-- Audit indexes
CREATE INDEX idx_audit_entries_entity ON audit_entries(entity_type, entity_id);
CREATE INDEX idx_audit_entries_created_on ON audit_entries(created_on);
CREATE INDEX idx_audit_entries_action_type ON audit_entries(action_type);

-- Notification indexes
CREATE INDEX idx_notification_history_entity ON notification_history(entity_type, entity_id);
CREATE INDEX idx_notification_history_sent_date ON notification_history(sent_date);
CREATE INDEX idx_notification_history_delivery_status ON notification_history(delivery_status);

-- =============================================
-- VIEWS FOR ENHANCED QUERIES
-- =============================================

-- Enhanced Product Licenses View with Tier Information
CREATE OR REPLACE VIEW v_enhanced_product_licenses AS
SELECT 
    pl.id AS license_id,
    pl.license_code,
    pl.product_id,
    p.name AS product_name,
    pl.consumer_id,
    ca.company_name AS consumer_name,
    pl.product_tier_id,
    pt.name AS tier_name,
    pl.valid_from,
    pl.valid_to,
    pl.valid_product_version_from,
    pl.valid_product_version_to,
    pl.status,
    pl.issued_by,
    pl.created_on,
    -- License type classification
    CASE 
        WHEN pl.product_tier_id IS NOT NULL THEN 'Tier-Based'
        ELSE 'Feature-Based'
    END AS license_type,
    -- Tier features (for tier-based licenses)
    (
        SELECT COUNT(*) 
        FROM product_features pf 
        WHERE pf.tier_id = pl.product_tier_id 
        AND pf.is_enabled = true 
        AND pf.is_deleted = false
    ) AS tier_feature_count,
    -- Direct features (for backward compatibility)
    (
        SELECT COUNT(*) 
        FROM product_license_features plf 
        WHERE plf.license_id = pl.id
    ) AS direct_feature_count
FROM product_licenses pl
INNER JOIN products p ON pl.product_id = p.id
INNER JOIN consumer_accounts ca ON pl.consumer_id = ca.id
LEFT JOIN product_tiers pt ON pl.product_tier_id = pt.id
WHERE pl.is_deleted = false;

-- Product Tier Features Summary View
CREATE OR REPLACE VIEW v_product_tier_features AS
SELECT 
    pt.id AS tier_id,
    pt.product_id,
    p.name AS product_name,
    pt.name AS tier_name,
    pt.description AS tier_description,
    COUNT(pf.id) AS feature_count,
    COUNT(CASE WHEN pf.is_enabled = true THEN 1 END) AS enabled_feature_count,
    string_agg(
        CASE WHEN pf.is_enabled = true THEN pf.name END, 
        ', ' ORDER BY pf.display_order
    ) AS enabled_features
FROM product_tiers pt
INNER JOIN products p ON pt.product_id = p.id
LEFT JOIN product_features pf ON pt.id = pf.tier_id AND pf.is_deleted = false
WHERE pt.is_deleted = false
GROUP BY pt.id, pt.product_id, p.name, pt.name, pt.description
ORDER BY pt.product_id, pt.display_order;

-- =============================================
-- FUNCTIONS FOR VALIDATION
-- =============================================

-- Function to validate tier-based licensing migration
CREATE OR REPLACE FUNCTION validate_tier_licensing_migration()
RETURNS TABLE (
    validation_check VARCHAR(50),
    status VARCHAR(10),
    details TEXT
) AS $$
BEGIN
    -- Check if product_tier_id column exists
    RETURN QUERY
    SELECT 
        'product_tier_id_column'::VARCHAR(50),
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = 'product_licenses' 
                AND column_name = 'product_tier_id'
            ) THEN 'PASS'::VARCHAR(10)
            ELSE 'FAIL'::VARCHAR(10)
        END,
        'product_tier_id column exists in product_licenses table'::TEXT;
    
    -- Check if version range columns exist
    RETURN QUERY
    SELECT 
        'version_range_columns'::VARCHAR(50),
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = 'product_licenses' 
                AND column_name IN ('valid_product_version_from', 'valid_product_version_to')
            ) THEN 'PASS'::VARCHAR(10)
            ELSE 'FAIL'::VARCHAR(10)
        END,
        'Version range columns exist in product_licenses table'::TEXT;
    
    -- Check foreign key constraint
    RETURN QUERY
    SELECT 
        'tier_foreign_key'::VARCHAR(50),
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM information_schema.table_constraints 
                WHERE constraint_name = 'fk_product_licenses_product_tier_id'
            ) THEN 'PASS'::VARCHAR(10)
            ELSE 'FAIL'::VARCHAR(10)
        END,
        'Foreign key constraint exists for product_tier_id'::TEXT;
    
    -- Check tier-based licenses
    RETURN QUERY
    SELECT 
        'tier_based_licenses'::VARCHAR(50),
        'INFO'::VARCHAR(10),
        'Total tier-based licenses: ' || COUNT(*)::TEXT
    FROM product_licenses 
    WHERE product_tier_id IS NOT NULL;
    
    -- Check enhanced view
    RETURN QUERY
    SELECT 
        'enhanced_view'::VARCHAR(50),
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM information_schema.views 
                WHERE table_name = 'v_enhanced_product_licenses'
            ) THEN 'PASS'::VARCHAR(10)
            ELSE 'FAIL'::VARCHAR(10)
        END,
        'Enhanced product licenses view exists'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- COMMENTS FOR DOCUMENTATION
-- =============================================

-- Table comments
COMMENT ON TABLE product_licenses IS 'Product licenses with support for both tier-based and feature-based licensing models';
COMMENT ON TABLE product_tiers IS 'Product tiers defining feature sets and pricing levels';
COMMENT ON TABLE product_features IS 'Features associated with product tiers';

-- Column comments for new tier-based licensing columns
COMMENT ON COLUMN product_licenses.product_tier_id IS 'Foreign key to product_tiers table for tier-based licensing (NULL for feature-based licenses)';
COMMENT ON COLUMN product_licenses.valid_product_version_from IS 'Minimum product version that this license supports';
COMMENT ON COLUMN product_licenses.valid_product_version_to IS 'Maximum product version that this license supports (NULL means no upper limit)';

-- View comments
COMMENT ON VIEW v_enhanced_product_licenses IS 'Enhanced view of product licenses with tier information and license type classification';
COMMENT ON VIEW v_product_tier_features IS 'Summary view of product tiers and their associated features';

-- =============================================
-- INITIAL DATA SETUP
-- =============================================

-- Insert default system user role
INSERT INTO user_roles (id, role_name, description, is_system_role, permissions_json, created_by)
VALUES (
    uuid_generate_v4(),
    'System Administrator',
    'Full system access for administrators',
    true,
    '["ALL_PERMISSIONS"]',
    'system'
) ON CONFLICT (role_name) DO NOTHING;

-- Insert default product tier (for backward compatibility)
INSERT INTO products (id, name, description, created_by)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Default Product',
    'Default product for licensing system',
    'system'
) ON CONFLICT (id) DO NOTHING;

INSERT INTO product_tiers (id, product_id, name, description, display_order, created_by)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'Community',
    'Basic community tier with essential features',
    1,
    'system'
) ON CONFLICT (id) DO NOTHING;

-- =============================================
-- SCHEMA VERSION TRACKING
-- =============================================

CREATE TABLE IF NOT EXISTS schema_versions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    version VARCHAR(20) NOT NULL UNIQUE,
    description VARCHAR(500),
    applied_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    applied_by VARCHAR(100) NOT NULL
);

-- Record this schema version
INSERT INTO schema_versions (version, description, applied_by)
VALUES ('1.2.0', 'Initial schema with tier-based licensing support', 'system')
ON CONFLICT (version) DO NOTHING;

-- =============================================
-- COMPLETION NOTICE
-- =============================================

DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'TechWayFit Licensing Management Database';
    RAISE NOTICE 'Schema Version: 1.2.0 (Tier-Based Licensing)';
    RAISE NOTICE 'Applied on: %', CURRENT_TIMESTAMP;
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Schema created successfully with tier-based licensing support!';
    RAISE NOTICE 'Key features:';
    RAISE NOTICE '- Product tier-based licensing';
    RAISE NOTICE '- Version range support for licenses';
    RAISE NOTICE '- Backward compatibility with feature-based licensing';
    RAISE NOTICE '- Enhanced views and indexes for performance';
    RAISE NOTICE '- Built-in validation functions';
    RAISE NOTICE '========================================';
END $$;
