-- TechWayFit Licensing Management System - PostgreSQL Seeding Script Part 3
-- User Roles, User Profiles, User Role Mappings, and System Configuration
-- Compatible with PostgreSQL 12+ and corrected schema with 'id' primary keys
-- Version: 1.0.0-pgsql

-- Start transaction
BEGIN;

-- =============================================
-- USER ROLES DATA (3 system roles)
-- =============================================
INSERT INTO user_roles (
    id, role_name, role_description, is_admin,
    is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
('10000000-0000-0000-0000-000000000001', 'Administrator', 
 'Full system access with all permissions including user management, system configuration, and complete data access', 
 true, true, false, 'system', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('10000000-0000-0000-0000-000000000002', 'Manager', 
 'Can create and manage most parts of the system except system settings and user management. Has access to all data views and can perform CRUD operations on products, licenses, and consumers', 
 false, true, false, 'system', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('10000000-0000-0000-0000-000000000003', 'User', 
 'Read-only access to all views except system settings and user management. Can view products, licenses, and consumer information but cannot modify data', 
 false, true, false, 'system', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- =============================================
-- USER PROFILES DATA (6 system users)
-- =============================================
INSERT INTO user_profiles (
    id, user_name, password_hash, password_salt, full_name, email, department, 
    is_locked, is_deleted, is_admin, last_login_date, failed_login_attempts, locked_date,
    is_active, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- Administrator Account - Password: Admin@123
('20000000-0000-0000-0000-000000000001', 'admin', 
 'v5yPL6Qc1jnMONXAZfwvWkuRuyXAOD4vBF5MJWVpFJw=', 'admin_salt_2024', 
 'System Administrator', 'admin@techwayfit.com', 'IT Department', 
 false, false, true, '2024-07-15 08:30:00+00', 0, NULL,
 true, 'system', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Manager Account - Password: Manager@123
('20000000-0000-0000-0000-000000000002', 'manager', 
 'tqudb+jIhjTjHikC8SrZDBSSm2HXpn13Ga3S+xyKblw=', 'manager_salt_2024', 
 'License Manager', 'manager@techwayfit.com', 'License Management', 
 false, false, false, '2024-07-15 09:15:00+00', 0, NULL,
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- User Account - Password: User@123
('20000000-0000-0000-0000-000000000003', 'user', 
 'DeejnpUJ/E59c7munAc4/jaLlmEbjB3+imsUXtdl7UQ=', 'user_salt_2024', 
 'Regular User', 'user@techwayfit.com', 'Support Department', 
 false, false, false, '2024-07-15 10:45:00+00', 0, NULL,
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Additional sample users for demonstration - Password: Demo@123
('20000000-0000-0000-0000-000000000004', 'john.doe', 
 'fX26ljrobvviwEwp8iNgklk6m+PSvugCUexJXy+bWpk=', 'john_salt_2024', 
 'John Doe', 'john.doe@techwayfit.com', 'Sales Department', 
 false, false, false, '2024-07-15 11:20:00+00', 0, NULL,
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('20000000-0000-0000-0000-000000000005', 'jane.smith', 
 'wi859whkHl0AlJz0+3XZjuuQQjgOEYDnquvO+2CDO5M=', 'jane_salt_2024', 
 'Jane Smith', 'jane.smith@techwayfit.com', 'Customer Success', 
 false, false, false, '2024-07-15 13:10:00+00', 0, NULL,
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('20000000-0000-0000-0000-000000000006', 'mike.wilson', 
 'P+9lIOuSkz8KdPJ3uapXrtas753WveQPIkV/34WnaJE=', 'mike_salt_2024', 
 'Mike Wilson', 'mike.wilson@techwayfit.com', 'Technical Support', 
 false, false, false, '2024-07-15 14:25:00+00', 0, NULL,
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- =============================================
-- USER ROLE MAPPINGS DATA (assign roles to users)
-- =============================================
INSERT INTO user_role_mappings (
    id, user_id, role_id, assigned_date, expiry_date,
    is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- Assign Administrator role to admin user
('30000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 
 '2024-01-01 00:00:00+00', NULL,
 true, false, 'system', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Assign Manager role to manager user
('30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002', 
 '2024-01-15 10:00:00+00', NULL,
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Assign User role to regular user
('30000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 
 '2024-02-01 09:00:00+00', NULL,
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Assign Manager role to John Doe
('30000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000002', 
 '2024-02-15 14:00:00+00', NULL,
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Assign User role to Jane Smith
('30000000-0000-0000-0000-000000000005', '20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000003', 
 '2024-03-01 11:00:00+00', NULL,
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Assign User role to Mike Wilson
('30000000-0000-0000-0000-000000000006', '20000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000003', 
 '2024-03-15 10:30:00+00', NULL,
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);


COMMIT;

-- =============================================
-- USAGE NOTES
-- =============================================

/*
Sample Login Credentials (for testing):

1. Administrator Account:
   Username: admin
   Password: Admin@123
   Role: Administrator
   
2. Manager Account:
   Username: manager
   Password: Manager@123
   Role: Manager
   
3. User Account:
   Username: user
   Password: User@123
   Role: User

Additional test accounts (Password: Demo@123):
- john.doe / John Doe / Manager role
- jane.smith / Jane Smith / User role  
- mike.wilson / Mike Wilson / User role

IMPORTANT SECURITY NOTES:
- All passwords shown here are examples and should be changed immediately in production
- Password hashes shown are simplified examples - implement proper bcrypt hashing
- Consider implementing password complexity requirements
- Consider implementing account lockout policies
- Enable audit logging for all user management operations

ROLE PERMISSIONS:
- Administrator: Complete system access including user management
- Manager: CRUD access to products, licenses, consumers (no user/system management)
- User: Read-only access to all data views (no user/system management)
*/

-- Display success message
SELECT 'Successfully inserted sample data part 3: User Roles, User Profiles, User Role Mappings, and Settings' AS status;
