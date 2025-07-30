-- TechWayFit Licensing Management System - Database Permissions Script
-- Version: 1.0.0-corrected
-- This script grants necessary permissions to the application user
-- Aligned with v1.0.0-initial-schema-corrected.sql
-- Run this as a PostgreSQL superuser or database owner

-- =============================================
-- DATABASE CONNECTION
-- =============================================
-- Connect to the correct database


-- =============================================
-- USER CREATION (run only if user doesn't exist)
-- =============================================
-- Create application user if it doesn't exist
-- DO $$ 
-- BEGIN
--     IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'twf_license_user') THEN
--         CREATE USER twf_license_user WITH PASSWORD 'your_secure_password_here';
--     END IF;
-- END
-- $$;

-- =============================================
-- SCHEMA PERMISSIONS
-- =============================================
-- Grant usage on public schema
GRANT USAGE ON SCHEMA public TO twf_license_user;

-- Grant connect permission to database
GRANT CONNECT ON DATABASE twf_license_management TO twf_license_user;

-- =============================================
-- TABLE PERMISSIONS - ALL CURRENT TABLES
-- =============================================
-- Grant permissions on all existing tables
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO twf_license_user;

-- Grant permissions on sequences (for auto-increment columns if any)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO twf_license_user;

-- Grant permissions on future tables (in case new tables are created)
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO twf_license_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO twf_license_user;

-- =============================================
-- SPECIFIC TABLE PERMISSIONS (Explicit grants)
-- =============================================
-- Core Product Tables
GRANT ALL PRIVILEGES ON TABLE products TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_versions TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_tiers TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_features TO twf_license_user;

-- Consumer Tables
GRANT ALL PRIVILEGES ON TABLE consumer_accounts TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_consumers TO twf_license_user;

-- License Tables
GRANT ALL PRIVILEGES ON TABLE product_licenses TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_license_features TO twf_license_user;

-- System Tables
GRANT ALL PRIVILEGES ON TABLE audit_entries TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE notification_templates TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE notification_history TO twf_license_user;

GRANT ALL PRIVILEGES ON TABLE settings TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE user_roles TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE user_profiles TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE user_role_mappings TO twf_license_user;
-- =============================================
-- VIEW PERMISSIONS
-- =============================================
-- Grant permissions on views
GRANT SELECT ON active_licenses_view TO twf_license_user;
GRANT SELECT ON product_features_summary TO twf_license_user;

-- =============================================
-- FUNCTION PERMISSIONS (if any custom functions exist)
-- =============================================
-- Grant execute permissions on functions
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO twf_license_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO twf_license_user;

-- =============================================
-- VALIDATION QUERIES
-- =============================================
-- Check table permissions (uncomment to verify)
-- SELECT 
--     schemaname,
--     tablename,
--     tableowner,
--     hasinserts,
--     hasselects,
--     hasupdates,
--     hasdeletes
-- FROM pg_tables 
-- WHERE schemaname = 'public'
-- ORDER BY tablename;

-- Check user privileges (uncomment to verify)
-- SELECT 
--     table_schema,
--     table_name,
--     privilege_type,
--     is_grantable
-- FROM information_schema.table_privileges 
-- WHERE grantee = 'twf_license_user'
-- ORDER BY table_name, privilege_type;

-- Check view permissions (uncomment to verify)
-- SELECT 
--     table_schema,
--     table_name,
--     privilege_type
-- FROM information_schema.table_privileges 
-- WHERE grantee = 'twf_license_user' 
--   AND table_name LIKE '%_view' OR table_name LIKE '%_summary'
-- ORDER BY table_name;

-- =============================================
-- COMMIT TRANSACTION
-- =============================================
COMMIT;

-- =============================================
-- USAGE INSTRUCTIONS
-- =============================================
-- To run this script:
-- 1. Connect as PostgreSQL superuser or database owner:
--    psql -h localhost -p 5432 -U postgres -d techwayfit_licensing
--
-- 2. Run this script:
--    \i /path/to/fix-permissions.sql
--
-- 3. Or execute manually by copying the GRANT statements above
--
-- 4. Test connection with application user:
--    psql -h localhost -p 5432 -U twf_license_user -d techwayfit_licensing
--
-- 5. Verify permissions with the validation queries above

-- =============================================
-- SECURITY NOTES
-- =============================================
-- 1. Change the default password for twf_license_user
-- 2. Use environment variables for connection strings in applications
-- 3. Consider using connection pooling for production deployments
-- 4. Regularly audit user permissions and access logs
-- 5. Use SSL connections in production environments

-- =============================================
-- TABLE LIST (for reference)
-- =============================================
-- The following 11 tables are covered by this script:
-- 1. products - Core product definitions
-- 2. product_versions - Product version tracking
-- 3. product_tiers - Product pricing tiers
-- 4. product_features - Product feature definitions
-- 5. consumer_accounts - Customer account information
-- 6. product_consumers - Product-consumer relationships
-- 7. product_licenses - License instances and keys
-- 8. product_license_features - License-feature many-to-many junction
-- 9. audit_entries - System audit trail
-- 10. notification_templates - Notification message templates
-- 11. notification_history - Notification delivery history
--
-- Views:
-- - active_licenses_view - Active licenses with product/consumer info
-- - product_features_summary - Feature count summary by product/tier
