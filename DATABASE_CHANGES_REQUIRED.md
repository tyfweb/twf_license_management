# Database Changes Required for Three License Types Implementation

## Overview

This document outlines all database schema changes required to implement the three types of license generation:

1. **Product License Files** - Offline activation with downloadable license files
2. **Product Keys** - Online activation with XXXX-XXXX-XXXX-XXXX format keys  
3. **Volumetric Licenses** - Multi-user keys with usage limits (XXXX-XXXX-XXXX-0001 to 9999)

## Current Database Structure Analysis

Based on the examination of the existing DbContext (`EfCoreLicensingDbContext`) and entity configurations, the current database has:

### Existing Tables
- `product_licenses` (PostgreSQL) / `ProductLicenses` (SQL Server) - Core license table
- `products`, `product_versions`, `product_tiers`, `product_features` - Product structure
- `consumer_accounts` - Customer information
- `product_consumers` - Product-consumer relationships
- Complete audit, notification, and user management infrastructure

### Current License Table Structure (PostgreSQL)
```sql
CREATE TABLE product_licenses (
    id UUID PRIMARY KEY,
    license_code VARCHAR(50) NOT NULL,
    product_id UUID NOT NULL,
    consumer_id UUID NOT NULL,
    product_tier_id UUID NULL,
    valid_from TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_to TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_product_version_from VARCHAR(20) NOT NULL DEFAULT '1.0.0',
    valid_product_version_to VARCHAR(20) NULL,
    license_key VARCHAR(2000) NOT NULL,
    public_key VARCHAR(2000),
    key_generated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    issued_by VARCHAR(100) NOT NULL,
    revoked_at TIMESTAMP WITH TIME ZONE,
    revocation_reason VARCHAR(500),
    metadata_json VARCHAR(2000) DEFAULT '{}'
    -- + audit fields
);
```

## Required Database Changes

### Phase 1: Core License Type Support

#### 1.1 Add License Type Column to Existing License Table

**PostgreSQL Migration:**
```sql
-- Add license type column to support different license types
ALTER TABLE product_licenses 
ADD COLUMN license_type VARCHAR(50) NOT NULL DEFAULT 'ProductLicenseFile';

-- Add license file management columns
ALTER TABLE product_licenses 
ADD COLUMN license_file_path VARCHAR(500) NULL,
ADD COLUMN license_file_name VARCHAR(255) NULL,
ADD COLUMN download_count INTEGER NOT NULL DEFAULT 0,
ADD COLUMN last_downloaded_at TIMESTAMP WITH TIME ZONE NULL,
ADD COLUMN last_downloaded_by VARCHAR(100) NULL;

-- Add product key specific columns
ALTER TABLE product_licenses 
ADD COLUMN formatted_product_key VARCHAR(19) NULL, -- XXXX-XXXX-XXXX-XXXX format
ADD COLUMN max_activations INTEGER NULL,
ADD COLUMN current_activations INTEGER NOT NULL DEFAULT 0;

-- Add volumetric license specific columns
ALTER TABLE product_licenses 
ADD COLUMN base_volumetric_key VARCHAR(14) NULL, -- XXXX-XXXX-XXXX format
ADD COLUMN max_concurrent_users INTEGER NULL,
ADD COLUMN max_total_users INTEGER NULL,
ADD COLUMN current_active_users INTEGER NOT NULL DEFAULT 0,
ADD COLUMN total_allocated_users INTEGER NOT NULL DEFAULT 0;

-- Add indexes for performance
CREATE INDEX idx_product_licenses_type ON product_licenses(license_type);
CREATE INDEX idx_product_licenses_formatted_key ON product_licenses(formatted_product_key) WHERE formatted_product_key IS NOT NULL;
CREATE INDEX idx_product_licenses_base_key ON product_licenses(base_volumetric_key) WHERE base_volumetric_key IS NOT NULL;
CREATE INDEX idx_product_licenses_download_tracking ON product_licenses(last_downloaded_at, download_count);
```

