-- TechWayFit Licensing Management System - Password Hash Update Script
-- Updates password hashes from bcrypt format to SHA256+salt format
-- Run this script against your existing database to fix authentication issues
-- Compatible with PostgreSQL 12+

BEGIN;

-- =============================================
-- UPDATE PASSWORD HASHES
-- =============================================

-- Update password hashes to match SecurityHelper.HashPasswordWithSalt() format
UPDATE user_profiles 
SET 
    password_hash = CASE user_name
        WHEN 'admin' THEN 'v5yPL6Qc1jnMONXAZfwvWkuRuyXAOD4vBF5MJWVpFJw='
        WHEN 'manager' THEN 'tqudb+jIhjTjHikC8SrZDBSSm2HXpn13Ga3S+xyKblw='
        WHEN 'user' THEN 'DeejnpUJ/E59c7munAc4/jaLlmEbjB3+imsUXtdl7UQ='
        WHEN 'john.doe' THEN 'fX26ljrobvviwEwp8iNgklk6m+PSvugCUexJXy+bWpk='
        WHEN 'jane.smith' THEN 'wi859whkHl0AlJz0+3XZjuuQQjgOEYDnquvO+2CDO5M='
        WHEN 'mike.wilson' THEN 'P+9lIOuSkz8KdPJ3uapXrtas753WveQPIkV/34WnaJE='
    END,
    updated_by = 'password-fix-script',
    updated_on = CURRENT_TIMESTAMP
WHERE user_name IN ('admin', 'manager', 'user', 'john.doe', 'jane.smith', 'mike.wilson')
  AND is_active = true 
  AND is_deleted = false;

-- =============================================
-- VERIFICATION
-- =============================================

-- Show updated users
SELECT 
    user_name,
    CASE 
        WHEN password_hash LIKE '$2a$%' THEN 'OLD_BCRYPT'
        WHEN password_hash LIKE '%=' THEN 'NEW_SHA256'  
        ELSE 'UNKNOWN'
    END as hash_format,
    password_salt,
    updated_by,
    updated_on
FROM user_profiles 
WHERE user_name IN ('admin', 'manager', 'user', 'john.doe', 'jane.smith', 'mike.wilson')
  AND is_active = true 
  AND is_deleted = false
ORDER BY user_name;

COMMIT;

-- Test the updated passwords with these credentials:
-- admin / Admin@123
-- manager / Manager@123  
-- user / User@123
-- john.doe / Demo@123
-- jane.smith / Demo@123
-- mike.wilson / Demo@123
