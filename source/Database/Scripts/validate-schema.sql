-- TechWayFit Licensing Database Schema Validation Script
-- This script validates that the database schema is correctly deployed
-- Run this after any deployment or migration to ensure schema integrity

-- Enable timing and feedback
\timing on
\echo 'Starting schema validation for TechWayFit Licensing Database...'

-- Create validation results table (temporary)
CREATE TEMP TABLE validation_results (
    check_name VARCHAR(100),
    status VARCHAR(20),
    details TEXT,
    checked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Helper function to record validation results
CREATE OR REPLACE FUNCTION record_validation(
    p_check_name VARCHAR(100),
    p_status VARCHAR(20),
    p_details TEXT DEFAULT ''
) RETURNS VOID AS $$
BEGIN
    INSERT INTO validation_results (check_name, status, details)
    VALUES (p_check_name, p_status, p_details);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- TABLE EXISTENCE CHECKS
-- =============================================

\echo 'Checking table existence...'

DO $$
DECLARE
    required_tables TEXT[] := ARRAY[
        'products',
        'product_versions', 
        'product_tiers',
        'product_features',
        'consumer_accounts',
        'product_consumers',
        'product_licenses',
        'product_license_features',
        'product_tier_features',
        'audit_entries',
        'notification_templates',
        'notification_history'
    ];
    table_name TEXT;
    table_count INTEGER;
BEGIN
    FOREACH table_name IN ARRAY required_tables
    LOOP
        SELECT COUNT(*) INTO table_count
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = table_name;
        
        IF table_count = 1 THEN
            PERFORM record_validation('Table: ' || table_name, 'PASS', 'Table exists');
        ELSE
            PERFORM record_validation('Table: ' || table_name, 'FAIL', 'Table missing');
        END IF;
    END LOOP;
END;
$$;

-- =============================================
-- FOREIGN KEY CONSTRAINT CHECKS
-- =============================================

\echo 'Checking foreign key constraints...'

DO $$
DECLARE
    required_fks TEXT[] := ARRAY[
        'fk_product_versions_product_id',
        'fk_product_tiers_product_id',
        'fk_product_features_product_id',
        'fk_product_consumers_product_id',
        'fk_product_consumers_consumer_account_id',
        'fk_product_licenses_product_id',
        'fk_product_licenses_consumer_id',
        'fk_license_features_license_id',
        'fk_license_features_feature_id',
        'fk_tier_features_tier_id',
        'fk_tier_features_feature_id',
        'fk_notification_history_template_id'
    ];
    fk_name TEXT;
    fk_count INTEGER;
BEGIN
    FOREACH fk_name IN ARRAY required_fks
    LOOP
        SELECT COUNT(*) INTO fk_count
        FROM information_schema.table_constraints 
        WHERE constraint_schema = 'public' 
        AND constraint_name = fk_name
        AND constraint_type = 'FOREIGN KEY';
        
        IF fk_count = 1 THEN
            PERFORM record_validation('FK: ' || fk_name, 'PASS', 'Foreign key exists');
        ELSE
            PERFORM record_validation('FK: ' || fk_name, 'FAIL', 'Foreign key missing');
        END IF;
    END LOOP;
END;
$$;

-- =============================================
-- UNIQUE CONSTRAINT CHECKS
-- =============================================

\echo 'Checking unique constraints...'

DO $$
DECLARE
    required_uks TEXT[] := ARRAY[
        'uk_products_product_code',
        'uk_product_versions_product_version',
        'uk_product_tiers_product_tier',
        'uk_product_features_product_feature',
        'uk_consumer_accounts_account_code',
        'uk_product_consumers_product_consumer',
        'idx_product_licenses_license_key'  -- This is actually a unique index
    ];
    uk_name TEXT;
    uk_count INTEGER;
BEGIN
    FOREACH uk_name IN ARRAY required_uks
    LOOP
        -- Check for unique constraints
        SELECT COUNT(*) INTO uk_count
        FROM information_schema.table_constraints 
        WHERE constraint_schema = 'public' 
        AND constraint_name = uk_name
        AND constraint_type = 'UNIQUE';
        
        -- If not found as constraint, check as unique index
        IF uk_count = 0 THEN
            SELECT COUNT(*) INTO uk_count
            FROM pg_indexes 
            WHERE schemaname = 'public' 
            AND indexname = uk_name
            AND indexdef LIKE '%UNIQUE%';
        END IF;
        
        IF uk_count = 1 THEN
            PERFORM record_validation('UK: ' || uk_name, 'PASS', 'Unique constraint/index exists');
        ELSE
            PERFORM record_validation('UK: ' || uk_name, 'FAIL', 'Unique constraint/index missing');
        END IF;
    END LOOP;
END;
$$;

-- =============================================
-- INDEX CHECKS
-- =============================================

\echo 'Checking performance indexes...'

DO $$
DECLARE
    required_indexes TEXT[] := ARRAY[
        'idx_products_product_code',
        'idx_products_is_active',
        'idx_product_versions_product_id',
        'idx_product_tiers_product_id',
        'idx_product_features_product_id',
        'idx_consumer_accounts_account_code',
        'idx_consumer_accounts_contact_email',
        'idx_product_consumers_product_id',
        'idx_product_consumers_consumer_account_id',
        'idx_product_licenses_product_id',
        'idx_product_licenses_consumer_id',
        'idx_product_licenses_status',
        'idx_audit_entries_entity_type',
        'idx_notification_templates_template_name',
        'idx_notification_history_template_id'
    ];
    index_name TEXT;
    index_count INTEGER;
BEGIN
    FOREACH index_name IN ARRAY required_indexes
    LOOP
        SELECT COUNT(*) INTO index_count
        FROM pg_indexes 
        WHERE schemaname = 'public' 
        AND indexname = index_name;
        
        IF index_count = 1 THEN
            PERFORM record_validation('Index: ' || index_name, 'PASS', 'Index exists');
        ELSE
            PERFORM record_validation('Index: ' || index_name, 'FAIL', 'Index missing');
        END IF;
    END LOOP;
END;
$$;

-- =============================================
-- COLUMN TYPE CHECKS
-- =============================================

\echo 'Checking critical column types and constraints...'

DO $$
DECLARE
    validation_query TEXT;
    validation_result BOOLEAN;
BEGIN
    -- Check that audit fields exist on key tables
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'products' 
        AND column_name IN ('created_by', 'created_on', 'updated_by', 'updated_on')
        HAVING COUNT(*) = 4
    ) INTO validation_result;
    
    IF validation_result THEN
        PERFORM record_validation('Audit Fields', 'PASS', 'Audit fields present on products table');
    ELSE
        PERFORM record_validation('Audit Fields', 'FAIL', 'Missing audit fields on products table');
    END IF;
    
    -- Check primary key constraints
    SELECT EXISTS (
        SELECT 1 FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu 
        ON tc.constraint_name = kcu.constraint_name
        WHERE tc.constraint_type = 'PRIMARY KEY'
        AND tc.table_schema = 'public'
        AND tc.table_name = 'products'
        AND kcu.column_name = 'product_id'
    ) INTO validation_result;
    
    IF validation_result THEN
        PERFORM record_validation('Primary Keys', 'PASS', 'Products table has correct primary key');
    ELSE
        PERFORM record_validation('Primary Keys', 'FAIL', 'Products table missing primary key');
    END IF;