**SQL Server Migration:**
```sql
-- Add license type column to support different license types
ALTER TABLE [dbo].[ProductLicenses] 
ADD [LicenseType] NVARCHAR(50) NOT NULL DEFAULT 'ProductLicenseFile';

-- Add license file management columns
ALTER TABLE [dbo].[ProductLicenses] 
ADD [LicenseFilePath] NVARCHAR(500) NULL,
    [LicenseFileName] NVARCHAR(255) NULL,
    [DownloadCount] INT NOT NULL DEFAULT 0,
    [LastDownloadedAt] DATETIMEOFFSET NULL,
    [LastDownloadedBy] NVARCHAR(100) NULL;

-- Add product key specific columns
ALTER TABLE [dbo].[ProductLicenses] 
ADD [FormattedProductKey] NVARCHAR(19) NULL, -- XXXX-XXXX-XXXX-XXXX format
    [MaxActivations] INT NULL,
    [CurrentActivations] INT NOT NULL DEFAULT 0;

-- Add volumetric license specific columns
ALTER TABLE [dbo].[ProductLicenses] 
ADD [BaseVolumetricKey] NVARCHAR(14) NULL, -- XXXX-XXXX-XXXX format
    [MaxConcurrentUsers] INT NULL,
    [MaxTotalUsers] INT NULL,
    [CurrentActiveUsers] INT NOT NULL DEFAULT 0,
    [TotalAllocatedUsers] INT NOT NULL DEFAULT 0;

-- Add indexes for performance
CREATE INDEX [IX_ProductLicenses_Type] ON [dbo].[ProductLicenses]([LicenseType]);
CREATE INDEX [IX_ProductLicenses_FormattedKey] ON [dbo].[ProductLicenses]([FormattedProductKey]) WHERE [FormattedProductKey] IS NOT NULL;
CREATE INDEX [IX_ProductLicenses_BaseKey] ON [dbo].[ProductLicenses]([BaseVolumetricKey]) WHERE [BaseVolumetricKey] IS NOT NULL;
CREATE INDEX [IX_ProductLicenses_Download] ON [dbo].[ProductLicenses]([LastDownloadedAt], [DownloadCount]);
```

### Phase 2: Product Key Activation Management

#### 2.1 Create Product Activations Table

**PostgreSQL:**
```sql
CREATE TABLE product_activations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    license_id UUID NOT NULL,
    product_key VARCHAR(19) NOT NULL, -- XXXX-XXXX-XXXX-XXXX
    machine_id VARCHAR(255) NOT NULL,
    machine_name VARCHAR(255) NULL,
    machine_fingerprint VARCHAR(500) NULL,
    ip_address VARCHAR(45) NULL,
    activation_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_heartbeat TIMESTAMP WITH TIME ZONE NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Inactive, Suspended, Expired
    activation_data JSONB DEFAULT '{}',
    deactivation_date TIMESTAMP WITH TIME ZONE NULL,
    deactivation_reason VARCHAR(500) NULL,
    deactivated_by VARCHAR(100) NULL,
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT fk_product_activations_license_id 
        FOREIGN KEY (license_id) REFERENCES product_licenses(id) ON DELETE CASCADE,
    CONSTRAINT fk_product_activations_tenant_id 
        FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE RESTRICT,
    
    -- Unique constraint to prevent duplicate activations for same machine
    CONSTRAINT uk_product_activations_license_machine 
        UNIQUE (license_id, machine_id)
);

-- Indexes for performance
CREATE INDEX idx_product_activations_product_key ON product_activations(product_key);
CREATE INDEX idx_product_activations_license_id ON product_activations(license_id);
CREATE INDEX idx_product_activations_machine_id ON product_activations(machine_id);
CREATE INDEX idx_product_activations_status ON product_activations(status);
CREATE INDEX idx_product_activations_heartbeat ON product_activations(last_heartbeat) WHERE last_heartbeat IS NOT NULL;
CREATE INDEX idx_product_activations_tenant ON product_activations(tenant_id);
```

