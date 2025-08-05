-- =====================================================
-- TechWayFit Licensing Management System
-- Migration Runner and Validator
-- 
-- Purpose: Execute and validate tier-based licensing migration
-- Author: System Migration
-- Date: 2025-08-05
-- =====================================================

-- Set session settings for migration
SET session_replication_role = replica;
SET lock_timeout = '30s';

-- =====================================================
-- 1. PRE-MIGRATION CHECKS
-- =====================================================

-- Check current schema version
DO $$
DECLARE
    table_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO table_count 
    FROM information_schema.tables 
    WHERE table_name IN ('product_licenses', 'product_tiers', 'product_features');
    
    IF table_count < 3 THEN
        RAISE EXCEPTION 'Required tables not found. Please ensure base schema is deployed.';
    END IF;
    
    RAISE NOTICE 'Pre-migration validation passed. Found % core tables.', table_count;
END $$;

-- Check for data that might be affected
SELECT 
    'product_licenses' as table_name,
    COUNT(*) as record_count,
    COUNT(CASE WHEN product_id IS NOT NULL THEN 1 END) as with_product_id
FROM product_licenses
UNION ALL
SELECT 
    'product_tiers' as table_name,
    COUNT(*) as record_count,
    COUNT(CASE WHEN product_id IS NOT NULL THEN 1 END) as with_product_id
FROM product_tiers;

-- =====================================================
-- 2. EXECUTE MIGRATION
-- =====================================================

\echo 'Starting tier-based licensing migration...'
\i 001-tier-based-licensing-migration.sql

-- =====================================================
-- 3. POST-MIGRATION VALIDATION
-- =====================================================

\echo 'Validating migration results...'

-- Check column existence
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'product_licenses' 
AND column_name IN ('product_tier_id', 'valid_product_version_from', 'valid_product_version_to')
ORDER BY column_name;

-- Check constraints
SELECT 
    constraint_name,
    constraint_type,
    table_name
FROM information_schema.table_constraints 
WHERE table_name = 'product_licenses' 
AND constraint_name LIKE '%tier%' OR constraint_name LIKE '%version%'
ORDER BY constraint_name;

-- Check indexes
SELECT 
    indexname,
    tablename,
    indexdef
FROM pg_indexes 
WHERE tablename = 'product_licenses' 
AND indexname LIKE '%tier%' OR indexname LIKE '%version%'
ORDER BY indexname;

-- Test the enhanced view
SELECT 
    license_type,
    COUNT(*) as count
FROM v_enhanced_product_licenses 
GROUP BY license_type;

-- =====================================================
-- 4. SAMPLE DATA VALIDATION
-- =====================================================

-- Test tier-based license creation (example)
DO $$
DECLARE
    sample_license_id UUID;
    sample_tier_id UUID;
BEGIN
    -- Get a sample tier ID
    SELECT id INTO sample_tier_id FROM product_tiers LIMIT 1;
    
    IF sample_tier_id IS NOT NULL THEN
        -- Test inserting a license with tier
        INSERT INTO product_licenses (
            id, license_code, product_id, consumer_id, product_tier_id,
            valid_from, valid_to, valid_product_version_from,
            status, issued_by, created_by, created_on
        ) VALUES (
            gen_random_uuid(),
            'TEST-TIER-' || substring(gen_random_uuid()::text, 1, 8),
            (SELECT id FROM products LIMIT 1),
            (SELECT id FROM consumer_accounts LIMIT 1),
            sample_tier_id,
            NOW(),
            NOW() + INTERVAL '1 year',
            '1.0.0',
            'Active',
            'migration-test',
            'migration-test',
            NOW()
        ) RETURNING id INTO sample_license_id;
        
        RAISE NOTICE 'Successfully created test license with tier: %', sample_license_id;
        
        -- Clean up test data
        DELETE FROM product_licenses WHERE id = sample_license_id;
        RAISE NOTICE 'Test license cleaned up successfully';
    ELSE
        RAISE NOTICE 'No product tiers found for testing';
    END IF;
END $$;

-- =====================================================
-- 5. PERFORMANCE TEST
-- =====================================================

-- Test query performance with new indexes
EXPLAIN (ANALYZE, BUFFERS) 
SELECT pl.*, pt.name as tier_name 
FROM product_licenses pl 
LEFT JOIN product_tiers pt ON pl.product_tier_id = pt.id 
WHERE pl.product_tier_id IS NOT NULL 
LIMIT 10;

-- =====================================================
-- 6. FINAL VALIDATION SUMMARY
-- =====================================================

\echo 'Running final validation...'
SELECT * FROM validate_tier_licensing_migration();

-- Reset session settings
SET session_replication_role = DEFAULT;

\echo 'Migration validation completed!'
\echo 'You can now update your application code to use tier-based licensing.'
