-- TechWayFit Licensing Management System - Sample User Data
-- PostgreSQL Sample Data for User Management Tables
-- This script initializes the system with default roles and sample users
-- Version: 1.0.0

-- =============================================
-- SAMPLE USER ROLES
-- =============================================

-- Insert default system roles
INSERT INTO user_roles (role_id, role_name, role_description, is_admin, created_by) VALUES
    ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Administrator', 'Full system access with all permissions including user management, system configuration, and complete data access', true, 'system'),
    ('b2c3d4e5-f6a7-8901-bcde-f23456789012', 'Manager', 'Can create and manage most parts of the system except system settings and user management. Has access to all data views and can perform CRUD operations on products, licenses, and consumers', false, 'system'),
    ('c3d4e5f6-a7b8-9012-cdef-345678901234', 'User', 'Read-only access to all views except system settings and user management. Can view products, licenses, and consumer information but cannot modify data', false, 'system');

-- =============================================
-- SAMPLE USER PROFILES
-- =============================================

-- Sample Administrator User
-- Password: Admin@123 (hashed with salt)
INSERT INTO user_profiles (
    user_id, 
    user_name, 
    password_hash, 
    password_salt, 
    full_name, 
    email, 
    department, 
    is_admin, 
    created_by
) VALUES (
    'd4e5f6a7-b8c9-0123-def4-56789012345a',
    'admin',
    'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3',  -- SHA-256 hash of "Admin@123" + salt
    'admin_salt_2024',
    'System Administrator',
    'admin@techway.com',
    'IT Department',
    true,
    'system'
);

-- Sample Manager User
-- Password: Manager@123 (hashed with salt)
INSERT INTO user_profiles (
    user_id, 
    user_name, 
    password_hash, 
    password_salt, 
    full_name, 
    email, 
    department, 
    is_admin,
    created_by
) VALUES (
    'e5f6a7b8-c9d0-1234-ef56-789012345b6c',
    'manager',
    'b109f3bbbc244eb82441917ed06d618b9008dd09b3befd1b5e07394c706a8bb980b1d7785e5976ec049b46df5f1326af5a2ea6d103fd07c95385ffab0cacbc86',  -- SHA-256 hash of "Manager@123" + salt
    'manager_salt_2024',
    'License Manager',
    'manager@techway.com',
    'License Management',
    false,
    'system'
);

-- Sample Regular User
-- Password: User@123 (hashed with salt)
INSERT INTO user_profiles (
    user_id, 
    user_name, 
    password_hash, 
    password_salt, 
    full_name, 
    email, 
    department, 
    is_admin,
    created_by
) VALUES (
    'f6a7b8c9-d0e1-2345-f678-90123456c7d8',
    'user',
    'c4ca4238a0b923820dcc509a6f75849b',  -- SHA-256 hash of "User@123" + salt
    'user_salt_2024',
    'Regular User',
    'user@techway.com',
    'Support Department',
    false,
    'system'
);

-- Additional sample users for demonstration
INSERT INTO user_profiles (
    user_id, 
    user_name, 
    password_hash, 
    password_salt, 
    full_name, 
    email, 
    department, 
    is_admin,
    created_by
) VALUES 
(
    'a7b8c9d0-e1f2-3456-789a-bc123456d8e9',
    'john.doe',
    'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f',  -- Sample hash
    'john_salt_2024',
    'John Doe',
    'john.doe@techway.com',
    'Sales Department',
    false,
    'admin'
),
(
    'b8c9d0e1-f2a3-4567-89ab-cd123456e9fa',
    'jane.smith',
    'c775e7b757ede630cd0aa1113bd102661ab38829ca52a6422ab782862f268646',  -- Sample hash
    'jane_salt_2024',
    'Jane Smith',
    'jane.smith@techway.com',
    'Customer Success',
    false,
    'admin'
),
(
    'c9d0e1f2-a3b4-5678-9abc-de123456fa0b',
    'mike.wilson',
    'a4c5c32b0a3a10d7e23c2c8e4c8c0e3e7f1a4b9c2d5e6f7a8b9c0d1e2f3a4b5',  -- Sample hash
    'mike_salt_2024',
    'Mike Wilson',
    'mike.wilson@techway.com',
    'Technical Support',
    false,
    'admin'
);

-- =============================================
-- USER ROLE MAPPINGS
-- =============================================

-- Assign Administrator role to admin user
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('d0e1f2a3-b4c5-6789-abcd-ef123456b0c1', 'd4e5f6a7-b8c9-0123-def4-56789012345a', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'system');

-- Assign Manager role to manager user
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('e1f2a3b4-c5d6-789a-bcde-f123456c1d2e', 'e5f6a7b8-c9d0-1234-ef56-789012345b6c', 'b2c3d4e5-f6a7-8901-bcde-f23456789012', 'system');

-- Assign User role to regular user
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('f2a3b4c5-d6e7-89ab-cdef-123456d2e3f4', 'f6a7b8c9-d0e1-2345-f678-90123456c7d8', 'c3d4e5f6-a7b8-9012-cdef-345678901234', 'system');

-- Assign Manager role to John Doe
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('a3b4c5d6-e7f8-9abc-def1-23456e3f4a5b', 'a7b8c9d0-e1f2-3456-789a-bc123456d8e9', 'b2c3d4e5-f6a7-8901-bcde-f23456789012', 'admin');

-- Assign User role to Jane Smith
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('b4c5d6e7-f8a9-abcd-ef12-3456f4a5b6c7', 'b8c9d0e1-f2a3-4567-89ab-cd123456e9fa', 'c3d4e5f6-a7b8-9012-cdef-345678901234', 'admin');

-- Assign User role to Mike Wilson
INSERT INTO user_role_mappings (mapping_id, user_id, role_id, created_by) VALUES
    ('c5d6e7f8-a9ba-bcde-f123-456a5b6c7d8e', 'c9d0e1f2-a3b4-5678-9abc-de123456fa0b', 'c3d4e5f6-a7b8-9012-cdef-345678901234', 'admin');

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Verify roles were created
-- SELECT role_id, role_name, role_description, is_admin FROM user_roles ORDER BY role_name;

-- Verify users were created
-- SELECT user_id, user_name, full_name, email, department, is_admin FROM user_profiles ORDER BY user_name;

-- Verify role assignments
-- SELECT 
--     up.user_name, 
--     up.full_name, 
--     ur.role_name,
--     urm.assigned_date
-- FROM user_profiles up
-- JOIN user_role_mappings urm ON up.user_id = urm.user_id
-- JOIN user_roles ur ON urm.role_id = ur.role_id
-- WHERE up.is_active = true AND urm.is_active = true
-- ORDER BY up.user_name;

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

Additional test accounts:
- john.doe / John Doe / Manager role
- jane.smith / Jane Smith / User role  
- mike.wilson / Mike Wilson / User role

IMPORTANT SECURITY NOTES:
- All passwords shown here are examples and should be changed immediately in production
- Password hashes shown are simplified examples - implement proper SHA-256 + salt hashing
- Consider implementing password complexity requirements
- Consider implementing account lockout policies
- Enable audit logging for all user management operations

ROLE PERMISSIONS:
- Administrator: Complete system access including user management
- Manager: CRUD access to products, licenses, consumers (no user/system management)
- User: Read-only access to all data views (no user/system management)
*/

COMMIT;

-- End of sample user data script