**SQL Server:**
```sql
CREATE TABLE [dbo].[ProductActivations] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [LicenseId] UNIQUEIDENTIFIER NOT NULL,
    [ProductKey] NVARCHAR(19) NOT NULL, -- XXXX-XXXX-XXXX-XXXX
    [MachineId] NVARCHAR(255) NOT NULL,
    [MachineName] NVARCHAR(255) NULL,
    [MachineFingerprint] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [ActivationDate] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [LastHeartbeat] DATETIMEOFFSET NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Inactive, Suspended, Expired
    [ActivationData] NVARCHAR(MAX) DEFAULT '{}',
    [DeactivationDate] DATETIMEOFFSET NULL,
    [DeactivationReason] NVARCHAR(500) NULL,
    [DeactivatedBy] NVARCHAR(100) NULL,
    
    -- Audit fields
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    
    -- Foreign key constraints
    FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[ProductLicenses]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]) ON DELETE NO ACTION,
    
    -- Unique constraint to prevent duplicate activations for same machine
    CONSTRAINT [UK_ProductActivations_License_Machine] 
        UNIQUE ([LicenseId], [MachineId])
);

-- Indexes for performance
CREATE INDEX [IX_ProductActivations_ProductKey] ON [dbo].[ProductActivations]([ProductKey]);
CREATE INDEX [IX_ProductActivations_LicenseId] ON [dbo].[ProductActivations]([LicenseId]);
CREATE INDEX [IX_ProductActivations_MachineId] ON [dbo].[ProductActivations]([MachineId]);
CREATE INDEX [IX_ProductActivations_Status] ON [dbo].[ProductActivations]([Status]);
CREATE INDEX [IX_ProductActivations_Heartbeat] ON [dbo].[ProductActivations]([LastHeartbeat]) WHERE [LastHeartbeat] IS NOT NULL;
CREATE INDEX [IX_ProductActivations_Tenant] ON [dbo].[ProductActivations]([TenantId]);
```

### Phase 3: Volumetric License Management

#### 3.1 Create Volumetric Licenses Table

**PostgreSQL:**
```sql
CREATE TABLE volumetric_licenses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    license_id UUID NOT NULL,
    base_key VARCHAR(14) NOT NULL, -- XXXX-XXXX-XXXX
    max_concurrent_users INTEGER NOT NULL,
    max_total_users INTEGER NOT NULL,
    current_active_users INTEGER NOT NULL DEFAULT 0,
    total_allocated_users INTEGER NOT NULL DEFAULT 0,
    auto_cleanup_inactive_hours INTEGER NOT NULL DEFAULT 24,
    configuration_data JSONB DEFAULT '{}',
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT fk_volumetric_licenses_license_id 
        FOREIGN KEY (license_id) REFERENCES product_licenses(id) ON DELETE CASCADE,
    CONSTRAINT fk_volumetric_licenses_tenant_id 
        FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE RESTRICT,
    CONSTRAINT uk_volumetric_licenses_base_key 
        UNIQUE (base_key),
    CONSTRAINT chk_volumetric_licenses_user_limits 
        CHECK (max_concurrent_users > 0 AND max_total_users > 0 AND current_active_users <= max_concurrent_users)
);

-- Indexes
CREATE INDEX idx_volumetric_licenses_license_id ON volumetric_licenses(license_id);
CREATE INDEX idx_volumetric_licenses_base_key ON volumetric_licenses(base_key);
CREATE INDEX idx_volumetric_licenses_tenant ON volumetric_licenses(tenant_id);
```

**SQL Server:**
```sql
CREATE TABLE [dbo].[VolumetricLicenses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [LicenseId] UNIQUEIDENTIFIER NOT NULL,
    [BaseKey] NVARCHAR(14) NOT NULL, -- XXXX-XXXX-XXXX
    [MaxConcurrentUsers] INT NOT NULL,
    [MaxTotalUsers] INT NOT NULL,
    [CurrentActiveUsers] INT NOT NULL DEFAULT 0,
    [TotalAllocatedUsers] INT NOT NULL DEFAULT 0,
    [AutoCleanupInactiveHours] INT NOT NULL DEFAULT 24,
    [ConfigurationData] NVARCHAR(MAX) DEFAULT '{}',
    
    -- Audit fields
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    
    -- Foreign key constraints
    FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[ProductLicenses]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]) ON DELETE NO ACTION,
    
    -- Unique and check constraints
    CONSTRAINT [UK_VolumetricLicenses_BaseKey] UNIQUE ([BaseKey]),
    CONSTRAINT [CHK_VolumetricLicenses_UserLimits] 
        CHECK ([MaxConcurrentUsers] > 0 AND [MaxTotalUsers] > 0 AND [CurrentActiveUsers] <= [MaxConcurrentUsers])
);

-- Indexes
CREATE INDEX [IX_VolumetricLicenses_LicenseId] ON [dbo].[VolumetricLicenses]([LicenseId]);
CREATE INDEX [IX_VolumetricLicenses_BaseKey] ON [dbo].[VolumetricLicenses]([BaseKey]);
CREATE INDEX [IX_VolumetricLicenses_Tenant] ON [dbo].[VolumetricLicenses]([TenantId]);
```

