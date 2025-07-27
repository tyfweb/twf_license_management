-- TechWayFit Licensing Management System - Sample Data Script Part 2
-- Version: 1.0.0-corrected - Consumers, Licenses, and System Data
-- Run this after sample-data-part1.sql

-- Continue transaction
BEGIN;

-- =============================================
-- CONSUMER ACCOUNTS DATA (5 consumers)
-- =============================================
INSERT INTO consumer_accounts (
    consumer_id, company_name, account_code, primary_contact_name, primary_contact_email, 
    primary_contact_phone, primary_contact_position, secondary_contact_name, secondary_contact_email, 
    secondary_contact_phone, secondary_contact_position, activated_at, subscription_end, 
    address_street, address_city, address_state, address_postal_code, address_country, 
    notes, status, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
('ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d', 'FitLife Gym Chain', 'FITLIFE001', 
 'Sarah Johnson', 'sarah.johnson@fitlife.com', '+1-555-1001', 'Operations Manager',
 'Mike Chen', 'mike.chen@fitlife.com', '+1-555-1002', 'IT Director',
 '2024-01-20 10:00:00+00', '2025-01-20 10:00:00+00',
 '123 Fitness Ave', 'Los Angeles', 'CA', '90210', 'USA',
 'Multi-location gym chain with 15 locations across California', 'Active', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e', 'Wellness Works Corp', 'WELLNESS002', 
 'David Rodriguez', 'david.rodriguez@wellnessworks.com', '+1-555-2001', 'CEO',
 'Lisa Wang', 'lisa.wang@wellnessworks.com', '+1-555-2002', 'Head of Wellness',
 '2024-02-15 14:00:00+00', '2025-02-15 14:00:00+00',
 '456 Health Blvd', 'New York', 'NY', '10001', 'USA',
 'Corporate wellness provider serving Fortune 500 companies', 'Active', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('ac3dcedf-ea7b-acbd-becf-3a4b5c6d7e8f', 'Elite Sports Academy', 'ELITE003', 
 'Jennifer Martinez', 'j.martinez@elitesports.com', '+1-555-3001', 'General Manager',
 'Tom Wilson', 't.wilson@elitesports.com', '+1-555-3002', 'Head Coach',
 '2024-03-01 09:00:00+00', '2025-03-01 09:00:00+00',
 '789 Champion Way', 'Chicago', 'IL', '60601', 'USA',
 'High-performance training facility for professional athletes', 'Active', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('bd4edfea-fb8c-bdce-cfda-4b5c6d7e8f9a', 'Community Health Centers', 'COMMUNITY004', 
 'Dr. Amanda Foster', 'a.foster@commhealth.org', '+1-555-4001', 'Medical Director',
 'James Thompson', 'j.thompson@commhealth.org', '+1-555-4002', 'Administrator',
 '2024-04-10 11:00:00+00', '2025-04-10 11:00:00+00',
 '321 Care Street', 'Austin', 'TX', '73301', 'USA',
 'Non-profit healthcare network with integrated wellness programs', 'Active', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('ce5feafb-ac9d-cedf-daeb-5c6d7e8f9a0b', 'TechFit Solutions', 'TECHFIT005', 
 'Robert Kim', 'robert.kim@techfit.com', '+1-555-5001', 'Founder & CTO',
 'Maria Garcia', 'maria.garcia@techfit.com', '+1-555-5002', 'Product Manager',
 '2024-05-01 16:00:00+00', '2025-05-01 16:00:00+00',
 '654 Innovation Dr', 'Seattle', 'WA', '98101', 'USA',
 'Tech startup developing AI-powered fitness solutions', 'Active', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT CONSUMERS DATA (Relationships)
-- =============================================
INSERT INTO product_consumers (
    product_consumer_id, product_id, consumer_id, account_manager_name, account_manager_email, 
    account_manager_phone, account_manager_position, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
-- FitLife Gym Chain - uses Fitness Pro and Enterprise
('df6afbac-bd0e-dfea-ebfc-6d7e8f9a0b1c', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d',
 'Alex Turner', 'alex.turner@techwayfit.com', '+1-555-0101', 'Senior Account Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ea7bacbd-ce1f-eafb-fcad-7e8f9a0b1c2d', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d',
 'Alex Turner', 'alex.turner@techwayfit.com', '+1-555-0101', 'Senior Account Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Works Corp - uses all three products
('fb8cbdce-df2a-fbac-adbe-8f9a0b1c2d3e', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 'Rachel Green', 'rachel.green@techwayfit.com', '+1-555-0102', 'Enterprise Account Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ac9dcedf-ea3b-acbd-becf-9a0b1c2d3e4f', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 'Rachel Green', 'rachel.green@techwayfit.com', '+1-555-0102', 'Enterprise Account Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('bd0edfea-fb4c-bdce-cfda-0b1c2d3e4f5a', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 'Rachel Green', 'rachel.green@techwayfit.com', '+1-555-0102', 'Enterprise Account Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Elite Sports Academy - uses Fitness Pro and Wellness Suite
('ce1feafb-ac5d-cedf-daeb-1c2d3e4f5a6b', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'ac3dcedf-ea7b-acbd-becf-3a4b5c6d7e8f',
 'Mark Stevens', 'mark.stevens@techwayfit.com', '+1-555-0103', 'Sports Account Specialist',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('df2afbac-bd6e-dfea-ebfc-2d3e4f5a6b7c', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'ac3dcedf-ea7b-acbd-becf-3a4b5c6d7e8f',
 'Mark Stevens', 'mark.stevens@techwayfit.com', '+1-555-0103', 'Sports Account Specialist',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Community Health Centers - uses Wellness Suite
('ea3bacbd-ce7f-eafb-fcad-3e4f5a6b7c8d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'bd4edfea-fb8c-bdce-cfda-4b5c6d7e8f9a',
 'Dr. Susan Lee', 'susan.lee@techwayfit.com', '+1-555-0104', 'Healthcare Solutions Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- TechFit Solutions - uses Enterprise
('fb4cbdce-df8a-fbac-adbe-4f5a6b7c8d9e', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ce5feafb-ac9d-cedf-daeb-5c6d7e8f9a0b',
 'Kevin Park', 'kevin.park@techwayfit.com', '+1-555-0105', 'Tech Partner Manager',
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT LICENSES DATA (Active licenses)
-- =============================================
INSERT INTO product_licenses (
    license_id, license_code, product_id, consumer_id, valid_from, valid_to, encryption, signature,
    license_key, public_key, license_signature, key_generated_at, status, issued_by, 
    revoked_at, revocation_reason, metadata_json, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
-- FitLife Gym Chain licenses
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'LIC-FITLIFE-FP-001', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d',
 '2024-01-20 10:00:00+00', '2025-01-20 10:00:00+00', 'AES256', 'SHA256',
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9kdWN0IjoiZml0bmVzc19wcm8iLCJjdXN0b21lciI6ImZpdGxpZmUiLCJ0aWVyIjoiZW50ZXJwcmlzZSJ9.sample_key_fitlife_fp',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1234567890abcdef', 'SHA256_signature_fitlife_fp_001',
 '2024-01-20 10:00:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "Enterprise", "max_locations": 20, "max_members": 10000}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('df8afbac-bd2e-dfea-ebfc-8d9e0f1a2b3c', 'LIC-FITLIFE-ENT-001', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d',
 '2024-02-01 10:00:00+00', '2025-02-01 10:00:00+00', 'AES256', 'SHA256',
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9kdWN0IjoiZW50ZXJwcmlzZSIsImN1c3RvbWVyIjoiZml0bGlmZSIsInRpZXIiOiJzdGFuZGFyZCJ9.sample_key_fitlife_ent',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1234567890abcdef', 'SHA256_signature_fitlife_ent_001',
 '2024-02-01 10:00:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "Standard", "tenants": 5, "api_calls": 50000}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Works Corp licenses
('ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'LIC-WELLNESS-FP-001', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 '2024-02-15 14:00:00+00', '2025-02-15 14:00:00+00', 'AES256', 'SHA256',
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9kdWN0IjoiZml0bmVzc19wcm8iLCJjdXN0b21lciI6IndlbGxuZXNzd29ya3MiLCJ0aWVyIjoicHJvZmVzc2lvbmFsIn0.sample_key_wellness_fp',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA9876543210fedcba', 'SHA256_signature_wellness_fp_001',
 '2024-02-15 14:00:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "Professional", "max_locations": 10, "max_members": 5000}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'LIC-WELLNESS-WS-001', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 '2024-02-15 14:00:00+00', '2025-02-15 14:00:00+00', 'AES256', 'SHA256',
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9kdWN0Ijoid2VsbG5lc3Nfc3VpdGUiLCJjdXN0b21lciI6IndlbGxuZXNzd29ya3MiLCJ0aWVyIjoicHJlbWl1bSJ9.sample_key_wellness_ws',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5555666677778888', 'SHA256_signature_wellness_ws_001',
 '2024-02-15 14:00:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "Premium", "max_users": 2000, "integrations": true}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('ac1dcedf-ea5b-acbd-becf-1a2b3c4d5e6f', 'LIC-WELLNESS-ENT-001', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'fb2cbdce-df6a-fbac-adbe-2f3a4b5c6d7e',
 '2024-03-01 14:00:00+00', '2025-03-01 14:00:00+00', 'AES256', 'SHA256',
 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9kdWN0IjoiZW50ZXJwcmlzZSIsImN1c3RvbWVyIjoid2VsbG5lc3N3b3JrcyIsInRpZXIiOiJ1bHRpbWF0ZSJ9.sample_key_wellness_ent',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA7777888899990000', 'SHA256_signature_wellness_ent_001',
 '2024-03-01 14:00:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "Ultimate", "tenants": 20, "custom_dev_hours": 40, "white_label": true}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT LICENSE FEATURES (Many-to-many relationships)
-- =============================================
INSERT INTO product_license_features (license_id, feature_id) VALUES 
-- FitLife Fitness Pro license features
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'ce1fea9b-ac5d-0e7f-da8b-1c2d3e4f5a6b'), -- Member Management
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'df2afbac-bd6e-1f8a-eb9c-2d3e4f5a6b7c'), -- Workout Tracking
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'ea3bacbd-ce7f-2a9b-fcad-3e4f5a6b7c8d'), -- Progress Analytics
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'fb4cbdce-df8a-3bac-adbe-4f5a6b7c8d9e'), -- Class Scheduling
('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'ac5dcedf-ea9b-4cbd-becf-5a6b7c8d9e0f'), -- Multi-Location Support

-- FitLife Enterprise license features
('df8afbac-bd2e-dfea-ebfc-8d9e0f1a2b3c', 'ce3feafb-ac7d-cedf-daeb-3c4d5e6f7a8b'), -- Multi-Tenant Management
('df8afbac-bd2e-dfea-ebfc-8d9e0f1a2b3c', 'df4afbac-bd8e-dfea-ebfc-4d5e6f7a8b9c'), -- Advanced Security
('df8afbac-bd2e-dfea-ebfc-8d9e0f1a2b3c', 'ea5bacbd-ce9f-eafb-fcad-5e6f7a8b9c0d'), -- API Access
('df8afbac-bd2e-dfea-ebfc-8d9e0f1a2b3c', 'fb6cbdce-df0a-fbac-adbe-6f7a8b9c0d1e'), -- Advanced Reporting

-- Wellness Works Fitness Pro license features
('ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'ce1fea9b-ac5d-0e7f-da8b-1c2d3e4f5a6b'), -- Member Management
('ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'df2afbac-bd6e-1f8a-eb9c-2d3e4f5a6b7c'), -- Workout Tracking
('ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'ea3bacbd-ce7f-2a9b-fcad-3e4f5a6b7c8d'), -- Progress Analytics
('ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'fb4cbdce-df8a-3bac-adbe-4f5a6b7c8d9e'), -- Class Scheduling

-- Wellness Works Wellness Suite license features
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'bd6edfea-fb0c-5dce-cfda-6b7c8d9e0f1a'), -- Health Metrics
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'ce7feafb-ac1d-6edf-daeb-7c8d9e0f1a2b'), -- Nutrition Tracking
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'df8afbac-bd2e-7fea-ebfc-8d9e0f1a2b3c'), -- Sleep Monitoring
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'ea9bacbd-ce3f-8afb-fcad-9e0f1a2b3c4d'), -- Stress Management
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'fb0cbdce-df4a-9bac-adbe-0f1a2b3c4d5e'), -- Goal Setting
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'ac1dcedf-ea5b-acbd-becf-1a2b3c4d5e6f'), -- Community Features
('fb0cbdce-df4a-fbac-adbe-0f1a2b3c4d5e', 'bd2edfea-fb6c-bdce-cfda-2b3c4d5e6f7a'); -- Integration Hub

-- =============================================
-- NOTIFICATION TEMPLATES DATA
-- =============================================
INSERT INTO notification_templates (
    notification_template_id, template_name, message_template, notification_type, subject, 
    template_variable_json, is_active, notification_mode, created_by, updated_by, created_on, updated_on
) VALUES 
('ac5dcedf-ea9b-acbd-becf-5a6b7c8d9e0f', 'License Expiry Warning', 
 'Dear {{customer_name}}, your license for {{product_name}} will expire on {{expiry_date}}. Please contact your account manager to renew.', 
 'Warning', 'License Expiring Soon - {{product_name}}',
 '{"customer_name": "string", "product_name": "string", "expiry_date": "date"}', 
 true, 'Email', 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('bd6edfea-fb0c-bdce-cfda-6b7c8d9e0f1a', 'License Activation', 
 'Welcome {{customer_name}}! Your {{product_name}} license has been activated. License Code: {{license_code}}', 
 'Info', 'License Activated - {{product_name}}',
 '{"customer_name": "string", "product_name": "string", "license_code": "string"}', 
 true, 'Email', 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'License Revocation', 
 'Your license for {{product_name}} has been revoked due to: {{reason}}. Please contact support for assistance.', 
 'Alert', 'License Revoked - {{product_name}}',
 '{"customer_name": "string", "product_name": "string", "reason": "string"}', 
 true, 'Email', 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- NOTIFICATION HISTORY DATA
-- =============================================
INSERT INTO notification_history (
    notification_id, entity_id, entity_type, recipients_json, notification_mode, 
    notification_template_id, notification_type, sent_date, delivery_status, delivery_error, 
    is_active, created_by, updated_by, created_on, updated_on
) VALUES 
('ac1dcedf-ea5b-acbd-becf-1a2b3c4d5e6f', 'ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'License', 
 '{"primary": "sarah.johnson@fitlife.com", "cc": ["mike.chen@fitlife.com", "alex.turner@techwayfit.com"]}', 
 'Email', 'bd6edfea-fb0c-bdce-cfda-6b7c8d9e0f1a', 'Info', '2024-01-20 10:30:00+00', 'Delivered', NULL,
 true, 'system', NULL, CURRENT_TIMESTAMP, NULL),

('bd2edfea-fb6c-bdce-cfda-2b3c4d5e6f7a', 'ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'License', 
 '{"primary": "david.rodriguez@wellnessworks.com", "cc": ["lisa.wang@wellnessworks.com", "rachel.green@techwayfit.com"]}', 
 'Email', 'bd6edfea-fb0c-bdce-cfda-6b7c8d9e0f1a', 'Info', '2024-02-15 14:30:00+00', 'Delivered', NULL,
 true, 'system', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- AUDIT ENTRIES DATA
-- =============================================
INSERT INTO audit_entries (
    audit_entry_id, entity_type, entity_id, action_type, old_value, new_value, 
    ip_address, user_agent, reason, metadata, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
('ac3dcedf-ea7b-acbd-becf-3a4b5c6d7e8f', 'ProductLicense', 'ce7feafb-ac1d-cedf-daeb-7c8d9e0f1a2b', 'Created', 
 NULL, '{"status": "Active", "license_code": "LIC-FITLIFE-FP-001"}', 
 '192.168.1.100', 'Mozilla/5.0 (admin-portal)', 'License creation for FitLife Gym Chain', 
 '{"admin_user": "admin", "approval_required": false}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('bd4edfea-fb8c-bdce-cfda-4b5c6d7e8f9a', 'ConsumerAccount', 'ea1bacbd-ce5f-eafb-fcad-1e2f3a4b5c6d', 'Created', 
 NULL, '{"company_name": "FitLife Gym Chain", "status": "Active"}', 
 '192.168.1.100', 'Mozilla/5.0 (admin-portal)', 'New customer account creation', 
 '{"admin_user": "admin", "verification_completed": true}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('ce5feafb-ac9d-cedf-daeb-5c6d7e8f9a0b', 'ProductLicense', 'ea9bacbd-ce3f-eafb-fcad-9e0f1a2b3c4d', 'Created', 
 NULL, '{"status": "Active", "license_code": "LIC-WELLNESS-FP-001"}', 
 '192.168.1.101', 'Mozilla/5.0 (admin-portal)', 'License creation for Wellness Works Corp', 
 '{"admin_user": "admin", "bulk_creation": false}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

COMMIT;

-- =============================================
-- SUMMARY OF SAMPLE DATA
-- =============================================
-- Products: 3 (Fitness Pro, Wellness Suite, Enterprise)
-- Product Versions: 9 total (3-4 per product) 
-- Product Tiers: 7 total (2-3 per product)
-- Product Features: 20 total (5 + 7 + 8 respectively)
-- Consumer Accounts: 5 companies
-- Product Consumers: 9 relationships
-- Product Licenses: 5 active licenses
-- License Features: 26 feature assignments
-- Notification Templates: 3 system templates
-- Notification History: 2 sent notifications
-- Audit Entries: 3 audit records
-- =============================================
