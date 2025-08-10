-- TechWayFit Licensing Management System - PostgreSQL Seeding Script Par-- =============================================
-- SETTINGS DATA (system configuration - corrected schema)
-- =============================================
INSERT INTO settings (
    category, key, value, default_value, data_type, 
    display_name, description, group_name, display_order, 
    is_editable, is_required, is_sensitive, validation_rules, possible_values,
    is_active, is_deleted, created_by, updated_by, created_on, updated_on, deleted_by, deleted_on
) VALUES 
-- System Configuration
('System', 'ApplicationName', 'TechWayFit License Management', 'TechWayFit License Management', 'string', 
 'Application Name', 'The name of the application displayed in the UI', 'Branding', 1, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('System', 'ApplicationVersion', '1.0.0', '1.0.0', 'string', 
 'Application Version', 'Current version of the application', 'Branding', 2, 
 false, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('System', 'CompanyName', 'TechWayFit Solutions', 'TechWayFit Solutions', 'string', 
 'Company Name', 'Name of the company owning this system', 'Branding', 3, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('System', 'CompanyLogo', '/images/logo.png', '/images/logo.png', 'string', 
 'Company Logo URL', 'URL path to the company logo image', 'Branding', 4, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('System', 'FaviconUrl', '/favicon.ico', '/favicon.ico', 'string', 
 'Favicon URL', 'URL path to the favicon', 'Branding', 5, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Email Configuration
('Email', 'SmtpServer', 'smtp.gmail.com', 'smtp.gmail.com', 'string', 
 'SMTP Server', 'SMTP server hostname for sending emails', 'Email Server', 1, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'SmtpPort', '587', '587', 'int', 
 'SMTP Port', 'SMTP server port number', 'Email Server', 2, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'SmtpUsername', '', '', 'string', 
 'SMTP Username', 'Username for SMTP authentication', 'Email Server', 3, 
 true, true, true, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'SmtpPassword', '', '', 'string', 
 'SMTP Password', 'Password for SMTP authentication', 'Email Server', 4, 
 true, true, true, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'FromAddress', 'noreply@techwayfitsolutions.com', 'noreply@techwayfitsolutions.com', 'string', 
 'From Email Address', 'Email address used as sender for outgoing emails', 'Email Server', 5, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'FromName', 'TechWayFit License Management', 'TechWayFit License Management', 'string', 
 'From Display Name', 'Display name used as sender for outgoing emails', 'Email Server', 6, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Email', 'EnableSsl', 'true', 'true', 'bool', 
 'Enable SSL', 'Whether to use SSL/TLS for SMTP connection', 'Email Server', 7, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Security Settings
('Security', 'SessionTimeoutMinutes', '30', '30', 'int', 
 'User Session Timeout (Minutes)', 'User session timeout in minutes', 'Authentication', 1, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Security', 'PasswordMinLength', '8', '8', 'int', 
 'Minimum Password Length', 'Minimum required password length', 'Password Policy', 2, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Security', 'RequireUppercase', 'true', 'true', 'bool', 
 'Require Uppercase Letters', 'Whether passwords must contain uppercase letters', 'Password Policy', 3, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Security', 'RequireLowercase', 'true', 'true', 'bool', 
 'Require Lowercase Letters', 'Whether passwords must contain lowercase letters', 'Password Policy', 4, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Security', 'RequireNumbers', 'true', 'true', 'bool', 
 'Require Numbers', 'Whether passwords must contain numeric digits', 'Password Policy', 5, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Security', 'RequireSymbols', 'true', 'true', 'bool', 
 'Require Special Characters', 'Whether passwords must contain special characters', 'Password Policy', 6, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- License Configuration
('License', 'DefaultDurationDays', '365', '365', 'int', 
 'Default License Duration (Days)', 'Default duration for new licenses in days', 'License Defaults', 1, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('License', 'GracePeriodDays', '30', '30', 'int', 
 'Grace Period (Days)', 'Grace period after license expiration in days', 'License Defaults', 2, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('License', 'AutoRenewEnabled', 'false', 'false', 'bool', 
 'Enable Auto-Renewal', 'Whether licenses can be automatically renewed', 'License Features', 3, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('License', 'MaxActivationsPerLicense', '5', '5', 'int', 
 'Maximum Activations per License', 'Maximum number of activations allowed per license', 'License Limits', 4, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- UI Settings
('UI', 'CurrentTheme', 'default', 'default', 'string', 
 'Current Theme', 'Currently active UI theme for the application', 'Appearance', 1, 
 true, false, false, NULL, 'default,dark,blue,green,purple',
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('UI', 'ItemsPerPage', '25', '25', 'int', 
 'Items per Page', 'Default number of items to display per page in lists', 'Pagination', 2, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('UI', 'AvailableThemes', 'default,dark,blue,green,purple', 'default,dark,blue,green,purple', 'string', 
 'Available Themes', 'Comma-separated list of available themes for users to choose from', 'Appearance', 3, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('UI', 'ThemeAutoDetect', 'false', 'false', 'bool', 
 'Auto-Detect System Theme', 'Whether to automatically detect and use system dark/light mode preference', 'Appearance', 4, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('UI', 'AllowThemeCustomization', 'true', 'true', 'bool', 
 'Allow Theme Customization', 'Whether administrators can customize and create new themes', 'Appearance', 5, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('UI', 'ThemeTransitionDuration', '300', '300', 'int', 
 'Theme Transition Duration (ms)', 'Duration in milliseconds for theme transition animations', 'Appearance', 6, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

-- Notification Settings
('Notification', 'EmailNotificationsEnabled', 'true', 'true', 'bool', 
 'Enable Email Notifications', 'Whether email notifications are enabled system-wide', 'Email', 1, 
 true, false, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL),

('Notification', 'LicenseExpiryNotificationDays', '30,7,1', '30,7,1', 'string', 
 'License Expiry Notification Days', 'Days before expiry to send notifications (comma-separated)', 'License Alerts', 2, 
 true, true, false, NULL, NULL,
 true, false, 'System', NULL, CURRENT_TIMESTAMP, NULL, NULL, NULL);

COMMIT;

-- Display success message
SELECT 'Successfully inserted sample data part 3: User Roles, User Profiles, User Role Mappings, and Settings' AS status;