#### 3.2 Create Volumetric User Slots Table

**PostgreSQL:**
```sql
CREATE TABLE volumetric_user_slots (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    volumetric_license_id UUID NOT NULL,
    slot_number INTEGER NOT NULL, -- 1 to 9999
    user_key VARCHAR(19) NOT NULL, -- XXXX-XXXX-XXXX-0001
    user_id VARCHAR(255) NULL,
    machine_id VARCHAR(255) NULL,
    machine_name VARCHAR(255) NULL,
    machine_fingerprint VARCHAR(500) NULL,
    ip_address VARCHAR(45) NULL,
    first_activation TIMESTAMP WITH TIME ZONE NULL,
    last_activity TIMESTAMP WITH TIME ZONE NULL,
    is_currently_active BOOLEAN NOT NULL DEFAULT false,
    activation_data JSONB DEFAULT '{}',
    session_data JSONB DEFAULT '{}',
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT fk_volumetric_user_slots_volumetric_license_id 
        FOREIGN KEY (volumetric_license_id) REFERENCES volumetric_licenses(id) ON DELETE CASCADE,
    CONSTRAINT fk_volumetric_user_slots_tenant_id 
        FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE RESTRICT,
    CONSTRAINT uk_volumetric_user_slots_user_key 
        UNIQUE (user_key),
    CONSTRAINT uk_volumetric_user_slots_license_slot 
        UNIQUE (volumetric_license_id, slot_number),
    CONSTRAINT chk_volumetric_user_slots_slot_range 
        CHECK (slot_number >= 1 AND slot_number <= 9999)
);

-- Indexes
CREATE INDEX idx_volumetric_user_slots_volumetric_license ON volumetric_user_slots(volumetric_license_id);
CREATE INDEX idx_volumetric_user_slots_user_key ON volumetric_user_slots(user_key);
CREATE INDEX idx_volumetric_user_slots_machine_id ON volumetric_user_slots(machine_id) WHERE machine_id IS NOT NULL;
CREATE INDEX idx_volumetric_user_slots_activity ON volumetric_user_slots(last_activity) WHERE last_activity IS NOT NULL;
CREATE INDEX idx_volumetric_user_slots_active ON volumetric_user_slots(is_currently_active) WHERE is_currently_active = true;
CREATE INDEX idx_volumetric_user_slots_tenant ON volumetric_user_slots(tenant_id);
```

