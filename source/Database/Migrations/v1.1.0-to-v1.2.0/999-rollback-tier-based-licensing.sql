-- =====================================================
-- TechWayFit Licensing Management System
-- Rollback Script: v1.2.0 to v1.1.0
-- 
-- Purpose: Rollback tier-based licensing changes
-- Author: System Migration
-- Date: 2025-08-05
-- =====================================================

-- Start transaction for atomic rollback
BEGIN;

-- =====================================================
-- 1. DROP VALIDATION FUNCTION
-- =====================================================

DROP FUNCTION IF EXISTS validate_tier_licensing_migration();

-- =====================================================
-- 2. DROP ENHANCED VIEW
-- =====================================================

DROP VIEW IF EXISTS v_enhanced_product_licenses;

-- =====================================================
-- 3. DROP INDEXES
-- =====================================================

DROP INDEX IF EXISTS idx_product_licenses_tier_id;
DROP INDEX IF EXISTS idx_product_licenses_version_range;
DROP INDEX IF EXISTS idx_product_licenses_product_tier;
DROP INDEX IF EXISTS idx_product_licenses_status_tier;

-- =====================================================
-- 4. DROP CHECK CONSTRAINTS
-- =====================================================

ALTER TABLE product_licenses 
DROP CONSTRAINT IF EXISTS chk_valid_product_version_from_format;

ALTER TABLE product_licenses 
DROP CONSTRAINT IF EXISTS chk_valid_product_version_to_format;

-- =====================================================
-- 5. DROP FOREIGN KEY CONSTRAINTS
-- =====================================================

ALTER TABLE product_licenses 
DROP CONSTRAINT IF EXISTS fk_product_licenses_product_tier_id;

-- =====================================================
-- 6. REMOVE NEW COLUMNS
-- =====================================================

-- Remove tier-based licensing column
ALTER TABLE product_licenses 
DROP COLUMN IF EXISTS product_tier_id;

-- Remove version range columns
ALTER TABLE product_licenses 
DROP COLUMN IF EXISTS valid_product_version_from;

ALTER TABLE product_licenses 
DROP COLUMN IF EXISTS valid_product_version_to;

-- =====================================================
-- 7. REMOVE COMMENTS
-- =====================================================

COMMENT ON TABLE product_tiers IS NULL;

-- =====================================================
-- 8. UPDATE STATISTICS
-- =====================================================

ANALYZE product_licenses;
ANALYZE product_tiers;

-- =====================================================
-- 9. COMMIT ROLLBACK
-- =====================================================

COMMIT;

-- =====================================================
-- 10. CONFIRMATION MESSAGE
-- =====================================================

DO $$ 
BEGIN 
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Tier-based licensing rollback completed!';
    RAISE NOTICE 'Schema version: 1.1.0';
    RAISE NOTICE 'Rollback date: %', NOW();
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Removed features:';
    RAISE NOTICE '- Tier-based licensing support';
    RAISE NOTICE '- Product version range validation';
    RAISE NOTICE '- Enhanced license view';
    RAISE NOTICE '- Performance indexes';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'WARNING: Any data in removed columns has been lost!';
    RAISE NOTICE '========================================';
END $$;
