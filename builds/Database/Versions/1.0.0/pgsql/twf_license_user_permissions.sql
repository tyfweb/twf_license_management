-- TechWayFit Licensing Management System - PostgreSQL User Permissions Script
-- Creates database user and grants appropriate permissions
-- Compatible with PostgreSQL 12+
-- Version: 1.0.0-initial

-- =============================================
-- USER CREATION AND SETUP
-- =============================================

-- Create the application user (if it doesn't exist)
-- Note: Replace 'your_secure_password' with a strong password
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'twf_license_user') THEN
        CREATE USER twf_license_user WITH PASSWORD 'M@n@s0000';
        RAISE NOTICE 'User twf_license_user created successfully';
    ELSE
        RAISE NOTICE 'User twf_license_user already exists';
    END IF;
END
$$;

-- =============================================
-- DATABASE PERMISSIONS
-- =============================================

-- Grant connection permissions to the database
-- Note: Replace 'twf_license_management' with your actual database name
GRANT CONNECT ON DATABASE twf_license_management TO twf_license_user;

-- Grant usage on the public schema
GRANT USAGE ON SCHEMA public TO twf_license_user;

-- =============================================
-- TABLE PERMISSIONS
-- =============================================

-- Grant full permissions on all tables for the application user
-- Core Product Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON products TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON product_versions TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON product_tiers TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON product_features TO twf_license_user;

-- Consumer Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON consumer_accounts TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON product_consumers TO twf_license_user;

-- License Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON product_licenses TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON product_license_features TO twf_license_user;

-- Audit Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON audit_entries TO twf_license_user;

-- Notification Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON notification_templates TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON notification_history TO twf_license_user;

-- User Management Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON user_profiles TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON user_roles TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON user_role_mappings TO twf_license_user;

-- Settings Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON settings TO twf_license_user;

-- Operations Dashboard Tables
GRANT SELECT, INSERT, UPDATE, DELETE ON system_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON system_health_snapshots TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON page_performance_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON query_performance_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON error_log_summaries TO twf_license_user;

-- =============================================
-- SEQUENCE PERMISSIONS (for UUID generation)
-- =============================================

-- Grant usage on all sequences (needed for auto-increment fields if any)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO twf_license_user;

-- Grant permissions on future sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO twf_license_user;

-- =============================================
-- VIEW PERMISSIONS
-- =============================================

-- Grant permissions on views
GRANT SELECT ON active_licenses_view TO twf_license_user;
GRANT SELECT ON product_features_summary TO twf_license_user;
GRANT SELECT ON v_user_details TO twf_license_user;
GRANT SELECT ON v_system_health_summary TO twf_license_user;

-- =============================================
-- FUTURE OBJECTS PERMISSIONS
-- =============================================

-- Grant permissions on future tables created in the public schema
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO twf_license_user;

-- Grant permissions on future views
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO twf_license_user;

-- =============================================
-- FUNCTION PERMISSIONS (if needed)
-- =============================================

-- Grant execute permissions on functions (if any are created later)
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO twf_license_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO twf_license_user;

-- =============================================
-- SECURITY SETTINGS
-- =============================================

-- Set connection limit (optional - adjust as needed)
ALTER USER twf_license_user CONNECTION LIMIT 50;

-- Set statement timeout (optional - 30 seconds)
ALTER USER twf_license_user SET statement_timeout = '30s';

-- Set idle timeout (optional - 5 minutes)
ALTER USER twf_license_user SET idle_in_transaction_session_timeout = '5min';

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Query to verify user permissions (run as superuser)
/*
SELECT 
    grantee,
    table_schema,
    table_name,
    privilege_type
FROM information_schema.table_privileges
WHERE grantee = 'twf_license_user'
ORDER BY table_name, privilege_type;
*/

-- Query to verify user exists and settings
/*
SELECT 
    rolname,
    rolconnlimit,
    rolcanlogin,
    rolcreatedb,
    rolcreaterole,
    rolsuper
FROM pg_roles 
WHERE rolname = 'twf_license_user';
*/

-- =============================================
-- ADDITIONAL SECURITY RECOMMENDATIONS
-- =============================================

/*
SECURITY BEST PRACTICES:

1. PASSWORD MANAGEMENT:
   - Replace 'your_secure_password' with a strong, unique password
   - Consider using environment variables or Azure Key Vault for password storage
   - Rotate passwords regularly

2. CONNECTION SECURITY:
   - Use SSL/TLS connections in production
   - Configure pg_hba.conf to restrict connections by IP/subnet
   - Consider using certificate-based authentication

3. PRINCIPLE OF LEAST PRIVILEGE:
   - Review and adjust permissions based on actual application needs
   - Consider creating separate users for read-only operations
   - Monitor user activity through PostgreSQL logs

4. DATABASE SECURITY:
   - Enable row-level security (RLS) if needed for multi-tenant scenarios
   - Use database auditing for sensitive operations
   - Regular security updates and patches

5. APPLICATION SECURITY:
   - Use connection pooling (like PgBouncer)
   - Implement proper error handling to avoid information disclosure
   - Use parameterized queries to prevent SQL injection
*/

-- =============================================
-- CLEANUP COMMANDS (if needed)
-- =============================================

/*
-- To remove user and permissions (USE WITH CAUTION):
-- REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM twf_license_user;
-- REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public FROM twf_license_user;
-- REVOKE ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public FROM twf_license_user;
-- REVOKE CONNECT ON DATABASE twf_license_management FROM twf_license_user;
-- DROP USER IF EXISTS twf_license_user;
*/

-- =============================================
-- END OF SCRIPT
-- =============================================

-- Script execution completed successfully
-- User 'twf_license_user' configured with:
-- - Full CRUD permissions on all application tables
-- - Read permissions on all views
-- - Execute permissions on functions
-- - Appropriate security settings and timeouts
-- - Connection limit and session management

COMMIT;

-- IMPORTANT NOTES:
-- 1. Update the password before running this script
-- 2. Replace 'twf_licensing_db' with your actual database name
-- 3. Run this script as a PostgreSQL superuser or database owner
-- 4. Test the permissions after running the script
-- 5. Consider additional security measures based on your environment