**SQL Server:**
```sql
CREATE TABLE [dbo].[VolumetricUserSlots] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [VolumetricLicenseId] UNIQUEIDENTIFIER NOT NULL,
    [SlotNumber] INT NOT NULL, -- 1 to 9999
    [UserKey] NVARCHAR(19) NOT NULL, -- XXXX-XXXX-XXXX-0001
    [UserId] NVARCHAR(255) NULL,
    [MachineId] NVARCHAR(255) NULL,
    [MachineName] NVARCHAR(255) NULL,
    [MachineFingerprint] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [FirstActivation] DATETIMEOFFSET NULL,
    [LastActivity] DATETIMEOFFSET NULL,
    [IsCurrentlyActive] BIT NOT NULL DEFAULT 0,
    [ActivationData] NVARCHAR(MAX) DEFAULT '{}',
    [SessionData] NVARCHAR(MAX) DEFAULT '{}',
    
    -- Audit fields
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    
    -- Foreign key constraints
    FOREIGN KEY ([VolumetricLicenseId]) REFERENCES [dbo].[VolumetricLicenses]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]) ON DELETE NO ACTION,
    
    -- Unique and check constraints
    CONSTRAINT [UK_VolumetricUserSlots_UserKey] UNIQUE ([UserKey]),
    CONSTRAINT [UK_VolumetricUserSlots_LicenseSlot] UNIQUE ([VolumetricLicenseId], [SlotNumber]),
    CONSTRAINT [CHK_VolumetricUserSlots_SlotRange] CHECK ([SlotNumber] >= 1 AND [SlotNumber] <= 9999)
);

-- Indexes
CREATE INDEX [IX_VolumetricUserSlots_VolumetricLicense] ON [dbo].[VolumetricUserSlots]([VolumetricLicenseId]);
CREATE INDEX [IX_VolumetricUserSlots_UserKey] ON [dbo].[VolumetricUserSlots]([UserKey]);
CREATE INDEX [IX_VolumetricUserSlots_MachineId] ON [dbo].[VolumetricUserSlots]([MachineId]) WHERE [MachineId] IS NOT NULL;
CREATE INDEX [IX_VolumetricUserSlots_Activity] ON [dbo].[VolumetricUserSlots]([LastActivity]) WHERE [LastActivity] IS NOT NULL;
CREATE INDEX [IX_VolumetricUserSlots_Active] ON [dbo].[VolumetricUserSlots]([IsCurrentlyActive]) WHERE [IsCurrentlyActive] = 1;
CREATE INDEX [IX_VolumetricUserSlots_Tenant] ON [dbo].[VolumetricUserSlots]([TenantId]);
```

### Phase 4: License File Management

#### 4.1 Create License Files Table

**PostgreSQL:**
```sql
CREATE TABLE license_files (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    license_id UUID NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size_bytes BIGINT NOT NULL DEFAULT 0,
    content_type VARCHAR(100) NOT NULL DEFAULT 'application/json',
    file_hash VARCHAR(128) NOT NULL, -- SHA-256 hash for integrity
    download_count INTEGER NOT NULL DEFAULT 0,
    last_downloaded_at TIMESTAMP WITH TIME ZONE NULL,
    last_downloaded_by VARCHAR(100) NULL,
    expires_at TIMESTAMP WITH TIME ZONE NULL,
    download_limit INTEGER NULL, -- NULL = unlimited
    is_download_enabled BOOLEAN NOT NULL DEFAULT true,
    file_version VARCHAR(20) NOT NULL DEFAULT '1.0',
    file_type VARCHAR(50) NOT NULL DEFAULT 'JSON',
    encryption VARCHAR(50) NOT NULL DEFAULT 'AES256',
    signature VARCHAR(50) NOT NULL DEFAULT 'SHA256',
    license_signature VARCHAR(2000) NOT NULL DEFAULT '',
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE,
    deleted_by VARCHAR(100),
    deleted_on TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT fk_license_files_license_id 
        FOREIGN KEY (license_id) REFERENCES product_licenses(id) ON DELETE CASCADE,
    CONSTRAINT fk_license_files_tenant_id 
        FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE RESTRICT,
    CONSTRAINT chk_license_files_download_limit 
        CHECK (download_limit IS NULL OR download_limit > 0)
);

-- Indexes
CREATE INDEX idx_license_files_license_id ON license_files(license_id);
CREATE INDEX idx_license_files_file_hash ON license_files(file_hash);
CREATE INDEX idx_license_files_download_tracking ON license_files(last_downloaded_at, download_count);
CREATE INDEX idx_license_files_tenant ON license_files(tenant_id);
```

