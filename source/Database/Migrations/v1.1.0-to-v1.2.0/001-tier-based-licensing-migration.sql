-- =====================================================
-- TechWayFit Licensing Management System
-- Migration Script: v1.1.0 to v1.2.0
-- 
-- Purpose: Implement tier-based licensing support
-- Author: System Migration
-- Date: 2025-08-05
-- =====================================================

-- Start transaction for atomic migration
BEGIN;

-- =====================================================
-- 1. UPDATE PRODUCT_LICENSES TABLE FOR TIER-BASED LICENSING
-- =====================================================

-- Add new columns for tier-based licensing
ALTER TABLE product_licenses 
ADD COLUMN IF NOT EXISTS product_tier_id UUID NULL;

-- Add new columns for version range support
ALTER TABLE product_licenses 
ADD COLUMN IF NOT EXISTS valid_product_version_from VARCHAR(50) NOT NULL DEFAULT '1.0.0';

ALTER TABLE product_licenses 
ADD COLUMN IF NOT EXISTS valid_product_version_to VARCHAR(50) NULL;

-- Add comment for documentation
COMMENT ON COLUMN product_licenses.product_tier_id IS 'Foreign key to product_tiers table for tier-based licensing';
COMMENT ON COLUMN product_licenses.valid_product_version_from IS 'Minimum product version that this license supports';
COMMENT ON COLUMN product_licenses.valid_product_version_to IS 'Maximum product version that this license supports (optional)';

-- =====================================================
-- 2. ADD FOREIGN KEY CONSTRAINTS
-- =====================================================

-- Add foreign key constraint for product_tier_id
ALTER TABLE product_licenses 
ADD CONSTRAINT fk_product_licenses_product_tier_id 
FOREIGN KEY (product_tier_id) REFERENCES product_tiers(id) ON DELETE SET NULL;

-- =====================================================
-- 3. ADD NAVIGATION PROPERTY SUPPORT TO PRODUCT_TIERS
-- =====================================================

-- Add comment to document the reverse relationship
COMMENT ON TABLE product_tiers IS 'Product tiers with navigation to licenses that use this tier';

-- =====================================================
-- 4. CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Index for tier-based license queries
CREATE INDEX IF NOT EXISTS idx_product_licenses_tier_id 
ON product_licenses(product_tier_id) 
WHERE product_tier_id IS NOT NULL;

-- Index for version range queries
CREATE INDEX IF NOT EXISTS idx_product_licenses_version_range 
ON product_licenses(valid_product_version_from, valid_product_version_to);

-- Composite index for product and tier filtering
CREATE INDEX IF NOT EXISTS idx_product_licenses_product_tier 
ON product_licenses(product_id, product_tier_id) 
WHERE product_tier_id IS NOT NULL;

-- Index for license status and tier combination
CREATE INDEX IF NOT EXISTS idx_product_licenses_status_tier 
ON product_licenses(status, product_tier_id) 
WHERE product_tier_id IS NOT NULL;

-- =====================================================
-- 5. UPDATE EXISTING DATA (MIGRATION LOGIC)
-- =====================================================

-- Update existing licenses to use default version range if not specified
UPDATE product_licenses 
SET valid_product_version_from = '1.0.0' 
WHERE valid_product_version_from IS NULL OR valid_product_version_from = '';

-- Optional: Set product_tier_id for existing licenses based on business logic
-- This would need to be customized based on your specific requirements
-- Example: Assign all existing licenses to a "Legacy" tier
-- UPDATE product_licenses 
-- SET product_tier_id = (
--     SELECT id FROM product_tiers 
--     WHERE name = 'Legacy' 
--     LIMIT 1
-- ) 
-- WHERE product_tier_id IS NULL;

-- =====================================================
-- 6. ADD CHECK CONSTRAINTS FOR DATA VALIDATION
-- =====================================================

-- Ensure version format is valid (basic semantic version check)
ALTER TABLE product_licenses 
ADD CONSTRAINT chk_valid_product_version_from_format 
CHECK (valid_product_version_from ~ '^[0-9]+\.[0-9]+\.[0-9]+');

-- Ensure version to format is valid if specified
ALTER TABLE product_licenses 
ADD CONSTRAINT chk_valid_product_version_to_format 
CHECK (
    valid_product_version_to IS NULL OR 
    valid_product_version_to ~ '^[0-9]+\.[0-9]+\.[0-9]+'
);

-- =====================================================
-- 7. UPDATE STATISTICS FOR QUERY OPTIMIZATION
-- =====================================================

-- Analyze the updated table for better query planning
ANALYZE product_licenses;
ANALYZE product_tiers;

-- =====================================================
-- 8. CREATE VIEW FOR ENHANCED LICENSE INFORMATION
-- =====================================================

