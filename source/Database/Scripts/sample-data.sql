-- TechWayFit Licensing Management System - Sample Data Script
-- This script populates all tables with realistic test data for development and testing
-- Run this after creating the database schema with v1.0.0-initial-schema.sql

-- Start transaction
BEGIN;

-- =============================================
-- PRODUCTS DATA
-- =============================================

-- Insert sample products
INSERT INTO products (
    product_id, product_name, product_code, description, vendor_name, 
    is_active, created_by, created_on
) VALUES 
('b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'TechWayFit Fitness Pro', 'TWF-FITNESS', 
 'Complete fitness management solution for gyms and wellness centers', 
 'TechWayFit Inc.', true, 'admin', CURRENT_TIMESTAMP),
 
('5876d029-83f5-4f02-8f86-d90c83b78e85', 'TechWayFit Wellness Suite', 'TWF-WELLNESS', 
 'Comprehensive wellness tracking and management platform', 
 'TechWayFit Inc.', true, 'admin', CURRENT_TIMESTAMP),
 
('9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'TechWayFit Nutrition Tracker', 'TWF-NUTRITION', 
 'Advanced nutrition planning and tracking application', 
 'TechWayFit Inc.', true, 'admin', CURRENT_TIMESTAMP),
 
('782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'TechWayFit Enterprise Suite', 'TWF-ENTERPRISE', 
 'Enterprise-grade fitness and wellness management solution', 
 'TechWayFit Inc.', true, 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT VERSIONS DATA
-- =============================================

INSERT INTO product_versions (
    product_version_id, product_id, version_number, version_name, 
    description, release_date, is_active, created_by, created_on
) VALUES 
('e2d12831-861d-4e94-acd6-7b6285417fe2', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '1.0.0', 'Initial Release', 
 'First stable release of TechWayFit Fitness Pro', '2024-01-15', true, 'admin', CURRENT_TIMESTAMP),
 
('0a3d0dc6-3c0f-41ad-b0d2-8a75d1628898', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '1.1.0', 'Feature Enhancement', 
 'Added advanced reporting and analytics', '2024-03-20', true, 'admin', CURRENT_TIMESTAMP),
 
('df31bcac-4ccc-4b55-ba82-4f427d24740e', '5876d029-83f5-4f02-8f86-d90c83b78e85', '1.0.0', 'Beta Release', 
 'Beta version with core wellness features', '2024-02-10', true, 'admin', CURRENT_TIMESTAMP),
 
('029f2f89-3ff3-44d5-9613-ba0c2c4ea6ce', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', '1.0.0', 'MVP Release', 
 'Minimum viable product with basic nutrition tracking', '2024-04-05', true, 'admin', CURRENT_TIMESTAMP),
 
('a09ea69a-3cd5-416e-a49c-e61468cdfa53', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', '2.0.0', 'Enterprise Launch', 
 'Full enterprise suite with multi-tenant support', '2024-05-01', true, 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT TIERS DATA
-- =============================================

INSERT INTO product_tiers (
    product_tier_id, product_id, tier_name, tier_code, description, 
    pricing_model, base_price, is_active, display_order, created_by, created_on
) VALUES 
-- TechWayFit Fitness Pro tiers
('28d421cd-e028-4857-b4c9-7768060f2069', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Basic', 'BASIC', 
 'Essential fitness tracking for small gyms', 'monthly', 99.99, true, 1, 'admin', CURRENT_TIMESTAMP),
 
('8b9ebb67-d001-4c6d-b5b9-0314076c3fed', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Professional', 'PRO', 
 'Advanced features for medium-sized fitness centers', 'monthly', 199.99, true, 2, 'admin', CURRENT_TIMESTAMP),
 
('751ea743-62a1-4427-a754-29ad3c0e76fb', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Premium', 'PREMIUM', 
 'Full-featured solution for large gym chains', 'monthly', 399.99, true, 3, 'admin', CURRENT_TIMESTAMP),

-- TechWayFit Wellness Suite tiers
('f786670c-9db0-4128-a9b0-bdd2b5a8b525', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'Starter', 'STARTER', 
 'Basic wellness tracking for individuals', 'monthly', 29.99, true, 1, 'admin', CURRENT_TIMESTAMP),
 
('5b971186-9b37-4c62-866f-91097e594ab4', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'Family', 'FAMILY', 
 'Wellness management for families', 'monthly', 79.99, true, 2, 'admin', CURRENT_TIMESTAMP),

-- TechWayFit Nutrition Tracker tiers
('b5276e8b-fd12-4e0b-b86e-faae5b9fcbc7', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'Personal', 'PERSONAL', 
 'Individual nutrition tracking', 'monthly', 19.99, true, 1, 'admin', CURRENT_TIMESTAMP),
 
('23743e1b-1348-4c11-875e-1564b6dc5534', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'Coach', 'COACH', 
 'Nutrition planning for fitness coaches', 'monthly', 59.99, true, 2, 'admin', CURRENT_TIMESTAMP),

-- TechWayFit Enterprise Suite tiers
('2df322a2-acc7-4eb6-a0a1-e6a0cc21094a', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'Standard', 'STANDARD', 
 'Standard enterprise features', 'yearly', 2999.99, true, 1, 'admin', CURRENT_TIMESTAMP),
 
('b55d151c-f29b-493f-a86f-2f09e9b19025', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'Premium', 'PREMIUM', 
 'Premium enterprise with advanced analytics', 'yearly', 4999.99, true, 2, 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT FEATURES DATA
-- =============================================

INSERT INTO product_features (
    product_feature_id, product_id, feature_name, feature_code, description, 
    feature_type, is_core_feature, is_active, created_by, created_on
) VALUES 
-- Fitness Pro features
('42c71a50-49e6-450f-80fb-b83b1d20c2ae', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'User Management', 'USER_MGMT', 
 'Manage gym members and staff accounts', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('77cb8b77-9db7-486f-9cfa-6774e01e6635', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Workout Tracking', 'WORKOUT_TRACK', 
 'Track and log workout sessions', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('bdee77e3-7caf-4196-a2b1-8087df6a8aae', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Equipment Management', 'EQUIPMENT_MGMT', 
 'Manage gym equipment and maintenance', 'addon', false, true, 'admin', CURRENT_TIMESTAMP),
 
('acafef8b-21d6-4461-960e-1385fc7d1219', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Advanced Reporting', 'REPORTING', 
 'Generate detailed fitness reports and analytics', 'premium', false, true, 'admin', CURRENT_TIMESTAMP),
 
('66a99388-ee13-467d-886d-19fbef7d6ff7', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', 'Mobile App Access', 'MOBILE_APP', 
 'Access via mobile applications', 'addon', false, true, 'admin', CURRENT_TIMESTAMP),

-- Wellness Suite features
('5632b479-fb02-460c-8d11-a395f8a81146', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'Health Tracking', 'HEALTH_TRACK', 
 'Track vital signs and health metrics', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('87ebf873-5b74-4148-96d3-bbf23f2f79e7', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'Goal Setting', 'GOALS', 
 'Set and track wellness goals', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('b8952f1b-62f7-41e7-b14e-794abfeb16c8', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'Meditation Sessions', 'MEDITATION', 
 'Guided meditation and mindfulness', 'addon', false, true, 'admin', CURRENT_TIMESTAMP),

-- Nutrition Tracker features
('35bbf68c-4faa-4abd-a091-8d952b8340a4', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'Meal Planning', 'MEAL_PLAN', 
 'Plan and track daily meals', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('abaec6e3-6d30-40db-9990-33b87a6cdaa0', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'Calorie Tracking', 'CALORIE_TRACK', 
 'Track daily caloric intake', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('4cd37113-728b-4bb9-88a0-37251062d523', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', 'Recipe Database', 'RECIPE_DB', 
 'Access to extensive recipe database', 'addon', false, true, 'admin', CURRENT_TIMESTAMP),

-- Enterprise Suite features
('287c09e3-cb38-4a5f-ac9b-afe3e4a74216', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'Multi-Tenant Support', 'MULTI_TENANT', 
 'Support for multiple organizations', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('8fbe1644-dbde-4666-a6a5-26fd78b469ae', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'API Access', 'API_ACCESS', 
 'RESTful API for integrations', 'core', true, true, 'admin', CURRENT_TIMESTAMP),
 
('30556d75-46a4-486f-bc2d-eb9601ae46b0', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'Single Sign-On', 'SSO', 
 'Enterprise SSO integration', 'premium', false, true, 'admin', CURRENT_TIMESTAMP),
 
('5bc00e01-c0fb-4267-ae56-034c941cc753', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', 'Advanced Analytics', 'ANALYTICS', 
 'Business intelligence and analytics', 'premium', false, true, 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- CONSUMER ACCOUNTS DATA
-- =============================================

INSERT INTO consumer_accounts (
    consumer_id, company_name, account_code, primary_contact_name, primary_contact_email, 
    primary_contact_phone, primary_contact_position, secondary_contact_name, secondary_contact_email,
    secondary_contact_phone, secondary_contact_position, activated_at, subscription_end, 
    address_street, address_city, address_state, address_postal_code, address_country,
    notes, status, is_active, created_by, created_on
) VALUES 
('6f572e8c-15e3-4564-9773-3e94e4aa8da0', 'FitLife Gym & Wellness', 'FITLIFE001', 'Sarah Johnson', 'sarah.johnson@fitlifegym.com', 
 '+1-555-0101', 'General Manager', 'Tom Anderson', 'tom.anderson@fitlifegym.com',
 '+1-555-0111', 'Assistant Manager', '2024-01-15', '2025-01-15', 
 '123 Fitness Avenue', 'New York', 'NY', '10001', 'USA',
 'Premium fitness center with multiple locations', 'Active', true, 'admin', CURRENT_TIMESTAMP),
 
('26c4f2b8-3676-4234-a639-278a19b1d7f5', 'PowerHouse Fitness Centers', 'POWER002', 'Mike Thompson', 'mike.thompson@powerhousefitness.com', 
 '+1-555-0102', 'Operations Director', 'Jessica Lee', 'jessica.lee@powerhousefitness.com',
 '+1-555-0112', 'Marketing Director', '2024-02-01', '2025-02-01', 
 '456 Strength Street', 'Los Angeles', 'CA', '90210', 'USA',
 'Chain of strength and conditioning facilities', 'Active', true, 'admin', CURRENT_TIMESTAMP),
 
('bbfb978f-3155-436c-9721-70939f93f5f4', 'Zen Wellness Studio', 'ZEN003', 'Lisa Chen', 'lisa.chen@zenwellness.com', 
 '+1-555-0103', 'Founder & CEO', 'Mark Davis', 'mark.davis@zenwellness.com',
 '+1-555-0113', 'Head Instructor', '2024-02-15', '2025-02-15', 
 '789 Peaceful Lane', 'San Francisco', 'CA', '94102', 'USA',
 'Holistic wellness and mindfulness center', 'Active', true, 'admin', CURRENT_TIMESTAMP),
 
('82aea135-39c7-4caf-926f-6f0b55bed277', 'Corporate Health Solutions', 'CORP004', 'David Wilson', 'david.wilson@corphealth.com', 
 '+1-555-0104', 'Chief Technology Officer', 'Rachel Brown', 'rachel.brown@corphealth.com',
 '+1-555-0114', 'Head of Wellness Programs', '2024-03-01', '2025-03-01', 
 '321 Business Boulevard', 'Chicago', 'IL', '60601', 'USA',
 'Enterprise wellness solutions for corporations', 'Active', true, 'admin', CURRENT_TIMESTAMP),
 
('661dccf3-351b-493f-96f1-309be17135b8', 'University Recreation Center', 'UNI005', 'Dr. Amanda Rodriguez', 'amanda.rodriguez@university.edu', 
 '+1-555-0105', 'Recreation Director', 'Chris Martinez', 'chris.martinez@university.edu',
 '+1-555-0115', 'Assistant Director', '2024-03-15', '2025-03-15', 
 '555 Campus Drive', 'Austin', 'TX', '78712', 'USA',
 'University recreation and fitness facilities', 'Active', true, 'admin', CURRENT_TIMESTAMP),
 
('30822013-1cea-483f-8657-229227ce00eb', 'TechWayFit Demo Account', 'DEMO006', 'Demo User', 'demo@techwayfit.com', 
 '+1-555-0999', 'Demo Administrator', 'Test User', 'test@techwayfit.com',
 '+1-555-0998', 'Test Assistant', '2024-01-01', '2024-12-31', 
 '999 Demo Street', 'Demo City', 'DC', '00000', 'USA',
 'Demo account for testing and evaluation purposes', 'Trial', true, 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT CONSUMERS (Product-Consumer Assignments)
-- =============================================

INSERT INTO product_consumers (
    product_consumer_id, product_id, consumer_id, assigned_date, status, 
    notes, created_by, created_on
) VALUES 
-- FitLife Gym assignments
('57d39cf9-6ca6-4b9f-9b1b-0e5ba4abef9e', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '6f572e8c-15e3-4564-9773-3e94e4aa8da0', '2024-01-15', 'Active', 
 'Premium fitness solution for main location', 'admin', CURRENT_TIMESTAMP),
 
('6ed64f86-8e9c-4814-8f44-57750d6a3029', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', '6f572e8c-15e3-4564-9773-3e94e4aa8da0', '2024-02-01', 'Active', 
 'Added nutrition tracking for personal trainers', 'admin', CURRENT_TIMESTAMP),

-- PowerHouse Fitness assignments
('f3ad28b9-a9ed-4b49-b241-27c200ed3bf8', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '26c4f2b8-3676-4234-a639-278a19b1d7f5', '2024-02-01', 'Active', 
 'Multi-location fitness management', 'admin', CURRENT_TIMESTAMP),
 
('56c64afb-496e-4f41-9642-3f27f0193467', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', '26c4f2b8-3676-4234-a639-278a19b1d7f5', '2024-03-01', 'Active', 
 'Enterprise solution for franchise management', 'admin', CURRENT_TIMESTAMP),

-- Zen Wellness assignments
('63f3f3c0-6ae3-4a9c-9577-1fc67b064c7b', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'bbfb978f-3155-436c-9721-70939f93f5f4', '2024-02-15', 'Active', 
 'Holistic wellness tracking solution', 'admin', CURRENT_TIMESTAMP),

-- Corporate Health assignments
('0d7c4f2d-027c-4458-918e-af046d03ee35', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', '82aea135-39c7-4caf-926f-6f0b55bed277', '2024-03-01', 'Active', 
 'Employee wellness program management', 'admin', CURRENT_TIMESTAMP),
 
('da722493-3efe-4ff4-aa37-21fed394da8a', '5876d029-83f5-4f02-8f86-d90c83b78e85', '82aea135-39c7-4caf-926f-6f0b55bed277', '2024-03-15', 'Active', 
 'Wellness tracking for employees', 'admin', CURRENT_TIMESTAMP),

-- University assignments
('fd0b19ad-8170-408f-82ed-5283cb034319', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '661dccf3-351b-493f-96f1-309be17135b8', '2024-03-15', 'Active', 
 'Student recreation center management', 'admin', CURRENT_TIMESTAMP),
 
('fe0d76c2-86eb-4c7f-831c-f26e7324494a', '5876d029-83f5-4f02-8f86-d90c83b78e85', '661dccf3-351b-493f-96f1-309be17135b8', '2024-04-01', 'Active', 
 'Student wellness programs', 'admin', CURRENT_TIMESTAMP),

-- Demo account assignments
('52d88432-e36b-4e90-b5d8-b5a9804ba5b9', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '30822013-1cea-483f-8657-229227ce00eb', '2024-01-01', 'Trial', 
 'Demo account for testing all features', 'admin', CURRENT_TIMESTAMP),
 
('0140c531-9a65-489d-897e-6aaafe6af484', '5876d029-83f5-4f02-8f86-d90c83b78e85', '30822013-1cea-483f-8657-229227ce00eb', '2024-01-01', 'Trial', 
 'Demo wellness features', 'admin', CURRENT_TIMESTAMP),
 
('bf9bc951-60a8-46bf-beb1-c1f56d6ea295', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', '30822013-1cea-483f-8657-229227ce00eb', '2024-01-01', 'Trial', 
 'Demo nutrition tracking', 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT LICENSES DATA
-- =============================================

INSERT INTO product_licenses (
    license_id, license_code, product_id, consumer_id, valid_from, valid_to, 
    encryption, signature, license_key, public_key, license_signature, 
    key_generated_at, status, issued_by, revoked_at, revocation_reason, metadata_json,
    created_by, created_on
) VALUES 
-- FitLife Gym licenses
('c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', 'TWF-FITNESS-FITLIFE-2024-001', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '6f572e8c-15e3-4564-9773-3e94e4aa8da0', 
 '2024-01-15', '2025-01-15', 'AES256', 'SHA256',
 'TWF-FITNESS-FITLIFE-2024-A1B2C3D4E5F6-ENCODED-LICENSE-KEY-DATA-HERE',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...PUBLIC-KEY-DATA...', 
 'SIGNATURE-DATA-FOR-LICENSE-VERIFICATION-HERE',
 '2024-01-15 10:30:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "premium", "features": ["user_mgmt", "workout_track", "reporting"], "customizations": {"branding": true}}',
 'admin', CURRENT_TIMESTAMP),

('7261d3f0-4773-4715-a075-4e3a00324dd8', 'TWF-NUTRITION-FITLIFE-2024-002', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', '6f572e8c-15e3-4564-9773-3e94e4aa8da0', 
 '2024-02-01', '2025-02-01', 'AES256', 'SHA256',
 'TWF-NUTRITION-FITLIFE-2024-F6E5D4C3B2A1-ENCODED-LICENSE-KEY-DATA-HERE',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...PUBLIC-KEY-DATA...', 
 'SIGNATURE-DATA-FOR-NUTRITION-LICENSE-VERIFICATION-HERE',
 '2024-02-01 14:15:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "coach", "features": ["meal_plan", "calorie_track"], "integration": {"fitness_app": true}}',
 'admin', CURRENT_TIMESTAMP),

-- PowerHouse Fitness licenses
('9295f969-db89-4472-b074-bb96933c2f2e', 'TWF-FITNESS-POWER-2024-003', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '26c4f2b8-3676-4234-a639-278a19b1d7f5', 
 '2024-02-01', '2025-02-01', 'AES256', 'SHA256',
 'TWF-FITNESS-POWER-2024-G7H8I9J0K1L2-ENCODED-LICENSE-KEY-DATA-HERE',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...PUBLIC-KEY-DATA...', 
 'SIGNATURE-DATA-FOR-POWER-LICENSE-VERIFICATION-HERE',
 '2024-02-01 09:45:00+00', 'Active', 'admin', NULL, NULL,
 '{"tier": "premium", "features": ["user_mgmt", "workout_track", "equipment_mgmt", "reporting"], "multi_location": true}',
 'admin', CURRENT_TIMESTAMP),

('b5e94bc1-0b81-4747-ac23-ec515fb04cd5', 'TWF-ENTERPRISE-POWER-2024-004', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', '26c4f2b8-3676-4234-a639-278a19b1d7f5', 
 '2024-03-01', '2025-03-01', 'AES256', 'SHA256',
 'TWF-ENTERPRISE-POWER-2024-M3N4O5P6Q7R8-ENCODED-LICENSE-KEY-DATA-HERE',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...PUBLIC-KEY-DATA...', 
 'SIGNATURE-DATA-FOR-ENTERPRISE-LICENSE-VERIFICATION-HERE',
 '2024-03-01 11:20:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "premium", "features": ["multi_tenant", "api_access", "sso", "analytics"], "franchise_management": true}',
 'Full enterprise suite for franchise operations', 'admin', CURRENT_TIMESTAMP),

-- Zen Wellness licenses
('529a752e-1515-4236-af85-616a9f879c28', 'TWF-WELLNESS-ZEN-2024-S9T0U1V2W3X4', '5876d029-83f5-4f02-8f86-d90c83b78e85', 'bbfb978f-3155-436c-9721-70939f93f5f4', 
 '2024-02-15', '2025-02-15',
 'TWF-WELLNESS-ZEN-2024-S9T0U1V2W3X4-ENCRYPTED-LICENSE-DATA',
 'WELLNESS-ZEN-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-WELLNESS-ZEN-2024-ENCODED-LICENSE-KEY-FOR-STUDIO-OPERATIONS',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...WELLNESS-PUBLIC-KEY...', 
 'WELLNESS-ZEN-LICENSE-SIGNATURE-DATA-FOR-STUDIO-VERIFICATION',
 '2024-02-15 08:30:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "family", "features": ["health_track", "goals", "meditation"], "studio_integration": true}',
 'Wellness solution for holistic health studio', 'admin', CURRENT_TIMESTAMP),

-- Corporate Health licenses
('5724b9be-d7e2-4eb3-8987-82a27472a804', 'TWF-ENTERPRISE-CORP-2024-Y5Z6A7B8C9D0', '782d5a8a-a3c8-4e91-ac9a-ad58fcd935e3', '82aea135-39c7-4caf-926f-6f0b55bed277', 
 '2024-03-01', '2025-03-01',
 'TWF-ENTERPRISE-CORP-2024-Y5Z6A7B8C9D0-ENCRYPTED-CORPORATE-DATA',
 'ENTERPRISE-CORP-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-ENTERPRISE-CORP-2024-ENCODED-LICENSE-KEY-FOR-EMPLOYEE-WELLNESS',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...CORPORATE-PUBLIC-KEY...', 
 'ENTERPRISE-CORP-LICENSE-SIGNATURE-DATA-FOR-WELLNESS-VERIFICATION',
 '2024-03-01 09:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "standard", "features": ["multi_tenant", "api_access"], "employee_wellness": true}',
 'Corporate employee wellness program', 'admin', CURRENT_TIMESTAMP),

('02a44fdb-8971-4f6d-9b8b-50119bf994f7', 'TWF-WELLNESS-CORP-2024-E1F2G3H4I5J6', '5876d029-83f5-4f02-8f86-d90c83b78e85', '82aea135-39c7-4caf-926f-6f0b55bed277', 
 '2024-03-15', '2025-03-15',
 'TWF-WELLNESS-CORP-2024-E1F2G3H4I5J6-ENCRYPTED-WELLNESS-DATA',
 'WELLNESS-CORP-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-WELLNESS-CORP-2024-ENCODED-LICENSE-KEY-FOR-CORPORATE-TRACKING',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...WELLNESS-CORP-PUBLIC-KEY...', 
 'WELLNESS-CORP-LICENSE-SIGNATURE-DATA-FOR-EMPLOYEE-VERIFICATION',
 '2024-03-15 10:15:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "family", "features": ["health_track", "goals"], "corporate_dashboard": true}',
 'Wellness tracking for corporate employees', 'admin', CURRENT_TIMESTAMP),

-- University licenses
('67059ecb-3d52-4574-8943-b2d461f28930', 'TWF-FITNESS-UNI-2024-K7L8M9N0O1P2', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '661dccf3-351b-493f-96f1-309be17135b8', 
 '2024-03-15', '2025-03-15', 'TWF-FITNESS-UNI-2024-ENCRYPTED-UNIVERSITY-DATA',
 'FITNESS-UNI-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-FITNESS-UNI-2024-ENCODED-LICENSE-KEY-FOR-STUDENT-RECREATION',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...FITNESS-UNI-PUBLIC-KEY...', 
 'FITNESS-UNI-LICENSE-SIGNATURE-DATA-FOR-STUDENT-VERIFICATION',
 '2024-03-15 14:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "pro", "features": ["user_mgmt", "workout_track", "equipment_mgmt"], "student_discount": true}',
 'Student recreation center management', 'admin', CURRENT_TIMESTAMP),

('cbdfe031-739d-49d4-a8a5-748227b3ed6f', 'TWF-WELLNESS-UNI-2024-Q3R4S5T6U7V8', '5876d029-83f5-4f02-8f86-d90c83b78e85', '661dccf3-351b-493f-96f1-309be17135b8', 
 '2024-04-01', '2025-04-01', 'TWF-WELLNESS-UNI-2024-ENCRYPTED-WELLNESS-DATA',
 'WELLNESS-UNI-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-WELLNESS-UNI-2024-ENCODED-LICENSE-KEY-FOR-STUDENT-WELLNESS',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...WELLNESS-UNI-PUBLIC-KEY...', 
 'WELLNESS-UNI-LICENSE-SIGNATURE-DATA-FOR-STUDENT-VERIFICATION',
 '2024-04-01 09:30:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "family", "features": ["health_track", "goals"], "student_programs": true}',
 'Student wellness programs', 'admin', CURRENT_TIMESTAMP),

-- Demo licenses
('51c7b989-66c8-4a8d-9ee0-070a38261467', 'TWF-FITNESS-DEMO-2024-W9X0Y1Z2A3B4', 'b0c8c511-bc37-4103-957d-ec8084ecbb3f', '30822013-1cea-483f-8657-229227ce00eb', 
 '2024-01-01', '2025-12-31',
 'TWF-FITNESS-DEMO-2024-ENCRYPTED-DEMO-DATA',
 'FITNESS-DEMO-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-FITNESS-DEMO-2024-ENCODED-LICENSE-KEY-FOR-DEMO-GYM',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...FITNESS-DEMO-PUBLIC-KEY...', 
 'FITNESS-DEMO-LICENSE-SIGNATURE-DATA-FOR-DEMO-VERIFICATION',
 '2024-01-01 00:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "premium", "features": ["user_mgmt", "workout_track", "reporting"], "demo_mode": true}',
 'Demo license with all features enabled', 'admin', CURRENT_TIMESTAMP),

('89c94046-6183-43a0-8ee7-7461fae7daf6', 'TWF-WELLNESS-DEMO-2024-C5D6E7F8G9H0', '5876d029-83f5-4f02-8f86-d90c83b78e85', '30822013-1cea-483f-8657-229227ce00eb', 
 '2024-01-01', '2025-12-31',
 'TWF-WELLNESS-DEMO-2024-ENCRYPTED-DEMO-DATA',
 'WELLNESS-DEMO-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-WELLNESS-DEMO-2024-ENCODED-LICENSE-KEY-FOR-DEMO-WELLNESS',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...WELLNESS-DEMO-PUBLIC-KEY...', 
 'WELLNESS-DEMO-LICENSE-SIGNATURE-DATA-FOR-DEMO-VERIFICATION',
 '2024-01-01 00:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "family", "features": ["health_track", "goals", "meditation"], "demo_mode": true}',
 'Demo wellness license', 'admin', CURRENT_TIMESTAMP),

('4ee19373-8a1b-4b31-b3c4-7753a3e7246a', 'TWF-NUTRITION-DEMO-2024-I1J2K3L4M5N6', '9f4cf3fe-df54-4c5b-ab39-7ab47522512a', '30822013-1cea-483f-8657-229227ce00eb', 
 '2024-01-01', '2025-12-31',
 'TWF-NUTRITION-DEMO-2024-ENCRYPTED-DEMO-DATA',
 'NUTRITION-DEMO-SIGNATURE-DATA-FOR-VERIFICATION-HERE',
 'TWF-NUTRITION-DEMO-2024-ENCODED-LICENSE-KEY-FOR-DEMO-NUTRITION',
 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...NUTRITION-DEMO-PUBLIC-KEY...', 
 'NUTRITION-DEMO-LICENSE-SIGNATURE-DATA-FOR-DEMO-VERIFICATION',
 '2024-01-01 00:00:00+00', 'Active', 'admin', NULL, NULL, 
 '{"tier": "coach", "features": ["meal_plan", "calorie_track", "recipe_db"], "demo_mode": true}',
 'Demo nutrition license', 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- PRODUCT LICENSE FEATURES (Many-to-Many)
-- =============================================

INSERT INTO product_license_features (product_license_id, product_feature_id) VALUES 
-- FitLife Gym fitness license features
('c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', 'acafef8b-21d6-4461-960e-1385fc7d1219'),
('c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', '66a99388-ee13-467d-886d-19fbef7d6ff7'),

-- FitLife Gym nutrition license features
('7261d3f0-4773-4715-a075-4e3a00324dd8', '35bbf68c-4faa-4abd-a091-8d952b8340a4'),
('7261d3f0-4773-4715-a075-4e3a00324dd8', 'abaec6e3-6d30-40db-9990-33b87a6cdaa0'),

-- PowerHouse fitness license features
('9295f969-db89-4472-b074-bb96933c2f2e', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('9295f969-db89-4472-b074-bb96933c2f2e', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('9295f969-db89-4472-b074-bb96933c2f2e', 'bdee77e3-7caf-4196-a2b1-8087df6a8aae'),
('9295f969-db89-4472-b074-bb96933c2f2e', 'acafef8b-21d6-4461-960e-1385fc7d1219'),
('9295f969-db89-4472-b074-bb96933c2f2e', '66a99388-ee13-467d-886d-19fbef7d6ff7'),

-- PowerHouse enterprise license features
('b5e94bc1-0b81-4747-ac23-ec515fb04cd5', '287c09e3-cb38-4a5f-ac9b-afe3e4a74216'),
('b5e94bc1-0b81-4747-ac23-ec515fb04cd5', '8fbe1644-dbde-4666-a6a5-26fd78b469ae'),
('b5e94bc1-0b81-4747-ac23-ec515fb04cd5', '30556d75-46a4-486f-bc2d-eb9601ae46b0'),
('b5e94bc1-0b81-4747-ac23-ec515fb04cd5', '5bc00e01-c0fb-4267-ae56-034c941cc753'),

-- Zen wellness license features
('529a752e-1515-4236-af85-616a9f879c28', '5632b479-fb02-460c-8d11-a395f8a81146'),
('529a752e-1515-4236-af85-616a9f879c28', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),
('529a752e-1515-4236-af85-616a9f879c28', 'b8952f1b-62f7-41e7-b14e-794abfeb16c8'),

-- Corporate enterprise license features
('5724b9be-d7e2-4eb3-8987-82a27472a804', '287c09e3-cb38-4a5f-ac9b-afe3e4a74216'),
('5724b9be-d7e2-4eb3-8987-82a27472a804', '8fbe1644-dbde-4666-a6a5-26fd78b469ae'),

-- Corporate wellness license features
('02a44fdb-8971-4f6d-9b8b-50119bf994f7', '5632b479-fb02-460c-8d11-a395f8a81146'),
('02a44fdb-8971-4f6d-9b8b-50119bf994f7', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),

-- University fitness license features
('67059ecb-3d52-4574-8943-b2d461f28930', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('67059ecb-3d52-4574-8943-b2d461f28930', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('67059ecb-3d52-4574-8943-b2d461f28930', 'bdee77e3-7caf-4196-a2b1-8087df6a8aae'),

-- University wellness license features
('cbdfe031-739d-49d4-a8a5-748227b3ed6f', '5632b479-fb02-460c-8d11-a395f8a81146'),
('cbdfe031-739d-49d4-a8a5-748227b3ed6f', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),

-- Demo licenses features (all features)
('51c7b989-66c8-4a8d-9ee0-070a38261467', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('51c7b989-66c8-4a8d-9ee0-070a38261467', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('51c7b989-66c8-4a8d-9ee0-070a38261467', 'bdee77e3-7caf-4196-a2b1-8087df6a8aae'),
('51c7b989-66c8-4a8d-9ee0-070a38261467', 'acafef8b-21d6-4461-960e-1385fc7d1219'),
('51c7b989-66c8-4a8d-9ee0-070a38261467', '66a99388-ee13-467d-886d-19fbef7d6ff7'),

('89c94046-6183-43a0-8ee7-7461fae7daf6', '5632b479-fb02-460c-8d11-a395f8a81146'),
('89c94046-6183-43a0-8ee7-7461fae7daf6', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),
('89c94046-6183-43a0-8ee7-7461fae7daf6', 'b8952f1b-62f7-41e7-b14e-794abfeb16c8'),

('4ee19373-8a1b-4b31-b3c4-7753a3e7246a', '35bbf68c-4faa-4abd-a091-8d952b8340a4'),
('4ee19373-8a1b-4b31-b3c4-7753a3e7246a', 'abaec6e3-6d30-40db-9990-33b87a6cdaa0'),
('4ee19373-8a1b-4b31-b3c4-7753a3e7246a', '4cd37113-728b-4bb9-88a0-37251062d523');

-- =============================================
-- PRODUCT TIER FEATURES (Many-to-Many)
-- =============================================

INSERT INTO product_tier_features (product_tier_id, product_feature_id) VALUES 
-- Fitness Basic tier features
('28d421cd-e028-4857-b4c9-7768060f2069', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('28d421cd-e028-4857-b4c9-7768060f2069', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),

-- Fitness Pro tier features
('8b9ebb67-d001-4c6d-b5b9-0314076c3fed', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('8b9ebb67-d001-4c6d-b5b9-0314076c3fed', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('8b9ebb67-d001-4c6d-b5b9-0314076c3fed', 'bdee77e3-7caf-4196-a2b1-8087df6a8aae'),
('8b9ebb67-d001-4c6d-b5b9-0314076c3fed', '66a99388-ee13-467d-886d-19fbef7d6ff7'),

-- Fitness Premium tier features
('751ea743-62a1-4427-a754-29ad3c0e76fb', '42c71a50-49e6-450f-80fb-b83b1d20c2ae'),
('751ea743-62a1-4427-a754-29ad3c0e76fb', '77cb8b77-9db7-486f-9cfa-6774e01e6635'),
('751ea743-62a1-4427-a754-29ad3c0e76fb', 'bdee77e3-7caf-4196-a2b1-8087df6a8aae'),
('751ea743-62a1-4427-a754-29ad3c0e76fb', 'acafef8b-21d6-4461-960e-1385fc7d1219'),
('751ea743-62a1-4427-a754-29ad3c0e76fb', '66a99388-ee13-467d-886d-19fbef7d6ff7'),

-- Wellness Starter tier features
('f786670c-9db0-4128-a9b0-bdd2b5a8b525', '5632b479-fb02-460c-8d11-a395f8a81146'),
('f786670c-9db0-4128-a9b0-bdd2b5a8b525', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),

-- Wellness Family tier features
('5b971186-9b37-4c62-866f-91097e594ab4', '5632b479-fb02-460c-8d11-a395f8a81146'),
('5b971186-9b37-4c62-866f-91097e594ab4', '87ebf873-5b74-4148-96d3-bbf23f2f79e7'),
('5b971186-9b37-4c62-866f-91097e594ab4', 'b8952f1b-62f7-41e7-b14e-794abfeb16c8'),

-- Nutrition Personal tier features
('b5276e8b-fd12-4e0b-b86e-faae5b9fcbc7', '35bbf68c-4faa-4abd-a091-8d952b8340a4'),
('b5276e8b-fd12-4e0b-b86e-faae5b9fcbc7', 'abaec6e3-6d30-40db-9990-33b87a6cdaa0'),

-- Nutrition Coach tier features
('23743e1b-1348-4c11-875e-1564b6dc5534', '35bbf68c-4faa-4abd-a091-8d952b8340a4'),
('23743e1b-1348-4c11-875e-1564b6dc5534', 'abaec6e3-6d30-40db-9990-33b87a6cdaa0'),
('23743e1b-1348-4c11-875e-1564b6dc5534', '4cd37113-728b-4bb9-88a0-37251062d523'),

-- Enterprise Standard tier features
('2df322a2-acc7-4eb6-a0a1-e6a0cc21094a', '287c09e3-cb38-4a5f-ac9b-afe3e4a74216'),
('2df322a2-acc7-4eb6-a0a1-e6a0cc21094a', '8fbe1644-dbde-4666-a6a5-26fd78b469ae'),

-- Enterprise Premium tier features
('b55d151c-f29b-493f-a86f-2f09e9b19025', '287c09e3-cb38-4a5f-ac9b-afe3e4a74216'),
('b55d151c-f29b-493f-a86f-2f09e9b19025', '8fbe1644-dbde-4666-a6a5-26fd78b469ae'),
('b55d151c-f29b-493f-a86f-2f09e9b19025', '30556d75-46a4-486f-bc2d-eb9601ae46b0'),
('b55d151c-f29b-493f-a86f-2f09e9b19025', '5bc00e01-c0fb-4267-ae56-034c941cc753');

-- =============================================
-- NOTIFICATION TEMPLATES DATA
-- =============================================

INSERT INTO notification_templates (
    notification_template_id, template_name, message_template, notification_type, 
    subject, template_variable_json, is_active, notification_mode, created_by, created_on
) VALUES 
('e1b62661-37be-4bc7-9511-1391a6718a34', 'Welcome Email', 
 'Welcome to TechWayFit, {{customer_name}}! Your {{product_name}} license is now active. License Key: {{license_key}}. Valid until: {{expiry_date}}.', 
 'License', 'Welcome to TechWayFit - Your License is Active', 
 '{"customer_name": "string", "product_name": "string", "license_key": "string", "expiry_date": "date"}', 
 true, 'Email', 'admin', CURRENT_TIMESTAMP),

('3be980f7-3257-427c-9464-a57dcc42be88', 'License Expiry Warning', 
 'Dear {{customer_name}}, your {{product_name}} license will expire on {{expiry_date}}. Please contact us to renew your subscription.', 
 'Warning', 'License Expiry Warning - Action Required', 
 '{"customer_name": "string", "product_name": "string", "expiry_date": "date"}', 
 true, 'Email', 'admin', CURRENT_TIMESTAMP),

('0a383ec1-933a-416a-bde4-1a3b69d84beb', 'License Activation', 
 'Your {{product_name}} license has been successfully activated. You can now access all features included in your {{tier_name}} plan.', 
 'Activation', 'License Successfully Activated', 
 '{"product_name": "string", "tier_name": "string", "features": "array"}', 
 true, 'Email', 'admin', CURRENT_TIMESTAMP),

('e31c2486-ddd9-4025-a26d-a42659282de5', 'Support Request Acknowledgment', 
 'Hello {{customer_name}}, we have received your support request for {{product_name}}. Our team will respond within 24 hours. Ticket #{{ticket_id}}.', 
 'Support', 'Support Request Received - Ticket #{{ticket_id}}', 
 '{"customer_name": "string", "product_name": "string", "ticket_id": "string"}', 
 true, 'Email', 'admin', CURRENT_TIMESTAMP);

-- =============================================
-- NOTIFICATION HISTORY DATA
-- =============================================

INSERT INTO notification_history (
    notification_id, entity_id, entity_type, recipients_json, notification_mode, 
    notification_template_id, notification_type, sent_date, delivery_status, 
    created_by, created_on
) VALUES 
-- Welcome notifications for new licenses
('cec0ba80-a9a4-4dde-94dd-f9e8f91584c2', 'c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', 'License', 
 '["sarah.johnson@fitlifegym.com"]', 'Email', 'e1b62661-37be-4bc7-9511-1391a6718a34', 'License', 
 '2024-01-15 10:30:00', 'Delivered', 'system', CURRENT_TIMESTAMP),

('7f933bdc-a0bf-4a69-9010-59dde53c3fc6', '9295f969-db89-4472-b074-bb96933c2f2e', 'License', 
 '["mike.thompson@powerhousefitness.com"]', 'Email', 'e1b62661-37be-4bc7-9511-1391a6718a34', 'License', 
 '2024-02-01 09:15:00', 'Delivered', 'system', CURRENT_TIMESTAMP),

('faf80e46-9b1c-446e-8b1a-9268bb00bb36', '529a752e-1515-4236-af85-616a9f879c28', 'License', 
 '["lisa.chen@zenwellness.com"]', 'Email', 'e1b62661-37be-4bc7-9511-1391a6718a34', 'License', 
 '2024-02-15 14:20:00', 'Delivered', 'system', CURRENT_TIMESTAMP),

-- License activation notifications
('aa4aa478-912f-4e67-b7c0-24ade61abaf3', '5724b9be-d7e2-4eb3-8987-82a27472a804', 'License', 
 '["david.wilson@corphealth.com"]', 'Email', '0a383ec1-933a-416a-bde4-1a3b69d84beb', 'Activation', 
 '2024-03-01 11:45:00', 'Delivered', 'system', CURRENT_TIMESTAMP),

('39ec2ab6-864f-4d08-a46b-eacab1f8b34c', '67059ecb-3d52-4574-8943-b2d461f28930', 'License', 
 '["amanda.rodriguez@university.edu"]', 'Email', '0a383ec1-933a-416a-bde4-1a3b69d84beb', 'Activation', 
 '2024-03-15 16:30:00', 'Delivered', 'system', CURRENT_TIMESTAMP),

-- Some failed delivery examples
('7109b531-a02a-4ae7-bb7a-43df213c73a3', '51c7b989-66c8-4a8d-9ee0-070a38261467', 'License', 
 '["demo@techwayfit.com"]', 'Email', 'e1b62661-37be-4bc7-9511-1391a6718a34', 'License', 
 '2024-01-01 12:00:00', 'Failed', 'system', CURRENT_TIMESTAMP);

-- =============================================
-- AUDIT ENTRIES DATA
-- =============================================

INSERT INTO audit_entries (
    audit_entry_id, entity_type, entity_id, operation_type, changes_json, 
    user_id, timestamp, additional_data_json, created_by, created_on
) VALUES 
('de8de0ae-bb23-46b2-8623-d2effe0d8a2e', 'ConsumerAccount', '6f572e8c-15e3-4564-9773-3e94e4aa8da0', 'CREATE', 
 '{"action": "create_consumer", "data": {"company_name": "FitLife Gym & Wellness"}}', 
 'admin', '2024-01-15 10:00:00', '{"ip_address": "192.168.1.100"}', 'admin', CURRENT_TIMESTAMP),

('bd262b1c-f91f-4e75-99f3-baf3b030773f', 'ProductLicense', 'c94d7f06-fdb6-4fd9-9485-8b7467dbddb2', 'CREATE', 
 '{"action": "create_license", "data": {"product": "TWF-FITNESS", "consumer": "FITLIFE001"}}', 
 'admin', '2024-01-15 10:30:00', '{"ip_address": "192.168.1.100"}', 'admin', CURRENT_TIMESTAMP),

('39a29d41-daa3-41a3-8152-db507f0757f2', 'ConsumerAccount', '26c4f2b8-3676-4234-a639-278a19b1d7f5', 'CREATE', 
 '{"action": "create_consumer", "data": {"company_name": "PowerHouse Fitness Centers"}}', 
 'admin', '2024-02-01 09:00:00', '{"ip_address": "192.168.1.101"}', 'admin', CURRENT_TIMESTAMP),

('2192187b-4ba8-4637-93db-1a2d5f1de246', 'ProductLicense', 'b5e94bc1-0b81-4747-ac23-ec515fb04cd5', 'CREATE', 
 '{"action": "create_license", "data": {"product": "TWF-ENTERPRISE", "consumer": "POWER002"}}', 
 'admin', '2024-03-01 11:45:00', '{"ip_address": "192.168.1.102"}', 'admin', CURRENT_TIMESTAMP),

('4894548f-bb6f-4cdf-a80a-bd372e0ab4a4', 'ConsumerAccount', 'bbfb978f-3155-436c-9721-70939f93f5f4', 'UPDATE', 
 '{"action": "update_contact", "old_email": "old@zenwellness.com", "new_email": "lisa.chen@zenwellness.com"}', 
 'admin', '2024-02-20 15:30:00', '{"ip_address": "192.168.1.103"}', 'admin', CURRENT_TIMESTAMP);

-- Commit all changes
COMMIT;

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Display summary of inserted data
SELECT 'Data Summary' as info, 
       (SELECT COUNT(*) FROM products) as products_count,
       (SELECT COUNT(*) FROM product_versions) as versions_count,
       (SELECT COUNT(*) FROM product_tiers) as tiers_count,
       (SELECT COUNT(*) FROM product_features) as features_count,
       (SELECT COUNT(*) FROM consumer_accounts) as consumers_count,
       (SELECT COUNT(*) FROM product_consumers) as assignments_count,
       (SELECT COUNT(*) FROM product_licenses) as licenses_count,
       (SELECT COUNT(*) FROM notification_templates) as templates_count,
       (SELECT COUNT(*) FROM notification_history) as notifications_count,
       (SELECT COUNT(*) FROM audit_entries) as audit_entries_count;

-- Verify active licenses view
SELECT 'Active Licenses View Test' as info;
SELECT company_name, product_name, license_key, status, valid_to 
FROM active_licenses_view 
ORDER BY valid_to 
LIMIT 3;

-- Test a sample query to ensure relationships work
SELECT 'Relationship Test' as info;
SELECT ca.company_name, pl.license_key, p.name as product_name 
FROM consumer_accounts ca
JOIN product_licenses pl ON ca.consumer_id = pl.consumer_id
JOIN products p ON pl.product_id = p.product_id
WHERE ca.status = 'Active'
LIMIT 3;
LIMIT 5;

-- Script completed successfully
SELECT '✅ Sample data insertion completed successfully!' as status,
       'All tables populated with realistic test data' as description,
       'Ready for application testing' as next_step;