**SQL Server:**
```sql
CREATE TABLE [dbo].[LicenseFiles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    [LicenseId] UNIQUEIDENTIFIER NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [FilePath] NVARCHAR(500) NOT NULL,
    [FileSizeBytes] BIGINT NOT NULL DEFAULT 0,
    [ContentType] NVARCHAR(100) NOT NULL DEFAULT 'application/json',
    [FileHash] NVARCHAR(128) NOT NULL, -- SHA-256 hash for integrity
    [DownloadCount] INT NOT NULL DEFAULT 0,
    [LastDownloadedAt] DATETIMEOFFSET NULL,
    [LastDownloadedBy] NVARCHAR(100) NULL,
    [ExpiresAt] DATETIMEOFFSET NULL,
    [DownloadLimit] INT NULL, -- NULL = unlimited
    [IsDownloadEnabled] BIT NOT NULL DEFAULT 1,
    [FileVersion] NVARCHAR(20) NOT NULL DEFAULT '1.0',
    [FileType] NVARCHAR(50) NOT NULL DEFAULT 'JSON',
    [Encryption] NVARCHAR(50) NOT NULL DEFAULT 'AES256',
    [Signature] NVARCHAR(50) NOT NULL DEFAULT 'SHA256',
    [LicenseSignature] NVARCHAR(2000) NOT NULL DEFAULT '',
    
    -- Audit fields
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] NVARCHAR(100) NULL,
    [UpdatedOn] DATETIMEOFFSET NULL,
    [DeletedBy] NVARCHAR(100) NULL,
    [DeletedOn] DATETIMEOFFSET NULL,
    [RowVersion] ROWVERSION,
    
    -- Foreign key constraints
    FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[ProductLicenses]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([TenantId]) ON DELETE NO ACTION,
    
    -- Check constraints
    CONSTRAINT [CHK_LicenseFiles_DownloadLimit] CHECK ([DownloadLimit] IS NULL OR [DownloadLimit] > 0)
);

-- Indexes
CREATE INDEX [IX_LicenseFiles_LicenseId] ON [dbo].[LicenseFiles]([LicenseId]);
CREATE INDEX [IX_LicenseFiles_FileHash] ON [dbo].[LicenseFiles]([FileHash]);
CREATE INDEX [IX_LicenseFiles_Download] ON [dbo].[LicenseFiles]([LastDownloadedAt], [DownloadCount]);
CREATE INDEX [IX_LicenseFiles_Tenant] ON [dbo].[LicenseFiles]([TenantId]);
```

### Phase 5: Enhanced Entity Framework Configuration Updates

#### 5.1 Update EfCoreLicensingDbContext

The `EfCoreLicensingDbContext` needs to be updated to include the new DbSets:

```csharp
// Add to EfCoreLicensingDbContext.cs in DbSets region

// License activation and management entities
public DbSet<ProductActivationEntity> ProductActivations { get; set; }
public DbSet<VolumetricLicenseEntity> VolumetricLicenses { get; set; }
public DbSet<VolumetricUserSlotEntity> VolumetricUserSlots { get; set; }
public DbSet<LicenseFileEntity> LicenseFiles { get; set; }
```

#### 5.2 Entity Configuration Classes

Create new entity configuration classes:

**ProductActivationEntityConfiguration.cs:**
```csharp
public class ProductActivationEntityConfiguration : IEntityTypeConfiguration<ProductActivationEntity>
{
    public void Configure(EntityTypeBuilder<ProductActivationEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ProductKey).HasMaxLength(19).IsRequired();
        builder.Property(e => e.MachineId).HasMaxLength(255).IsRequired();
        builder.Property(e => e.MachineName).HasMaxLength(255);
        builder.Property(e => e.MachineFingerprint).HasMaxLength(500);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        
        // Relationships
        builder.HasOne(e => e.License)
               .WithMany(l => l.Activations)
               .HasForeignKey(e => e.LicenseId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // Indexes
        builder.HasIndex(e => e.ProductKey);
        builder.HasIndex(e => e.MachineId);
        builder.HasIndex(e => new { e.LicenseId, e.MachineId }).IsUnique();
    }
}
```

**VolumetricLicenseEntityConfiguration.cs:**
```csharp
public class VolumetricLicenseEntityConfiguration : IEntityTypeConfiguration<VolumetricLicenseEntity>
{
    public void Configure(EntityTypeBuilder<VolumetricLicenseEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BaseKey).HasMaxLength(14).IsRequired();
        builder.Property(e => e.MaxConcurrentUsers).IsRequired();
        builder.Property(e => e.MaxTotalUsers).IsRequired();
        
        // Relationships
        builder.HasOne(e => e.License)
               .WithOne(l => l.VolumetricLicense)
               .HasForeignKey<VolumetricLicenseEntity>(e => e.LicenseId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // Indexes
        builder.HasIndex(e => e.BaseKey).IsUnique();
        builder.HasIndex(e => e.LicenseId);
    }
}
```

#### 5.3 Update OnModelCreating Method

Add to the `OnModelCreating` method in `EfCoreLicensingDbContext`:

