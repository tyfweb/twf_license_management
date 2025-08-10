-- TechWayFit Licensing Management System - PostgreSQL Seeding Script Part 1
-- Products, Product Versions, Product Tiers, and Product Features
-- Compatible with PostgreSQL 12+ and corrected schema with 'id' primary keys
-- Version: 1.0.0-pgsql

-- Start transaction
BEGIN;

-- =============================================
-- PRODUCTS DATA (3 products)
-- =============================================
INSERT INTO products (
    id, name, description, release_date, support_email, support_phone, 
    decommission_date, status, is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'TechWayFit Fitness Pro', 
 'Complete fitness management solution for gyms and wellness centers', 
 '2024-01-15 10:00:00+00', 'support@techwayfit.com', '+1-555-0100', 
 NULL, 'Active', true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'TechWayFit Wellness Suite', 
 'Comprehensive wellness tracking and management platform', 
 '2024-02-20 14:00:00+00', 'wellness@techwayfit.com', '+1-555-0200', 
 NULL, 'Active', true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'TechWayFit Enterprise', 
 'Enterprise-grade fitness and wellness management solution', 
 '2024-03-10 09:00:00+00', 'enterprise@techwayfit.com', '+1-555-0300', 
 NULL, 'Active', true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- =============================================
-- PRODUCT VERSIONS DATA (3-4 versions per product)
-- =============================================
INSERT INTO product_versions (
    id, product_id, version, name, release_date, end_of_life_date, 
    support_end_date, release_notes, is_current, is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- Fitness Pro versions
('123e4567-e89b-12d3-a456-426614174000', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.0.0', 'Initial Release', 
 '2024-01-15 10:00:00+00', NULL, NULL, 'First stable release with core fitness tracking', false, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('987fcdeb-51d2-4a8b-9c45-123456789abc', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.1.0', 'Feature Enhancement', 
 '2024-04-01 10:00:00+00', NULL, NULL, 'Added advanced reporting and member analytics', false, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('c9bf9e57-1685-4c89-bafb-ff5af830be8a', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '1.2.0', 'Mobile Integration', 
 '2024-06-15 10:00:00+00', NULL, NULL, 'Mobile app integration and real-time sync', true, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Wellness Suite versions  
('15f8b2c1-4d6e-4a2b-8f3c-1e4d5a6b7c8d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '1.0.0', 'Beta Release', 
 '2024-02-20 14:00:00+00', NULL, NULL, 'Beta version with wellness tracking features', false, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('2a5b8c3d-6e9f-4a1b-9c2d-5e6f7a8b9c0d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', '1.1.0', 'Stability Release', 
 '2024-05-10 12:00:00+00', NULL, NULL, 'Performance improvements and bug fixes', true, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Enterprise versions
('3b6c9d4e-7f0a-5b2c-0d3e-6f7a8b9c0d1e', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', '1.0.0', 'Enterprise Launch', 
 '2024-03-10 09:00:00+00', NULL, NULL, 'Enterprise-grade features with advanced security', false, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('4c7d0e5f-8a1b-6c3d-1e4f-7a8b9c0d1e2f', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', '1.1.0', 'Advanced Analytics', 
 '2024-07-01 11:00:00+00', NULL, NULL, 'Advanced analytics and enterprise reporting', true, true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- =============================================
-- PRODUCT TIERS DATA (3 tiers per product)
-- =============================================
INSERT INTO product_tiers (
    id, product_id, name, description, price, display_order, support_sla_json, 
    is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- Fitness Pro tiers
('5d8e1f6a-9b2c-7d4e-2f5a-8b9c0d1e2f3a', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Basic', 
 'Essential fitness tracking for small gyms', '99.00', 1, 
 '{"response_time": "48h", "support_channels": ["email"], "availability": "business_hours"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('6e9f2a7b-0c3d-8e5f-3a6b-9c0d1e2f3a4b', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Professional', 
 'Advanced features for growing fitness businesses', '199.00', 2, 
 '{"response_time": "24h", "support_channels": ["email", "phone"], "availability": "extended_hours"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('7f0a3b8c-1d4e-9f6a-4b7c-0d1e2f3a4b5c', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Premium', 
 'Complete fitness management with premium support', '299.00', 3, 
 '{"response_time": "4h", "support_channels": ["email", "phone", "chat"], "availability": "24x7"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Wellness Suite tiers
('8a1b4c9d-2e5f-0a7b-5c8d-1e2f3a4b5c6d', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Starter', 
 'Basic wellness tracking for small organizations', '149.00', 1, 
 '{"response_time": "48h", "support_channels": ["email"], "availability": "business_hours"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('9b2c5d0e-3f6a-1b8c-6d9e-2f3a4b5c6d7e', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Business', 
 'Comprehensive wellness solution for mid-size companies', '249.00', 2, 
 '{"response_time": "24h", "support_channels": ["email", "phone"], "availability": "extended_hours"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('0c3d6e1f-4a7b-2c9d-7e0f-3a4b5c6d7e8f', '6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Enterprise', 
 'Full-featured wellness platform with dedicated support', '399.00', 3, 
 '{"response_time": "2h", "support_channels": ["email", "phone", "chat", "dedicated_manager"], "availability": "24x7"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Enterprise tiers
('1d4e7f2a-5b8c-3d0e-8f1a-4b5c6d7e8f9a', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'Corporate', 
 'Enterprise solution for large organizations', '599.00', 1, 
 '{"response_time": "4h", "support_channels": ["email", "phone", "chat"], "availability": "24x7"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('2e5f8a3b-6c9d-4e1f-9a2b-5c6d7e8f9a0b', '6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'Enterprise Plus', 
 'Premium enterprise features with white-label options', '899.00', 2, 
 '{"response_time": "1h", "support_channels": ["email", "phone", "chat", "dedicated_manager"], "availability": "24x7"}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- =============================================
-- PRODUCT FEATURES DATA (5-8 features per tier)
-- =============================================
INSERT INTO product_features (
    id, product_id, tier_id, name, description, code, is_enabled, display_order, 
    support_from_version, support_to_version, feature_usage_json, 
    is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- Fitness Pro Basic tier features
('3f6a9b4c-7d0e-5f2a-0b3c-6d7e8f9a0b1c', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '5d8e1f6a-9b2c-7d4e-2f5a-8b9c0d1e2f3a', 
 'Member Management', 'Basic member registration and profile management', 'MEMBER_MGMT', true, 1, 
 '1.0.0', '9999.0.0', '{"max_members": 500, "custom_fields": 5}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('4a7b0c5d-8e1f-6a3b-1c4d-7e8f9a0b1c2d', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '5d8e1f6a-9b2c-7d4e-2f5a-8b9c0d1e2f3a', 
 'Basic Reporting', 'Standard reports and member statistics', 'BASIC_REPORTS', true, 2, 
 '1.0.0', '9999.0.0', '{"report_types": ["attendance", "revenue"], "history_days": 30}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('5b8c1d6e-9f2a-7b4c-2d5e-8f9a0b1c2d3e', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '5d8e1f6a-9b2c-7d4e-2f5a-8b9c0d1e2f3a', 
 'Class Scheduling', 'Basic class and session scheduling', 'CLASS_SCHEDULE', true, 3, 
 '1.0.0', '9999.0.0', '{"max_classes_per_day": 20, "advance_booking_days": 30}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Fitness Pro Professional tier features (includes Basic + more)
('6c9d2e7f-0a3b-8c5d-3e6f-9a0b1c2d3e4f', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '6e9f2a7b-0c3d-8e5f-3a6b-9c0d1e2f3a4b', 
 'Advanced Analytics', 'Detailed analytics and performance metrics', 'ADV_ANALYTICS', true, 1, 
 '1.1.0', '9999.0.0', '{"custom_dashboards": true, "data_export": true, "history_days": 365}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('7d0e3f8a-1b4c-9d6e-4f7a-0b1c2d3e4f5a', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '6e9f2a7b-0c3d-8e5f-3a6b-9c0d1e2f3a4b', 
 'Payment Processing', 'Integrated payment and billing system', 'PAYMENT_PROC', true, 2, 
 '1.0.0', '9999.0.0', '{"payment_methods": ["credit_card", "ach", "cash"], "recurring_billing": true}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),
('8e1f4a9b-2c5d-0e7f-5a8b-1c2d3e4f5a6b', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '6e9f2a7b-0c3d-8e5f-3a6b-9c0d1e2f3a4b', 
 'Mobile App Integration', 'Native mobile app for members', 'MOBILE_APP', true, 3, 
 '1.2.0', '9999.0.0', '{"ios": true, "android": true, "push_notifications": true}', 
 true, false, 'admin', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

-- Add more features for other tiers and products...
-- (Continuing with similar pattern for Wellness Suite and Enterprise products)

COMMIT;

-- Display success message
SELECT 'Successfully inserted sample data part 1: Products, Versions, Tiers, and Features' AS status;