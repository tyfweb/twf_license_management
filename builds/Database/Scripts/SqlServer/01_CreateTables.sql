-- ================================================================================================
-- SQL Server CREATE TABLE Script for TechWayFit Licensing Management System
-- Created: Auto-generated from Entity Framework Models
-- Provider: SQL Server with PascalCase naming convention
-- ================================================================================================

-- Create tenant table first (referenced by other tables)
CREATE TABLE [dbo].[Tenants] (
    [TenantId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantName] NVARCHAR(255) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION
);

-- Create products table
CREATE TABLE [dbo].[Products] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [ReleaseDate] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [SupportEmail] NVARCHAR(255) NOT NULL DEFAULT '',
    [SupportPhone] NVARCHAR(50) NOT NULL DEFAULT '',
    [DecommissionDate] DATETIMEOFFSET NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [MetadataJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    -- Workflow fields
    [ApprovalStatus] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [ApprovalRequired] BIT NOT NULL DEFAULT 0,
    [ApprovalComments] NVARCHAR(MAX) NULL,
    [ApprovedBy] NVARCHAR(100) NULL,
    [ApprovedOn] DATETIMEOFFSET NULL,
    [RejectedBy] NVARCHAR(100) NULL,
    [RejectedOn] DATETIMEOFFSET NULL,
    [SubmissionDate] DATETIMEOFFSET NULL,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create product_versions table
CREATE TABLE [dbo].[ProductVersions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [VersionNumber] NVARCHAR(50) NOT NULL,
    [VersionName] NVARCHAR(255) NOT NULL DEFAULT '',
    [ReleaseDate] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [EndOfLifeDate] DATETIMEOFFSET NULL,
    [IsBeta] BIT NOT NULL DEFAULT 0,
    [Changelog] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [DownloadUrl] NVARCHAR(500) NOT NULL DEFAULT '',
    [MinimumRequirements] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [InstallationGuide] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
);

-- Create product_tiers table
CREATE TABLE [dbo].[ProductTiers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [TierName] NVARCHAR(100) NOT NULL,
    [TierDescription] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [Price] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [CurrencyCode] NVARCHAR(3) NOT NULL DEFAULT 'USD',
    [BillingCycle] NVARCHAR(50) NOT NULL DEFAULT 'Monthly',
    [MaxUsers] INT NOT NULL DEFAULT 1,
    [MaxDevices] INT NOT NULL DEFAULT 1,
    [StorageLimitGb] INT NOT NULL DEFAULT 0,
    [ApiRateLimit] INT NOT NULL DEFAULT 1000,
    [FeaturesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
);

-- Create product_features table
CREATE TABLE [dbo].[ProductFeatures] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [FeatureName] NVARCHAR(100) NOT NULL,
    [FeatureCode] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [Category] NVARCHAR(100) NOT NULL DEFAULT '',
    [IsPremium] BIT NOT NULL DEFAULT 0,
    [MinTierRequired] NVARCHAR(50) NOT NULL DEFAULT 'Basic',
    [ConfigurationJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
);

-- Create consumer_accounts table
CREATE TABLE [dbo].[ConsumerAccounts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [CompanyName] NVARCHAR(255) NOT NULL DEFAULT '',
    [AccountCode] NVARCHAR(50) NULL,
    [PrimaryContactName] NVARCHAR(255) NOT NULL DEFAULT '',
    [PrimaryContactEmail] NVARCHAR(255) NOT NULL DEFAULT '',
    [PrimaryContactPhone] NVARCHAR(50) NOT NULL DEFAULT '',
    [PrimaryContactPosition] NVARCHAR(100) NOT NULL DEFAULT '',
    [SecondaryContactName] NVARCHAR(255) NULL,
    [SecondaryContactEmail] NVARCHAR(255) NULL,
    [SecondaryContactPhone] NVARCHAR(50) NULL,
    [SecondaryContactPosition] NVARCHAR(100) NULL,
    [ActivatedAt] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [SubscriptionEnd] DATETIMEOFFSET NULL,
    [AddressStreet] NVARCHAR(255) NOT NULL DEFAULT '',
    [AddressCity] NVARCHAR(100) NOT NULL DEFAULT '',
    [AddressState] NVARCHAR(100) NOT NULL DEFAULT '',
    [AddressPostalCode] NVARCHAR(20) NOT NULL DEFAULT '',
    [AddressCountry] NVARCHAR(100) NOT NULL DEFAULT '',
    [Notes] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [BillingContactName] NVARCHAR(255) NOT NULL DEFAULT '',
    [BillingContactEmail] NVARCHAR(255) NOT NULL DEFAULT '',
    [BillingContactPhone] NVARCHAR(50) NOT NULL DEFAULT '',
    [AccountManager] NVARCHAR(255) NOT NULL DEFAULT '',
    [Industry] NVARCHAR(100) NOT NULL DEFAULT '',
    [CompanySize] NVARCHAR(50) NOT NULL DEFAULT '',
    [Website] NVARCHAR(500) NOT NULL DEFAULT '',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create product_consumers table (relationship between products and consumer accounts)
CREATE TABLE [dbo].[ProductConsumers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [ConsumerAccountId] UNIQUEIDENTIFIER NOT NULL,
    [SubscriptionStart] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [SubscriptionEnd] DATETIMEOFFSET NULL,
    [TierId] UNIQUEIDENTIFIER NULL,
    [IsTrial] BIT NOT NULL DEFAULT 0,
    [TrialEndDate] DATETIMEOFFSET NULL,
    [CustomFeaturesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [CustomLimitsJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [SubscriptionStatus] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ConsumerAccountId]) REFERENCES [dbo].[ConsumerAccounts]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([TierId]) REFERENCES [dbo].[ProductTiers]([Id])
);

-- Create product_licenses table
CREATE TABLE [dbo].[ProductLicenses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [ConsumerAccountId] UNIQUEIDENTIFIER NOT NULL,
    [LicenseKey] NVARCHAR(500) NOT NULL,
    [LicenseType] NVARCHAR(50) NOT NULL DEFAULT 'Standard',
    [ExpiresAt] DATETIMEOFFSET NULL,
    [MaxActivations] INT NOT NULL DEFAULT 1,
    [CurrentActivations] INT NOT NULL DEFAULT 0,
    [FeaturesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [RestrictionsJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [HardwareFingerprint] NVARCHAR(500) NOT NULL DEFAULT '',
    [ActivationDataJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsRevoked] BIT NOT NULL DEFAULT 0,
    [RevokedAt] DATETIMEOFFSET NULL,
    [RevokedReason] NVARCHAR(500) NOT NULL DEFAULT '',
    [LastHeartbeat] DATETIMEOFFSET NULL,
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ConsumerAccountId]) REFERENCES [dbo].[ConsumerAccounts]([Id]) ON DELETE CASCADE
);

-- Create user_roles table
CREATE TABLE [dbo].[UserRoles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [RoleName] NVARCHAR(100) NOT NULL,
    [RoleDescription] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [PermissionsJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsSystemRole] BIT NOT NULL DEFAULT 0,
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create user_profiles table
CREATE TABLE [dbo].[UserProfiles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [UserName] NVARCHAR(255) NOT NULL,
    [PasswordHash] NVARCHAR(500) NOT NULL DEFAULT '',
    [PasswordSalt] NVARCHAR(500) NOT NULL DEFAULT '',
    [FullName] NVARCHAR(255) NOT NULL DEFAULT '',
    [Email] NVARCHAR(255) NOT NULL DEFAULT '',
    [Department] NVARCHAR(100) NULL,
    [IsLocked] BIT NOT NULL DEFAULT 0,
    [IsAdmin] BIT NOT NULL DEFAULT 0,
    [LastLoginDate] DATETIMEOFFSET NULL,
    [FailedLoginAttempts] INT NOT NULL DEFAULT 0,
    [LockedDate] DATETIMEOFFSET NULL,
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create user_role_mappings table
CREATE TABLE [dbo].[UserRoleMappings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedAt] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [AssignedBy] NVARCHAR(100) NOT NULL,
    [ExpiresAt] DATETIMEOFFSET NULL,
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserProfiles]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[UserRoles]([Id]) ON DELETE CASCADE
);

-- Create notification_templates table
CREATE TABLE [dbo].[NotificationTemplates] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [TemplateName] NVARCHAR(100) NOT NULL,
    [TemplateType] NVARCHAR(50) NOT NULL DEFAULT 'Email',
    [SubjectTemplate] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [BodyTemplate] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [VariablesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsSystemTemplate] BIT NOT NULL DEFAULT 0,
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create notification_history table
CREATE TABLE [dbo].[NotificationHistory] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [TemplateId] UNIQUEIDENTIFIER NULL,
    [RecipientEmail] NVARCHAR(255) NOT NULL DEFAULT '',
    [RecipientName] NVARCHAR(255) NOT NULL DEFAULT '',
    [Subject] NVARCHAR(500) NOT NULL DEFAULT '',
    [Body] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [NotificationType] NVARCHAR(50) NOT NULL DEFAULT 'Email',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [SentAt] DATETIMEOFFSET NULL,
    [FailedAt] DATETIMEOFFSET NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    [ErrorMessage] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [MetadataJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]),
    FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[NotificationTemplates]([Id])
);

-- Create workflow_history table
CREATE TABLE [dbo].[WorkflowHistory] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [EntityId] UNIQUEIDENTIFIER NOT NULL,
    [EntityType] NVARCHAR(100) NOT NULL,
    [WorkflowStep] NVARCHAR(100) NOT NULL,
    [PreviousStatus] NVARCHAR(50) NOT NULL DEFAULT '',
    [NewStatus] NVARCHAR(50) NOT NULL DEFAULT '',
    [ActionTaken] NVARCHAR(100) NOT NULL DEFAULT '',
    [ActorId] UNIQUEIDENTIFIER NULL,
    [ActorName] NVARCHAR(255) NOT NULL DEFAULT '',
    [Comments] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [MetadataJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [OccurredAt] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create settings table
CREATE TABLE [dbo].[Settings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [SettingKey] NVARCHAR(255) NOT NULL,
    [SettingValue] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [SettingType] NVARCHAR(50) NOT NULL DEFAULT 'String',
    [Category] NVARCHAR(100) NOT NULL DEFAULT 'General',
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [IsSensitive] BIT NOT NULL DEFAULT 0,
    [IsSystemSetting] BIT NOT NULL DEFAULT 0,
    [ValidationRulesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create audit_entries table
CREATE TABLE [dbo].[AuditEntries] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [EntityId] UNIQUEIDENTIFIER NOT NULL,
    [EntityType] NVARCHAR(100) NOT NULL,
    [Operation] NVARCHAR(50) NOT NULL,
    [UserId] NVARCHAR(100) NOT NULL,
    [UserName] NVARCHAR(255) NOT NULL DEFAULT '',
    [Timestamp] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [OldValuesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [NewValuesJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [ChangesSummaryJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [IpAddress] NVARCHAR(45) NOT NULL DEFAULT '',
    [UserAgent] NVARCHAR(500) NOT NULL DEFAULT '',
    [IsReadOnly] BIT NOT NULL DEFAULT 0,
    [CanDelete] BIT NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId])
);

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_Products_TenantId] ON [dbo].[Products]([TenantId]);
CREATE NONCLUSTERED INDEX [IX_Products_Status] ON [dbo].[Products]([Status]);
CREATE NONCLUSTERED INDEX [IX_ProductVersions_ProductId] ON [dbo].[ProductVersions]([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductTiers_ProductId] ON [dbo].[ProductTiers]([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductFeatures_ProductId] ON [dbo].[ProductFeatures]([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ConsumerAccounts_TenantId] ON [dbo].[ConsumerAccounts]([TenantId]);
CREATE NONCLUSTERED INDEX [IX_ProductConsumers_ProductId] ON [dbo].[ProductConsumers]([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductConsumers_ConsumerAccountId] ON [dbo].[ProductConsumers]([ConsumerAccountId]);
CREATE NONCLUSTERED INDEX [IX_ProductLicenses_ProductId] ON [dbo].[ProductLicenses]([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductLicenses_ConsumerAccountId] ON [dbo].[ProductLicenses]([ConsumerAccountId]);
CREATE NONCLUSTERED INDEX [IX_ProductLicenses_LicenseKey] ON [dbo].[ProductLicenses]([LicenseKey]);
CREATE NONCLUSTERED INDEX [IX_UserProfiles_TenantId] ON [dbo].[UserProfiles]([TenantId]);
CREATE NONCLUSTERED INDEX [IX_UserProfiles_UserName] ON [dbo].[UserProfiles]([UserName]);
CREATE NONCLUSTERED INDEX [IX_UserProfiles_Email] ON [dbo].[UserProfiles]([Email]);
CREATE NONCLUSTERED INDEX [IX_UserRoleMappings_UserId] ON [dbo].[UserRoleMappings]([UserId]);
CREATE NONCLUSTERED INDEX [IX_UserRoleMappings_RoleId] ON [dbo].[UserRoleMappings]([RoleId]);
CREATE NONCLUSTERED INDEX [IX_NotificationHistory_RecipientEmail] ON [dbo].[NotificationHistory]([RecipientEmail]);
CREATE NONCLUSTERED INDEX [IX_WorkflowHistory_EntityId] ON [dbo].[WorkflowHistory]([EntityId]);
CREATE NONCLUSTERED INDEX [IX_AuditEntries_EntityId] ON [dbo].[AuditEntries]([EntityId]);
CREATE NONCLUSTERED INDEX [IX_AuditEntries_Timestamp] ON [dbo].[AuditEntries]([Timestamp]);

-- Add unique constraints
ALTER TABLE [dbo].[Tenants] ADD CONSTRAINT [UK_Tenants_TenantName] UNIQUE ([TenantName]);
ALTER TABLE [dbo].[Products] ADD CONSTRAINT [UK_Products_Name_Tenant] UNIQUE ([Name], [TenantId]);
ALTER TABLE [dbo].[ProductVersions] ADD CONSTRAINT [UK_ProductVersions_Number_Product] UNIQUE ([VersionNumber], [ProductId]);
ALTER TABLE [dbo].[ProductTiers] ADD CONSTRAINT [UK_ProductTiers_Name_Product] UNIQUE ([TierName], [ProductId]);
ALTER TABLE [dbo].[ProductFeatures] ADD CONSTRAINT [UK_ProductFeatures_Code_Product] UNIQUE ([FeatureCode], [ProductId]);
ALTER TABLE [dbo].[ConsumerAccounts] ADD CONSTRAINT [UK_ConsumerAccounts_AccountCode] UNIQUE ([AccountCode]) WHERE [AccountCode] IS NOT NULL;
ALTER TABLE [dbo].[UserProfiles] ADD CONSTRAINT [UK_UserProfiles_UserName_Tenant] UNIQUE ([UserName], [TenantId]);
ALTER TABLE [dbo].[UserProfiles] ADD CONSTRAINT [UK_UserProfiles_Email_Tenant] UNIQUE ([Email], [TenantId]);
ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [UK_UserRoles_Name_Tenant] UNIQUE ([RoleName], [TenantId]);
ALTER TABLE [dbo].[UserRoleMappings] ADD CONSTRAINT [UK_UserRoleMappings_User_Role] UNIQUE ([UserId], [RoleId]);
ALTER TABLE [dbo].[NotificationTemplates] ADD CONSTRAINT [UK_NotificationTemplates_Name_Tenant] UNIQUE ([TemplateName], [TenantId]);
ALTER TABLE [dbo].[Settings] ADD CONSTRAINT [UK_Settings_Key_Tenant] UNIQUE ([SettingKey], [TenantId]);

-- Add table descriptions using extended properties
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Tenant information for multi-tenancy support', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Enterprise products available for licensing', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Products';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Version information for products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductVersions';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Pricing tiers and feature sets for products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductTiers';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Individual features available in products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductFeatures';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Customer accounts that consume licensed products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ConsumerAccounts';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Relationship between products and consumer accounts', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductConsumers';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'License keys and activation data for products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductLicenses';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'User accounts for system access', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserProfiles';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Role definitions for access control', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Assignment of roles to users', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoleMappings';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Templates for system notifications', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'NotificationTemplates';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'History of sent notifications', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'NotificationHistory';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Audit trail for workflow state changes', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WorkflowHistory';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'System and tenant-specific configuration settings', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Settings';
EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'Comprehensive audit log for all entity changes', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditEntries';