END;
$$;

-- =============================================
-- VIEW CHECKS
-- =============================================

\echo 'Checking views...'

DO $$
DECLARE
    required_views TEXT[] := ARRAY[
        'active_licenses_view',
        'product_features_summary'
    ];
    view_name TEXT;
    view_count INTEGER;
BEGIN
    FOREACH view_name IN ARRAY required_views
    LOOP
        SELECT COUNT(*) INTO view_count
        FROM information_schema.views 
        WHERE table_schema = 'public' 
        AND table_name = view_name;
        
        IF view_count = 1 THEN
            PERFORM record_validation('View: ' || view_name, 'PASS', 'View exists');
        ELSE
            PERFORM record_validation('View: ' || view_name, 'FAIL', 'View missing');
        END IF;
    END LOOP;
END;
$$;

-- =============================================
-- DATABASE CONNECTIVITY TESTS
-- =============================================

\echo 'Testing database functionality...'

DO $$
DECLARE
    test_result TEXT;
BEGIN
    -- Test basic insert/select/delete functionality
    BEGIN
        -- Test insert
        INSERT INTO products (product_id, product_name, product_code, created_by, created_on)
        VALUES ('test-validation-001', 'Validation Test Product', 'VAL-TEST', 'validation_script', CURRENT_TIMESTAMP);
        
        -- Test select
        SELECT product_name INTO test_result 
        FROM products 
        WHERE product_id = 'test-validation-001';
        
        -- Test delete
        DELETE FROM products WHERE product_id = 'test-validation-001';
        
        PERFORM record_validation('CRUD Operations', 'PASS', 'Basic CRUD operations working');
        
    EXCEPTION WHEN OTHERS THEN
        PERFORM record_validation('CRUD Operations', 'FAIL', 'CRUD operations failed: ' || SQLERRM);
    END;
END;
$$;

-- =============================================
-- VALIDATION RESULTS SUMMARY
-- =============================================

\echo 'Validation completed. Results summary:'

-- Display results
SELECT 
    status,
    COUNT(*) as count,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM validation_results), 2) as percentage
FROM validation_results 
GROUP BY status 
ORDER BY status;

\echo ''
\echo 'Detailed results:'

SELECT 
    check_name,
    status,
    details,
    checked_at
FROM validation_results 
ORDER BY 
    CASE status 
        WHEN 'FAIL' THEN 1 
        WHEN 'PASS' THEN 2 
        ELSE 3 
    END,
    check_name;

-- Check for any failures
DO $$
DECLARE
    failure_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO failure_count 
    FROM validation_results 
    WHERE status = 'FAIL';
    
    IF failure_count > 0 THEN
        RAISE NOTICE 'VALIDATION FAILED: % check(s) failed. Please review the results above.', failure_count;
    ELSE
        RAISE NOTICE 'VALIDATION PASSED: All schema checks completed successfully!';
    END IF;
END;
$$;

-- Cleanup
DROP FUNCTION record_validation(VARCHAR(100), VARCHAR(20), TEXT);

\echo 'Schema validation completed.'
\timing off
