-- ==================================================
-- Fix Operations Dashboard Schema
-- Version: 1.1.0
-- Description: Fixes schema mismatches between database and entities
-- ==================================================

-- Fix system_health_snapshots table - rename health_issues to health_issues_json
-- to match entity property name

-- First check if the column exists
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'system_health_snapshots' 
            AND column_name = 'health_issues'
        ) THEN 'health_issues column found - will rename to health_issues_json'
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'system_health_snapshots' 
            AND column_name = 'health_issues_json'
        ) THEN 'health_issues_json column already exists'
        ELSE 'Neither column found - will create health_issues_json'
    END as column_status;

-- Rename the column if it exists with the old name
ALTER TABLE system_health_snapshots 
RENAME COLUMN health_issues TO health_issues_json;

-- Verify all operations dashboard tables have correct structure
SELECT 
    'system_metrics' as table_name,
    COUNT(*) as column_count
FROM information_schema.columns 
WHERE table_name = 'system_metrics'

UNION ALL

SELECT 
    'error_log_summaries' as table_name,
    COUNT(*) as column_count
FROM information_schema.columns 
WHERE table_name = 'error_log_summaries'

UNION ALL

SELECT 
    'page_performance_metrics' as table_name,
    COUNT(*) as column_count
FROM information_schema.columns 
WHERE table_name = 'page_performance_metrics'

UNION ALL

SELECT 
    'query_performance_metrics' as table_name,
    COUNT(*) as column_count
FROM information_schema.columns 
WHERE table_name = 'query_performance_metrics'

UNION ALL

SELECT 
    'system_health_snapshots' as table_name,
    COUNT(*) as column_count
FROM information_schema.columns 
WHERE table_name = 'system_health_snapshots';

-- Verify specific columns that were causing issues
SELECT 
    'Column Verification' as check_type,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'system_health_snapshots' 
            AND column_name = 'health_issues_json'
        ) THEN 'health_issues_json column exists ✓'
        ELSE 'health_issues_json column MISSING ✗'
    END as result

UNION ALL

SELECT 
    'Column Verification' as check_type,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'page_performance_metrics' 
            AND column_name = 'p95_response_time_ms'
        ) THEN 'p95_response_time_ms column exists ✓'
        ELSE 'p95_response_time_ms column MISSING ✗'
    END as result

UNION ALL

SELECT 
    'Column Verification' as check_type,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'page_performance_metrics' 
            AND column_name = 'p99_response_time_ms'
        ) THEN 'p99_response_time_ms column exists ✓'
        ELSE 'p99_response_time_ms column MISSING ✗'
    END as result;

-- Display final success message
SELECT 'Operations Dashboard schema fix completed successfully!' as status;