```csharp
// License activation and management entities
modelBuilder.ApplyConfiguration(new ProductActivationEntityConfiguration());
modelBuilder.ApplyConfiguration(new VolumetricLicenseEntityConfiguration());
modelBuilder.ApplyConfiguration(new VolumetricUserSlotEntityConfiguration());
modelBuilder.ApplyConfiguration(new LicenseFileEntityConfiguration());
```

#### 5.4 Update Global Query Filters

Add tenant filtering for new entities in `ConfigureGlobalQueryFilters`:

```csharp
// License activation and management entities
modelBuilder.Entity<ProductActivationEntity>().HasQueryFilter(e => GetCurrentTenantId() == IdConstants.SystemTenantId || e.TenantId == GetCurrentTenantId());
modelBuilder.Entity<VolumetricLicenseEntity>().HasQueryFilter(e => GetCurrentTenantId() == IdConstants.SystemTenantId || e.TenantId == GetCurrentTenantId());
modelBuilder.Entity<VolumetricUserSlotEntity>().HasQueryFilter(e => GetCurrentTenantId() == IdConstants.SystemTenantId || e.TenantId == GetCurrentTenantId());
modelBuilder.Entity<LicenseFileEntity>().HasQueryFilter(e => GetCurrentTenantId() == IdConstants.SystemTenantId || e.TenantId == GetCurrentTenantId());
```

#### 5.5 Update Tenant Indexes

Add tenant indexes in `ConfigureTenantIndexes`:

```csharp
// License activation and management entities
modelBuilder.Entity<ProductActivationEntity>().HasIndex(e => e.TenantId);
modelBuilder.Entity<VolumetricLicenseEntity>().HasIndex(e => e.TenantId);
modelBuilder.Entity<VolumetricUserSlotEntity>().HasIndex(e => e.TenantId);
modelBuilder.Entity<LicenseFileEntity>().HasIndex(e => e.TenantId);
```

### Phase 6: Migration Scripts Summary

#### 6.1 Complete Migration Script for PostgreSQL

```sql
-- License Type Implementation Migration - PostgreSQL
-- Version: 2.0.0
-- Date: August 2025

BEGIN;

-- Step 1: Add new columns to existing product_licenses table
ALTER TABLE product_licenses 
ADD COLUMN license_type VARCHAR(50) NOT NULL DEFAULT 'ProductLicenseFile',
ADD COLUMN license_file_path VARCHAR(500) NULL,
ADD COLUMN license_file_name VARCHAR(255) NULL,
ADD COLUMN download_count INTEGER NOT NULL DEFAULT 0,
ADD COLUMN last_downloaded_at TIMESTAMP WITH TIME ZONE NULL,
ADD COLUMN last_downloaded_by VARCHAR(100) NULL,
ADD COLUMN formatted_product_key VARCHAR(19) NULL,
ADD COLUMN max_activations INTEGER NULL,
ADD COLUMN current_activations INTEGER NOT NULL DEFAULT 0,
ADD COLUMN base_volumetric_key VARCHAR(14) NULL,
ADD COLUMN max_concurrent_users INTEGER NULL,
ADD COLUMN max_total_users INTEGER NULL,
ADD COLUMN current_active_users INTEGER NOT NULL DEFAULT 0,
ADD COLUMN total_allocated_users INTEGER NOT NULL DEFAULT 0;

-- Step 2: Create new tables
-- (Include all CREATE TABLE statements from above)

-- Step 3: Create indexes
-- (Include all CREATE INDEX statements from above)

-- Step 4: Update schema version
INSERT INTO schema_versions (version, description, applied_by)
VALUES ('2.0.0', 'License type implementation with three license types support', 'system')
ON CONFLICT (version) DO NOTHING;

COMMIT;
```

#### 6.2 Complete Migration Script for SQL Server

