-- TechWayFit Licensing Management System - Permission Fix Script
-- This script grants necessary permissions to the application user
-- Run this as a PostgreSQL superuser or database owner

-- Connect to the correct database
\c techwayfit_licensing;

-- Grant usage on schema
GRANT USAGE ON SCHEMA public TO twf_license_user;

-- Grant permissions on all existing tables
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO twf_license_user;

-- Grant permissions on sequences (for auto-increment columns)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO twf_license_user;

-- Grant permissions on future tables (in case new tables are created)
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO twf_license_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO twf_license_user;

-- Specific table permissions (ensure all tables are covered)
GRANT ALL PRIVILEGES ON TABLE products TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_versions TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_tiers TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_features TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE consumer_accounts TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_consumers TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_licenses TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE product_license_features TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE audit_entries TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE notification_templates TO twf_license_user;
GRANT ALL PRIVILEGES ON TABLE notification_history TO twf_license_user;

-- Grant permissions on views
GRANT SELECT ON active_licenses_view TO twf_license_user;
GRANT SELECT ON product_features_summary TO twf_license_user;

-- Verify permissions (optional - run this to check)
-- SELECT table_name, privilege_type 
-- FROM information_schema.table_privileges 
-- WHERE grantee = 'twf_license_user';

COMMIT;

-- Note: Run this script as follows:
-- 1. Connect as superuser: psql -h localhost -p 5433 -U postgres
-- 2. Run this script: \i /path/to/fix-permissions.sql
-- 3. Or execute manually: GRANT statements above
