-- Create Settings table for system configuration management
-- TechWayFit License Management System
-- Database Version: PostgreSQL 13+

CREATE TABLE IF NOT EXISTS public.settings (
    setting_id VARCHAR(36) PRIMARY KEY DEFAULT gen_random_uuid()::text,
    category VARCHAR(100) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value VARCHAR(4000),
    default_value VARCHAR(4000),
    data_type VARCHAR(50) NOT NULL DEFAULT 'string',
    display_name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    group_name VARCHAR(100),
    display_order INTEGER NOT NULL DEFAULT 0,
    is_editable BOOLEAN NOT NULL DEFAULT TRUE,
    is_required BOOLEAN NOT NULL DEFAULT FALSE,
    is_sensitive BOOLEAN NOT NULL DEFAULT FALSE,
    validation_rules VARCHAR(2000),
    possible_values VARCHAR(2000),
    value_source VARCHAR(50) NOT NULL DEFAULT 'Database',
    tags VARCHAR(500),
    requires_restart BOOLEAN NOT NULL DEFAULT FALSE,
    environment VARCHAR(50) NOT NULL DEFAULT 'All',
    introduced_in_version VARCHAR(20),
    is_deprecated BOOLEAN NOT NULL DEFAULT FALSE,
    deprecation_message VARCHAR(500),
    
    -- Audit fields from BaseAuditEntity
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_by VARCHAR(100),
    updated_on TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT uc_settings_category_key UNIQUE (category, key),
    CONSTRAINT chk_settings_data_type CHECK (data_type IN ('string', 'int', 'bool', 'decimal', 'double', 'datetime', 'json')),
    CONSTRAINT chk_settings_environment CHECK (environment IN ('Development', 'Staging', 'Production', 'All')),
    CONSTRAINT chk_settings_value_source CHECK (value_source IN ('Default', 'Configuration', 'Database', 'User'))
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS ix_settings_category ON public.settings (category) WHERE is_active = TRUE;
CREATE INDEX IF NOT EXISTS ix_settings_environment ON public.settings (environment) WHERE is_active = TRUE;
CREATE INDEX IF NOT EXISTS ix_settings_display_order ON public.settings (category, display_order) WHERE is_active = TRUE;
CREATE INDEX IF NOT EXISTS ix_settings_tags ON public.settings USING gin(to_tsvector('english', tags)) WHERE is_active = TRUE AND tags IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_settings_search ON public.settings USING gin(to_tsvector('english', display_name || ' ' || COALESCE(description, ''))) WHERE is_active = TRUE;

-- Insert default system settings
INSERT INTO public.settings (
    setting_id, category, key, value, default_value, data_type, 
    display_name, description, group_name, display_order, 
    is_editable, is_required, is_sensitive, tags, created_by
) VALUES 
-- System Configuration
('a1b2c3d4-e5f6-7890-1234-567890abcdef', 'System', 'ApplicationName', 'TechWayFit License Management', 'TechWayFit License Management', 'string', 
 'Application Name', 'The name of the application displayed in the UI', 'Branding', 1, 
 TRUE, TRUE, FALSE, 'branding,ui,display', 'System'),

('b2c3d4e5-f6g7-8901-2345-67890abcdef1', 'System', 'ApplicationVersion', '1.0.0', '1.0.0', 'string', 
 'Application Version', 'Current version of the application', 'Branding', 2, 
 FALSE, TRUE, FALSE, 'version,system', 'System'),

('c3d4e5f6-g7h8-9012-3456-7890abcdef12', 'System', 'CompanyName', 'TechWayFit Solutions', 'TechWayFit Solutions', 'string', 
 'Company Name', 'Name of the company owning this system', 'Branding', 3, 
 TRUE, TRUE, FALSE, 'branding,company', 'System'),

('d4e5f6g7-h8i9-0123-4567-890abcdef123', 'System', 'CompanyLogo', '/images/logo.png', '/images/logo.png', 'string', 
 'Company Logo URL', 'URL path to the company logo image', 'Branding', 4, 
 TRUE, FALSE, FALSE, 'branding,logo,ui', 'System'),

('e5f6g7h8-i9j0-1234-5678-90abcdef1234', 'System', 'FaviconUrl', '/favicon.ico', '/favicon.ico', 'string', 
 'Favicon URL', 'URL path to the favicon', 'Branding', 5, 
 TRUE, FALSE, FALSE, 'branding,favicon,ui', 'System'),

-- Email Configuration
('f6g7h8i9-j0k1-2345-6789-0abcdef12345', 'Email', 'SmtpServer', 'smtp.gmail.com', 'smtp.gmail.com', 'string', 
 'SMTP Server', 'SMTP server hostname for sending emails', 'Email Server', 1, 
 TRUE, TRUE, FALSE, 'email,smtp,server', 'System'),

('g7h8i9j0-k1l2-3456-7890-abcdef123456', 'Email', 'SmtpPort', '587', '587', 'int', 
 'SMTP Port', 'SMTP server port number', 'Email Server', 2, 
 TRUE, TRUE, FALSE, 'email,smtp,port', 'System'),

('h8i9j0k1-l2m3-4567-8901-bcdef1234567', 'Email', 'SmtpUsername', '', '', 'string', 
 'SMTP Username', 'Username for SMTP authentication', 'Email Server', 3, 
 TRUE, TRUE, TRUE, 'email,smtp,auth', 'System'),

('i9j0k1l2-m3n4-5678-9012-cdef12345678', 'Email', 'SmtpPassword', '', '', 'string', 
 'SMTP Password', 'Password for SMTP authentication', 'Email Server', 4, 
 TRUE, TRUE, TRUE, 'email,smtp,auth,password', 'System'),

('j0k1l2m3-n4o5-6789-0123-def123456789', 'Email', 'FromAddress', 'noreply@techwayfitsolutions.com', 'noreply@techwayfitsolutions.com', 'string', 
 'From Email Address', 'Email address used as sender for outgoing emails', 'Email Server', 5, 
 TRUE, TRUE, FALSE, 'email,sender', 'System'),

('k1l2m3n4-o5p6-7890-1234-ef12345678ab', 'Email', 'FromName', 'TechWayFit License Management', 'TechWayFit License Management', 'string', 
 'From Display Name', 'Display name used as sender for outgoing emails', 'Email Server', 6, 
 TRUE, TRUE, FALSE, 'email,sender,display', 'System'),

('l2m3n4o5-p6q7-8901-2345-f12345678abc', 'Email', 'EnableSsl', 'true', 'true', 'bool', 
 'Enable SSL', 'Whether to use SSL/TLS for SMTP connection', 'Email Server', 7, 
 TRUE, TRUE, FALSE, 'email,ssl,security', 'System'),

-- Security Settings
('m3n4o5p6-q7r8-9012-3456-12345678abcd', 'Security', 'SessionTimeoutMinutes', '30', '30', 'int', 
 'User Session Timeout (Minutes)', 'User session timeout in minutes', 'Authentication', 1, 
 TRUE, TRUE, FALSE, 'security,session,timeout', 'System'),

('n4o5p6q7-r8s9-0123-4567-2345678abcde', 'Security', 'PasswordMinLength', '8', '8', 'int', 
 'Minimum Password Length', 'Minimum required password length', 'Password Policy', 2, 
 TRUE, TRUE, FALSE, 'security,password,length', 'System'),

('o5p6q7r8-s9t0-1234-5678-345678abcdef', 'Security', 'RequireUppercase', 'true', 'true', 'bool', 
 'Require Uppercase Letters', 'Whether passwords must contain uppercase letters', 'Password Policy', 3, 
 TRUE, TRUE, FALSE, 'security,password,uppercase', 'System'),

('p6q7r8s9-t0u1-2345-6789-45678abcdef0', 'Security', 'RequireLowercase', 'true', 'true', 'bool', 
 'Require Lowercase Letters', 'Whether passwords must contain lowercase letters', 'Password Policy', 4, 
 TRUE, TRUE, FALSE, 'security,password,lowercase', 'System'),

('q7r8s9t0-u1v2-3456-7890-5678abcdef01', 'Security', 'RequireNumbers', 'true', 'true', 'bool', 
 'Require Numbers', 'Whether passwords must contain numeric digits', 'Password Policy', 5, 
 TRUE, TRUE, FALSE, 'security,password,numbers', 'System'),

('r8s9t0u1-v2w3-4567-8901-678abcdef012', 'Security', 'RequireSymbols', 'true', 'true', 'bool', 
 'Require Special Characters', 'Whether passwords must contain special characters', 'Password Policy', 6, 
 TRUE, TRUE, FALSE, 'security,password,symbols', 'System'),

-- License Configuration
('s9t0u1v2-w3x4-5678-9012-78abcdef0123', 'License', 'DefaultDurationDays', '365', '365', 'int', 
 'Default License Duration (Days)', 'Default duration for new licenses in days', 'License Defaults', 1, 
 TRUE, TRUE, FALSE, 'license,duration,default', 'System'),

('t0u1v2w3-x4y5-6789-0123-8abcdef01234', 'License', 'GracePeriodDays', '30', '30', 'int', 
 'Grace Period (Days)', 'Grace period after license expiration in days', 'License Defaults', 2, 
 TRUE, TRUE, FALSE, 'license,grace,period', 'System'),

('u1v2w3x4-y5z6-7890-1234-abcdef012345', 'License', 'AutoRenewEnabled', 'false', 'false', 'bool', 
 'Enable Auto-Renewal', 'Whether licenses can be automatically renewed', 'License Features', 3, 
 TRUE, FALSE, FALSE, 'license,auto,renew', 'System'),

('v2w3x4y5-z6a7-8901-2345-bcdef0123456', 'License', 'MaxActivationsPerLicense', '5', '5', 'int', 
 'Maximum Activations per License', 'Maximum number of activations allowed per license', 'License Limits', 4, 
 TRUE, TRUE, FALSE, 'license,activations,limit', 'System'),

-- UI Settings
('w3x4y5z6-a7b8-9012-3456-cdef01234567', 'UI', 'DefaultTheme', 'light', 'light', 'string', 
 'Default Theme', 'Default UI theme for new users', 'Appearance', 1, 
 TRUE, FALSE, FALSE, 'ui,theme,appearance', 'System'),

('x4y5z6a7-b8c9-0123-4567-def012345678', 'UI', 'ItemsPerPage', '25', '25', 'int', 
 'Items per Page', 'Default number of items to display per page in lists', 'Pagination', 2, 
 TRUE, FALSE, FALSE, 'ui,pagination,items', 'System'),

('y5z6a7b8-c9d0-1234-5678-ef0123456789', 'UI', 'EnableDarkMode', 'true', 'true', 'bool', 
 'Enable Dark Mode', 'Whether dark mode option is available to users', 'Appearance', 3, 
 TRUE, FALSE, FALSE, 'ui,dark,mode', 'System'),

-- Notification Settings
('z6a7b8c9-d0e1-2345-6789-f012345678ab', 'Notification', 'EmailNotificationsEnabled', 'true', 'true', 'bool', 
 'Enable Email Notifications', 'Whether email notifications are enabled system-wide', 'Email', 1, 
 TRUE, FALSE, FALSE, 'notification,email,enabled', 'System'),

('a7b8c9d0-e1f2-3456-7890-012345678abc', 'Notification', 'LicenseExpiryNotificationDays', '30,7,1', '30,7,1', 'string', 
 'License Expiry Notification Days', 'Days before expiry to send notifications (comma-separated)', 'License Alerts', 2, 
 TRUE, TRUE, FALSE, 'notification,license,expiry', 'System');

-- Add comments to the table
COMMENT ON TABLE public.settings IS 'System configuration settings for the TechWayFit License Management application';
COMMENT ON COLUMN public.settings.setting_id IS 'Unique identifier for the setting';
COMMENT ON COLUMN public.settings.category IS 'Logical grouping category for the setting';
COMMENT ON COLUMN public.settings.key IS 'Unique key within the category';
COMMENT ON COLUMN public.settings.value IS 'Current value of the setting';
COMMENT ON COLUMN public.settings.default_value IS 'Default value to reset to';
COMMENT ON COLUMN public.settings.data_type IS 'Data type for validation and conversion';
COMMENT ON COLUMN public.settings.is_sensitive IS 'Whether this setting contains sensitive data (passwords, API keys)';
COMMENT ON COLUMN public.settings.requires_restart IS 'Whether changing this setting requires application restart';

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON public.settings TO postgres;

COMMIT;