-- Create or replace view with tier information
CREATE OR REPLACE VIEW v_enhanced_product_licenses AS
SELECT 
    pl.id,
    pl.license_code,
    pl.product_id,
    pl.consumer_id,
    pl.product_tier_id,
    pt.name as tier_name,
    pt.description as tier_description,
    pl.valid_from,
    pl.valid_to,
    pl.valid_product_version_from,
    pl.valid_product_version_to,
    pl.status,
    pl.issued_by,
    pl.key_generated_at,
    pl.revoked_at,
    pl.revocation_reason,
    pl.created_on,
    pl.updated_on,
    -- Calculated fields
    CASE 
        WHEN pl.product_tier_id IS NOT NULL THEN 'Tier-Based'
        ELSE 'Legacy'
    END as license_type,
    CASE 
        WHEN pl.valid_to > NOW() AND pl.status = 'Active' THEN 'Active'
        WHEN pl.valid_to <= NOW() THEN 'Expired'
        ELSE pl.status
    END as effective_status
FROM product_licenses pl
LEFT JOIN product_tiers pt ON pl.product_tier_id = pt.id
WHERE pl.is_deleted = false;

-- Add comment for the view
COMMENT ON VIEW v_enhanced_product_licenses IS 'Enhanced view of product licenses with tier information and calculated fields';

-- =====================================================
-- 9. CREATE MIGRATION VALIDATION QUERIES
-- =====================================================

-- Function to validate migration success
CREATE OR REPLACE FUNCTION validate_tier_licensing_migration()
RETURNS TABLE (
    validation_check VARCHAR(50),
    passed BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if new columns exist
    RETURN QUERY
    SELECT 
        'product_tier_id_column'::VARCHAR(50),
        EXISTS(
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'product_licenses' 
            AND column_name = 'product_tier_id'
        ),
        'product_tier_id column exists in product_licenses table'::TEXT;

    -- Check if version columns exist
    RETURN QUERY
    SELECT 
        'version_columns'::VARCHAR(50),
        EXISTS(
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'product_licenses' 
            AND column_name = 'valid_product_version_from'
        ) AND EXISTS(
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'product_licenses' 
            AND column_name = 'valid_product_version_to'
        ),
        'Version range columns exist in product_licenses table'::TEXT;

    -- Check foreign key constraint
    RETURN QUERY
    SELECT 
        'foreign_key_constraint'::VARCHAR(50),
        EXISTS(
            SELECT 1 FROM information_schema.table_constraints 
            WHERE table_name = 'product_licenses' 
            AND constraint_name = 'fk_product_licenses_product_tier_id'
        ),
        'Foreign key constraint exists for product_tier_id'::TEXT;

    -- Check indexes
    RETURN QUERY
    SELECT 
        'tier_index'::VARCHAR(50),
        EXISTS(
            SELECT 1 FROM pg_indexes 
            WHERE tablename = 'product_licenses' 
            AND indexname = 'idx_product_licenses_tier_id'
        ),
        'Tier ID index exists'::TEXT;

    -- Check view exists
    RETURN QUERY
    SELECT 
        'enhanced_view'::VARCHAR(50),
        EXISTS(
            SELECT 1 FROM information_schema.views 
            WHERE table_name = 'v_enhanced_product_licenses'
        ),
        'Enhanced license view exists'::TEXT;

END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 10. COMMIT TRANSACTION
-- =====================================================

-- Run validation
SELECT * FROM validate_tier_licensing_migration();

-- Commit the migration
COMMIT;

-- =====================================================
-- 11. POST-MIGRATION NOTES
-- =====================================================

-- Print success message
DO $$ 
BEGIN 
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Tier-based licensing migration completed successfully!';
    RAISE NOTICE 'Schema version: 1.2.0';
    RAISE NOTICE 'Migration date: %', NOW();
    RAISE NOTICE '========================================';
    RAISE NOTICE 'New features available:';
    RAISE NOTICE '- Tier-based licensing support';
    RAISE NOTICE '- Product version range validation';
    RAISE NOTICE '- Enhanced license view';
    RAISE NOTICE '- Performance optimized indexes';
    RAISE NOTICE '========================================';
END $$;

-- =====================================================
-- 12. EXAMPLE QUERIES FOR TESTING
-- =====================================================

-- Example: Query licenses by tier
-- SELECT * FROM v_enhanced_product_licenses WHERE tier_name = 'Professional';

-- Example: Query licenses supporting specific product version
-- SELECT * FROM product_licenses 
-- WHERE '2.1.0' >= valid_product_version_from 
-- AND ('2.1.0' <= valid_product_version_to OR valid_product_version_to IS NULL);

-- Example: Count licenses by type
-- SELECT license_type, COUNT(*) FROM v_enhanced_product_licenses GROUP BY license_type;
