-- TechWayFit Licensing Management System - PostgreSQL Seeding Script Part 2
-- Consumer Accounts, Product Consumers, and Product Licenses
-- Compatible with PostgreSQL 12+ and corrected schema with 'id' primary keys
-- Version: 1.0.0-pgsql

-- Start transaction
BEGIN;

-- =============================================
-- CONSUMER ACCOUNTS DATA (5 companies)
-- =============================================
INSERT INTO consumer_accounts (
    id, company_name, account_code, primary_contact_name, primary_contact_email, 
    primary_contact_phone, primary_contact_position, secondary_contact_name, secondary_contact_email, 
    secondary_contact_phone, secondary_contact_position, activated_at, subscription_end,
    address_street, address_city, address_state, address_postal_code, address_country, 
    notes, status, 
    is_active, is_deleted, created_by, updated_by, created_on, updated_on
) VALUES 
('a1b2c3d4-e5f6-4789-a123-456789abcdef', 'FitLife Gym Chain LLC', 'FLC001', 
 'John Martinez', 'john.martinez@fitlifegyms.com', '+1-555-1001', 'Operations Director',
 'Maria Rodriguez', 'maria.rodriguez@fitlifegyms.com', '+1-555-1002', 'IT Manager',
 '2024-01-20 09:00:00+00', '2025-01-19 23:59:59+00',
 '123 Fitness Ave, Suite 100', 'Los Angeles', 'CA', '90210', 'USA', 
 'Multi-location gym chain with 15 locations across California', 'Active', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('b2c3d4e5-f6a7-5890-b234-56789abcdef0', 'Wellness Works Corporation', 'WWC001', 
 'Sarah Johnson', 'sarah.johnson@wellnessworks.com', '+1-555-2002', 'VP of Wellness',
 'Michael Brown', 'michael.brown@wellnessworks.com', '+1-555-2003', 'IT Director',
 '2024-02-10 11:30:00+00', '2025-02-09 23:59:59+00',
 '456 Health Blvd', 'New York', 'NY', '10001', 'USA', 
 'Fortune 500 company implementing comprehensive employee wellness program', 'Active', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('c3d4e5f6-a7b8-6901-c345-6789abcdef01', 'Elite Sports Academy Inc', 'ESA001', 
 'Mike Thompson', 'mike.thompson@elitesports.edu', '+1-555-3003', 'Academy Director',
 'Jennifer Adams', 'jennifer.adams@elitesports.edu', '+1-555-3004', 'Program Coordinator',
 '2024-02-25 14:15:00+00', '2025-02-24 23:59:59+00',
 '789 Champion Dr', 'Austin', 'TX', '73301', 'USA', 
 'Premier sports training academy for young athletes', 'Active', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('d4e5f6a7-b8c9-7012-d456-789abcdef012', 'Community Health Center Foundation', 'CHC001', 
 'Dr. Lisa Chen', 'lisa.chen@communityhealthcenter.org', '+1-555-4004', 'Medical Director',
 'Robert Kim', 'robert.kim@communityhealthcenter.org', '+1-555-4005', 'Administrator',
 '2024-03-05 10:45:00+00', '2025-03-04 23:59:59+00',
 '321 Wellness Way', 'Seattle', 'WA', '98101', 'USA', 
 'Non-profit community health center serving underserved populations', 'Active', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('e5f6a7b8-c9d0-8123-e567-89abcdef0123', 'Corporate Fitness Solutions LLC', 'CFS001', 
 'David Wilson', 'david.wilson@corpfitness.com', '+1-555-5005', 'CEO',
 'Amanda Smith', 'amanda.smith@corpfitness.com', '+1-555-5006', 'Operations Manager',
 '2024-03-15 13:20:00+00', '2025-03-14 23:59:59+00',
 '654 Business Park Dr, Building C', 'Denver', 'CO', '80202', 'USA', 
 'Provides fitness consulting and management services to corporate clients', 'Active', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT CONSUMERS DATA (consumer-product relationships)
-- =============================================
INSERT INTO product_consumers (
    id, product_id, consumer_id, account_manager_name, account_manager_email, 
    account_manager_phone, account_manager_position,
    is_active, is_deleted, created_by, updated_by, created_on, updated_on
) VALUES 
-- FitLife Gym Chain - Fitness Pro Premium
('f6a7b8c9-d0e1-9234-f678-9abcdef01234', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'a1b2c3d4-e5f6-4789-a123-456789abcdef', 
 'Jennifer Adams', 'jennifer.adams@techwayfit.com', '+1-555-3001', 'Senior Account Manager',
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Works Corp - Wellness Suite Enterprise
('a7b8c9d0-e1f2-0345-a789-abcdef012345', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'b2c3d4e5-f6a7-5890-b234-56789abcdef0', 
 'Robert Kim', 'robert.kim@techwayfit.com', '+1-555-3002', 'Account Manager',
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Elite Sports Academy - Fitness Pro Professional
('b8c9d0e1-f2a3-1456-b890-bcdef0123456', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'c3d4e5f6-a7b8-6901-c345-6789abcdef01', 
 'Amanda Rodriguez', 'amanda.rodriguez@techwayfit.com', '+1-555-4001', 'Senior Support Specialist',
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Community Health Center - Wellness Suite Starter
('c9d0e1f2-a3b4-2567-c901-cdef01234567', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'd4e5f6a7-b8c9-7012-d456-789abcdef012', 
 'Maria Santos', 'maria.santos@techwayfit.com', '+1-555-4002', 'Support Specialist',
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Corporate Fitness Solutions - Enterprise Corporate
('d0e1f2a3-b4c5-3678-d012-def012345678', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'e5f6a7b8-c9d0-8123-e567-89abcdef0123', 
 'Thomas Lee', 'thomas.lee@techwayfit.com', '+1-555-5001', 'Business Analyst',
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT LICENSES DATA (active licenses for each consumer)
-- =============================================
INSERT INTO product_licenses (
    id, license_code, product_id, consumer_id, valid_from, valid_to, 
    encryption, signature, license_key, public_key, license_signature, 
    key_generated_at, status, issued_by, revoked_at, revocation_reason, 
    metadata_json, is_active, is_deleted, created_by, updated_by, created_on, updated_on
) VALUES 
-- FitLife Gym Chain License
('e1f2a3b4-c5d6-4789-e123-ef0123456789', 'FLP-PREM-2024-001', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'a1b2c3d4-e5f6-4789-a123-456789abcdef', 
 '2024-01-20 09:00:00+00', '2025-01-19 23:59:59+00', 'AES256', 'SHA256', 
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJGaXRMaWZlR3ltQ2hhaW4iLCJsaWNlbnNlX2NvZGUiOiJGTFAtUFJFTS0yMDI0LTAwMSIsInByb2R1Y3RfaWQiOiJmNDdhYzEwYi01OGNjLTQzNzItYTU2Ny0wZTAyYjJjM2Q0NzkiLCJjb25zdW1lcl9pZCI6ImExYjJjM2Q0LWU1ZjYtNDc4OS1hMTIzLTQ1Njc4OWFiY2RlZiIsImV4cCI6MTczNzM3Nzk5OX0', 
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...', 
 'sha256signature_fitlife_gym_chain_license_signature', 
 '2024-01-20 09:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"locations": 15, "concurrent_users": 500, "features": ["member_mgmt", "payment_proc", "analytics", "mobile_app"]}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Works Corp License
('f2a3b4c5-d6e7-5890-f234-f01234567890', 'WWC-ENT-2024-001', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'b2c3d4e5-f6a7-5890-b234-56789abcdef0', 
 '2024-02-10 11:30:00+00', '2025-02-09 23:59:59+00', 'AES256', 'SHA256', 
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJXZWxsbmVzc1dvcmtzQ29ycCIsImxpY2Vuc2VfY29kZSI6IldXQy1FTlQtMjAyNC0wMDEiLCJwcm9kdWN0X2lkIjoiNmJhN2I4MTAtOWRhZC0xMWQxLTgwYjQtMDBjMDRmZDQzMGM4IiwiY29uc3VtZXJfaWQiOiJiMmMzZDRlNS1mNmE3LTU4OTAtYjIzNC01Njc4OWFiY2RlZjAiLCJleHAiOjE3Mzg0NDk5OTl9', 
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...', 
 'sha256signature_wellness_works_corp_license_signature', 
 '2024-02-10 11:30:00+00', 'Active', 'admin', NULL, NULL, 
 '{"employees": 5000, "departments": 12, "integrations": ["sso", "hris", "payroll"], "custom_programs": 8}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Elite Sports Academy License
('a3b4c5d6-e7f8-6901-a345-012345678901', 'ESA-PRO-2024-001', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'c3d4e5f6-a7b8-6901-c345-6789abcdef01', 
 '2024-02-25 14:15:00+00', '2025-02-24 23:59:59+00', 'AES256', 'SHA256', 
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJFbGl0ZVNwb3J0c0FjYWRlbXkiLCJsaWNlbnNlX2NvZGUiOiJFU0EtUFJPLTIwMjQtMDAxIiwicHJvZHVjdF9pZCI6ImY0N2FjMTBiLTU4Y2MtNDM3Mi1hNTY3LTBlMDJiMmMzZDQ3OSIsImNvbnN1bWVyX2lkIjoiYzNkNGU1ZjYtYTdiOC02OTAxLWMzNDUtNjc4OWFiY2RlZjAxIiwiZXhwIjoxNzQwMzM1OTk5fQ', 
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...', 
 'sha256signature_elite_sports_academy_license_signature', 
 '2024-02-25 14:15:00+00', 'Active', 'admin', NULL, NULL, 
 '{"students": 450, "programs": 35, "performance_metrics": true, "parent_access": true}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Community Health Center License
('b4c5d6e7-f8a9-7012-b456-123456789012', 'CHC-START-2024-001', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'd4e5f6a7-b8c9-7012-d456-789abcdef012', 
 '2024-03-05 10:45:00+00', '2025-03-04 23:59:59+00', 'AES256', 'SHA256', 
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJDb21tdW5pdHlIZWFsdGhDZW50ZXIiLCJsaWNlbnNlX2NvZGUiOiJDSEMtU1RBUlQtMjAyNC0wMDEiLCJwcm9kdWN0X2lkIjoiNmJhN2I4MTAtOWRhZC0xMWQxLTgwYjQtMDBjMDRmZDQzMGM4IiwiY29uc3VtZXJfaWQiOiJkNGU1ZjZhNy1iOGM5LTcwMTItZDQ1Ni03ODlhYmNkZWYwMTIiLCJleHAiOjE3NDEyMjg3OTl9', 
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...', 
 'sha256signature_community_health_center_license_signature', 
 '2024-03-05 10:45:00+00', 'Active', 'admin', NULL, NULL, 
 '{"patients": 850, "basic_features": true, "community_programs": 6, "volunteer_access": true}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Corporate Fitness Solutions License
('c5d6e7f8-a9b0-8123-c567-234567890123', 'CFS-CORP-2024-001', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'e5f6a7b8-c9d0-8123-e567-89abcdef0123', 
 '2024-03-15 13:20:00+00', '2025-03-14 23:59:59+00', 'AES256', 'SHA256', 
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJDb3Jwb3JhdGVGaXRuZXNzU29sdXRpb25zIiwibGljZW5zZV9jb2RlIjoiQ0ZTLUNPUlAtMjAyNC0wMDEiLCJwcm9kdWN0X2lkIjoiNmJhN2I4MTEtOWRhZC0xMWQxLTgwYjQtMDBjMDRmZDQzMGM4IiwiY29uc3VtZXJfaWQiOiJlNWY2YTdiOC1jOWQwLTgxMjMtZTU2Ny04OWFiY2RlZjAxMjMiLCJleHAiOjE3NDExMjU1OTl9', 
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...', 
 'sha256signature_corporate_fitness_solutions_license_signature', 
 '2024-03-15 13:20:00+00', 'Active', 'admin', NULL, NULL, 
 '{"reseller_clients": 18, "white_label": true, "api_access": "full", "custom_development": 15}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

COMMIT;

-- Display success message
SELECT 'Successfully inserted sample data part 2: Consumer Accounts, Product Consumers, and Product Licenses' AS status;
