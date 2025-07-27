-- TechWayFit Licensing Management System - Sample Data Script
-- Version: 1.0.0-corrected - Aligned with corrected schema
-- This script populates all tables with realistic test data
-- Dataset: 3 products, 5 consumers, with realistic relationships
-- Run this after creating the database schema

-- Start transaction
BEGIN;

-- =============================================
-- PRODUCTS DATA (3 products)
-- =============================================
INSERT INTO products (
    product_id, name, description, release_date, support_email, support_phone, 
    decommission_date, status, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'TechWayFit Fitness Pro', 
 'Complete fitness management solution for gyms and wellness centers', 
 '2024-01-15 10:00:00+00', 'support@techwayfit.com', '+1-555-0100', 
 NULL, 'Active', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'TechWayFit Wellness Suite', 
 'Comprehensive wellness tracking and management platform', 
 '2024-02-20 14:00:00+00', 'wellness@techwayfit.com', '+1-555-0200', 
 NULL, 'Active', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

('6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'TechWayFit Enterprise', 
 'Enterprise-grade fitness and wellness management solution', 
 '2024-03-10 09:00:00+00', 'enterprise@techwayfit.com', '+1-555-0300', 
 NULL, 'Active', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT VERSIONS DATA (3-4 versions per product)
-- =============================================
INSERT INTO product_versions (
    version_id, product_id, version, name, release_date, end_of_life_date, 
    support_end_date, release_notes, is_current, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
-- Fitness Pro versions
('123e4567-e89b-12d3-a456-426614174000', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.0.0', 'Initial Release', 
 '2024-01-15 10:00:00+00', NULL, NULL, 'First stable release with core fitness tracking', false, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('987fcdeb-51d2-4a8b-9c45-123456789abc', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.1.0', 'Feature Enhancement', 
 '2024-04-01 10:00:00+00', NULL, NULL, 'Added advanced reporting and member analytics', false, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('c9bf9e57-1685-4c89-bafb-ff5af830be8a', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.2.0', 'Mobile Integration', 
 '2024-06-15 10:00:00+00', NULL, NULL, 'Mobile app integration and real-time sync', true, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Suite versions  
('15f8b2c1-4d6e-4a2b-8f3c-1e4d5a6b7c8d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '1.0.0', 'Beta Release', 
 '2024-02-20 14:00:00+00', NULL, NULL, 'Beta version with wellness tracking features', false, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('2a5b8c3d-6e9f-4a1b-9c2d-5e6f7a8b9c0d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '1.1.0', 'Stability Release', 
 '2024-05-10 14:00:00+00', NULL, NULL, 'Improved stability and performance optimizations', false, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('3b6c9d4e-7f0a-5b2c-ad3e-6f7a8b9c0d1e', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '2.0.0', 'Major Update', 
 '2024-07-01 14:00:00+00', NULL, NULL, 'Complete UI overhaul and new wellness modules', true, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Enterprise versions
('4c7d0e5f-8a1b-6c3d-be4f-7a8b9c0d1e2f', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', '1.0.0', 'Enterprise Launch', 
 '2024-03-10 09:00:00+00', NULL, NULL, 'Initial enterprise release with multi-tenant support', false, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('5d8e1f6a-9b2c-7d4e-cf5a-8b9c0d1e2f3a', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', '1.1.0', 'Security Enhancement', 
 '2024-05-20 09:00:00+00', NULL, NULL, 'Enhanced security features and compliance tools', true, true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT TIERS DATA (2-3 tiers per product)
-- =============================================
INSERT INTO product_tiers (
    tier_id, product_id, name, description, price, display_order, support_sla_json, 
    is_active, created_by, updated_by, created_on, updated_on
) VALUES 
-- Fitness Pro tiers
('6e9f2a7b-ac3d-8e5f-da6b-9c0d1e2f3a4b', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Basic', 
 'Essential fitness tracking for small gyms', '99.99', 1, '{"response_time": "24h", "support_hours": "business"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('7f0a3b8c-bd4e-9f6a-eb7c-0d1e2f3a4b5c', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Professional', 
 'Advanced features for medium-sized fitness centers', '199.99', 2, '{"response_time": "12h", "support_hours": "extended"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('8a1b4c9d-ce5f-0a7b-fc8d-1e2f3a4b5c6d', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Enterprise', 
 'Full-featured solution for large gym chains', '399.99', 3, '{"response_time": "4h", "support_hours": "24/7"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Suite tiers
('9b2c5d0e-df6a-1b8c-ad9e-2f3a4b5c6d7e', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Starter', 
 'Basic wellness tracking for individuals', '49.99', 1, '{"response_time": "48h", "support_hours": "business"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ac3d6e1f-ea7b-2c9d-be0f-3a4b5c6d7e8f', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Premium', 
 'Comprehensive wellness management for organizations', '149.99', 2, '{"response_time": "8h", "support_hours": "extended"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Enterprise tiers
('bd4e7f2a-fb8c-3d0e-cf1a-4b5c6d7e8f9a', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'Standard', 
 'Multi-location fitness management', '599.99', 1, '{"response_time": "2h", "support_hours": "24/7"}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ce5f8a3b-ac9d-4e1f-da2b-5c6d7e8f9a0b', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'Ultimate', 
 'Complete enterprise solution with all features', '999.99', 2, '{"response_time": "1h", "support_hours": "24/7", "dedicated_manager": true}', 
 true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

-- =============================================
-- PRODUCT FEATURES DATA (5, 7, 8 features respectively)
-- =============================================
INSERT INTO product_features (
    feature_id, product_id, tier_id, name, description, code, is_enabled, display_order, 
    support_from_version, support_to_version, feature_usage_json, is_active, created_by, updated_by, created_on, updated_on
) VALUES 
-- Fitness Pro features (5 total)
('ce1fea9b-ac5d-0e7f-da8b-1c2d3e4f5a6b', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '6e9f2a7b-ac3d-8e5f-da6b-9c0d1e2f3a4b', 
 'Member Management', 'Basic member registration and profile management', 'MEMBER_MGMT', true, 1, '1.0.0', '9999.0.0', 
 '{"max_members": 500}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('df2afbac-bd6e-1f8a-eb9c-2d3e4f5a6b7c', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '6e9f2a7b-ac3d-8e5f-da6b-9c0d1e2f3a4b', 
 'Workout Tracking', 'Track member workouts and exercise routines', 'WORKOUT_TRACK', true, 2, '1.0.0', '9999.0.0', 
 '{"exercise_library": "basic"}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ea3bacbd-ce7f-2a9b-fcad-3e4f5a6b7c8d', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '7f0a3b8c-bd4e-9f6a-eb7c-0d1e2f3a4b5c', 
 'Progress Analytics', 'Advanced analytics and progress tracking', 'ANALYTICS', true, 3, '1.1.0', '9999.0.0', 
 '{"reports": "advanced", "charts": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('fb4cbdce-df8a-3bac-adbe-4f5a6b7c8d9e', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '7f0a3b8c-bd4e-9f6a-eb7c-0d1e2f3a4b5c', 
 'Class Scheduling', 'Schedule and manage fitness classes', 'CLASS_SCHEDULE', true, 4, '1.0.0', '9999.0.0', 
 '{"max_classes": 50}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ac5dcedf-ea9b-4cbd-becf-5a6b7c8d9e0f', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '8a1b4c9d-ce5f-0a7b-fc8d-1e2f3a4b5c6d', 
 'Multi-Location Support', 'Manage multiple gym locations from single dashboard', 'MULTI_LOCATION', true, 5, '1.1.0', '9999.0.0', 
 '{"max_locations": 20}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Wellness Suite features (7 total)
('bd6edfea-fb0c-5dce-cfda-6b7c8d9e0f1a', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '9b2c5d0e-df6a-1b8c-ad9e-2f3a4b5c6d7e', 
 'Health Metrics', 'Track basic health metrics like weight, BMI', 'HEALTH_METRICS', true, 1, '1.0.0', '9999.0.0', 
 '{"metrics": ["weight", "bmi", "body_fat"]}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ce7feafb-ac1d-6edf-daeb-7c8d9e0f1a2b', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '9b2c5d0e-df6a-1b8c-ad9e-2f3a4b5c6d7e', 
 'Nutrition Tracking', 'Log meals and track nutritional intake', 'NUTRITION', true, 2, '1.0.0', '9999.0.0', 
 '{"food_database": "basic"}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('df8afbac-bd2e-7fea-ebfc-8d9e0f1a2b3c', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '9b2c5d0e-df6a-1b8c-ad9e-2f3a4b5c6d7e', 
 'Sleep Monitoring', 'Track sleep patterns and quality', 'SLEEP_TRACK', true, 3, '1.1.0', '9999.0.0', 
 '{"sleep_phases": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ea9bacbd-ce3f-8afb-fcad-9e0f1a2b3c4d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'ac3d6e1f-ea7b-2c9d-be0f-3a4b5c6d7e8f', 
 'Stress Management', 'Stress level monitoring and management tools', 'STRESS_MGMT', true, 4, '2.0.0', '9999.0.0', 
 '{"meditation_guides": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('fb0cbdce-df4a-9bac-adbe-0f1a2b3c4d5e', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'ac3d6e1f-ea7b-2c9d-be0f-3a4b5c6d7e8f', 
 'Goal Setting', 'Set and track wellness goals', 'GOAL_SETTING', true, 5, '1.0.0', '9999.0.0', 
 '{"goal_types": ["fitness", "nutrition", "wellness"]}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ac1dcedf-ea5b-acbd-becf-1a2b3c4d5e6f', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'ac3d6e1f-ea7b-2c9d-be0f-3a4b5c6d7e8f', 
 'Community Features', 'Social features and community challenges', 'COMMUNITY', true, 6, '2.0.0', '9999.0.0', 
 '{"challenges": true, "leaderboards": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('bd2edfea-fb6c-bdce-cfda-2b3c4d5e6f7a', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'ac3d6e1f-ea7b-2c9d-be0f-3a4b5c6d7e8f', 
 'Integration Hub', 'Connect with wearables and health apps', 'INTEGRATIONS', true, 7, '2.0.0', '9999.0.0', 
 '{"supported_devices": ["fitbit", "apple_health", "google_fit"]}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),

-- Enterprise features (8 total)
('ce3feafb-ac7d-cedf-daeb-3c4d5e6f7a8b', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'bd4e7f2a-fb8c-3d0e-cf1a-4b5c6d7e8f9a', 
 'Multi-Tenant Management', 'Manage multiple organizations in single instance', 'MULTI_TENANT', true, 1, '1.0.0', '9999.0.0', 
 '{"max_tenants": 100}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('df4afbac-bd8e-dfea-ebfc-4d5e6f7a8b9c', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'bd4e7f2a-fb8c-3d0e-cf1a-4b5c6d7e8f9a', 
 'Advanced Security', 'Enterprise-grade security and compliance features', 'SECURITY', true, 2, '1.0.0', '9999.0.0', 
 '{"sso": true, "rbac": true, "audit_logs": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ea5bacbd-ce9f-eafb-fcad-5e6f7a8b9c0d', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'bd4e7f2a-fb8c-3d0e-cf1a-4b5c6d7e8f9a', 
 'API Access', 'RESTful API for custom integrations', 'API_ACCESS', true, 3, '1.0.0', '9999.0.0', 
 '{"rate_limit": 10000, "endpoints": "full"}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('fb6cbdce-df0a-fbac-adbe-6f7a8b9c0d1e', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'bd4e7f2a-fb8c-3d0e-cf1a-4b5c6d7e8f9a', 
 'Advanced Reporting', 'Executive dashboards and detailed analytics', 'ADV_REPORTING', true, 4, '1.0.0', '9999.0.0', 
 '{"custom_reports": true, "scheduled_reports": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ac7dcedf-ea1b-acbd-becf-7a8b9c0d1e2f', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ce5f8a3b-ac9d-4e1f-da2b-5c6d7e8f9a0b', 
 'White Labeling', 'Custom branding and white-label solutions', 'WHITE_LABEL', true, 5, '1.1.0', '9999.0.0', 
 '{"custom_branding": true, "custom_domain": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('bd8edfea-fb2c-bdce-cfda-8b9c0d1e2f3a', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ce5f8a3b-ac9d-4e1f-da2b-5c6d7e8f9a0b', 
 'Dedicated Support', '24/7 dedicated support with account manager', 'DEDICATED_SUPPORT', true, 6, '1.0.0', '9999.0.0', 
 '{"dedicated_manager": true, "priority_support": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('ce9feafb-ac3d-cedf-daeb-9c0d1e2f3a4b', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ce5f8a3b-ac9d-4e1f-da2b-5c6d7e8f9a0b', 
 'Data Export', 'Complete data export and migration tools', 'DATA_EXPORT', true, 7, '1.0.0', '9999.0.0', 
 '{"formats": ["csv", "json", "xml"], "scheduling": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL),
('df0afbac-bd4e-dfea-ebfc-0d1e2f3a4b5c', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'ce5f8a3b-ac9d-4e1f-da2b-5c6d7e8f9a0b', 
 'Custom Development', 'Custom feature development and implementation', 'CUSTOM_DEV', true, 8, '1.1.0', '9999.0.0', 
 '{"hours_included": 40, "priority_development": true}', true, 'admin', NULL, CURRENT_TIMESTAMP, NULL);

COMMIT;