```sql
-- License Type Implementation Migration - SQL Server
-- Version: 2.0.0
-- Date: August 2025

BEGIN TRANSACTION;

-- Step 1: Add new columns to existing ProductLicenses table
ALTER TABLE [dbo].[ProductLicenses] 
ADD [LicenseType] NVARCHAR(50) NOT NULL DEFAULT 'ProductLicenseFile',
    [LicenseFilePath] NVARCHAR(500) NULL,
    [LicenseFileName] NVARCHAR(255) NULL,
    [DownloadCount] INT NOT NULL DEFAULT 0,
    [LastDownloadedAt] DATETIMEOFFSET NULL,
    [LastDownloadedBy] NVARCHAR(100) NULL,
    [FormattedProductKey] NVARCHAR(19) NULL,
    [MaxActivations] INT NULL,
    [CurrentActivations] INT NOT NULL DEFAULT 0,
    [BaseVolumetricKey] NVARCHAR(14) NULL,
    [MaxConcurrentUsers] INT NULL,
    [MaxTotalUsers] INT NULL,
    [CurrentActiveUsers] INT NOT NULL DEFAULT 0,
    [TotalAllocatedUsers] INT NOT NULL DEFAULT 0;

-- Step 2: Create new tables
-- (Include all CREATE TABLE statements from above)

-- Step 3: Create indexes
-- (Include all CREATE INDEX statements from above)

COMMIT TRANSACTION;
```

## Implementation Priority and Dependencies

### Priority 1 (Must Have)
1. **Core License Type Support** - Modify existing `product_licenses` table
2. **Product Key Activation** - Create `product_activations` table
3. **Entity Framework Updates** - Update DbContext and configurations

### Priority 2 (Should Have)
4. **Volumetric License Management** - Create volumetric tables
5. **License File Management** - Create license files table

### Priority 3 (Nice to Have)
6. **Performance Optimizations** - Additional indexes and constraints
7. **Audit Enhancements** - Extended tracking capabilities

## Performance Considerations

1. **Indexing Strategy**: All new tables include comprehensive indexing for common query patterns
2. **Partitioning**: Consider partitioning for `product_activations` and `volumetric_user_slots` tables if high volume expected
3. **Cleanup Jobs**: Implement automated cleanup for inactive sessions and expired activations
4. **Caching**: Use appropriate caching strategies for frequently accessed license validation data

## Security Considerations

1. **Data Encryption**: Consider encrypting sensitive fields like machine fingerprints and activation data
2. **Access Control**: Ensure proper tenant isolation and role-based access
3. **Audit Trail**: All tables include comprehensive audit fields
4. **Key Management**: Secure storage and rotation of cryptographic keys

## Rollback Strategy

1. **Backup Requirements**: Full database backup before migration
2. **Rollback Scripts**: Prepare DROP statements for new tables and columns
3. **Data Migration**: Consider data migration scripts for existing licenses
4. **Testing**: Comprehensive testing in staging environment

This comprehensive database change document provides all the necessary schema modifications to support the three license types while maintaining compatibility with the existing system architecture.

## Field Reorganization and Separation of Concerns

### ProductLicenseEntity Cleanup
The following fields have been moved from `product_licenses` table to their appropriate specialized tables to ensure proper separation of concerns:

#### Moved to LicenseFileEntity (product_license_files table):
- `encryption` → `encryption`
- `signature` → `signature` 
- `license_signature` → `license_signature`

#### Moved to ProductActivationEntity (product_activations table):
- `formatted_product_key` → `formatted_product_key`
- `max_activations` → `max_activations`

#### Moved to VolumetricLicenseEntity (volumetric_licenses table):
- `base_volumetric_key` → `base_volumetric_key`
- `max_concurrent_users` → `max_concurrent_users`
- `max_total_users` → `max_total_users`

### Rationale:
- **ProductLicenseEntity**: Now contains only immutable core license data shared across all license types
- **LicenseFileEntity**: Contains file-specific properties including security fields
- **ProductActivationEntity**: Contains product key specific configuration and tracking
- **VolumetricLicenseEntity**: Contains volumetric license specific configuration and user management

### Database Schema Impact:
1. **product_licenses table**: Remove license-type-specific columns
2. **product_activations table**: Add `formatted_product_key` and `max_activations` columns
3. **volumetric_licenses table**: Ensure `base_volumetric_key`, `max_concurrent_users`, and `max_total_users` columns exist
4. **product_license_files table**: Add `encryption`, `signature`, and `license_signature` columns

This reorganization ensures each entity has a single responsibility and improves maintainability and data integrity.

```
