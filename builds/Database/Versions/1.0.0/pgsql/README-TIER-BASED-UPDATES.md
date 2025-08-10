# Database Schema Updates for Tier-Based Licensing

## Overview

The database schema has been updated to support tier-based licensing while maintaining backward compatibility with the existing feature-based licensing system.

## Files Updated

### 1. Initial Schema (v1.0.0)

- **File**: `v1.0.0-initial-schema-corrected.sql`
- **Status**: ✅ **UPDATED** - Enhanced with tier-based licensing support
- **Description**: The original initial schema has been updated to include the new tier-based licensing columns from the start

### 2. New Complete Schema (v1.2.0)

- **File**: `v1.2.0-initial-schema-tier-based.sql`
- **Status**: ✅ **NEW** - Complete schema for new deployments
- **Description**: A complete schema file that includes all tier-based licensing features, enhanced views, and validation functions

## Key Changes Made

### Schema Enhancements

#### 1. Product Licenses Table Updates

**New Columns Added:**
```sql
-- Tier-based licensing support
product_tier_id UUID NULL,

-- Version range support  
valid_product_version_from VARCHAR(20) NOT NULL DEFAULT '1.0.0',
valid_product_version_to VARCHAR(20) NULL,
```

**New Foreign Key Constraint:**
```sql
CONSTRAINT fk_product_licenses_product_tier_id 
    FOREIGN KEY (product_tier_id) REFERENCES product_tiers(id) ON DELETE SET NULL
```

#### 2. Enhanced Indexes

**New Tier-Based Licensing Indexes:**
```sql
-- Optimize tier-based license queries
CREATE INDEX idx_product_licenses_tier_id ON product_licenses(product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_product_tier ON product_licenses(product_id, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_status_tier ON product_licenses(status, product_tier_id) WHERE product_tier_id IS NOT NULL;
CREATE INDEX idx_product_licenses_version_range ON product_licenses(valid_product_version_from, valid_product_version_to);
```

#### 3. Enhanced Views

**Updated Active Licenses View:**
- Added tier information
- Added license type classification (Tier-Based vs Feature-Based)
- Added version range information

**New Enhanced Views (v1.2.0 only):**
- `v_enhanced_product_licenses` - Complete license view with tier information
- `v_product_tier_features` - Summary of features by tier

#### 4. Product Tier Entity Navigation

**ProductTierEntity** now includes:
```sql
-- Navigation property for licenses using this tier
-- (Handled by Entity Framework relationships)
```

## Migration Strategy

### For New Deployments
- Use `v1.2.0-initial-schema-tier-based.sql` for complete tier-based licensing support

### For Existing Deployments
1. First deploy the current updated `v1.0.0-initial-schema-corrected.sql` (if starting fresh)
2. Or use the migration scripts in `Database/Migrations/v1.1.0-to-v1.2.0/` to upgrade existing databases

## Backward Compatibility

The updated schema maintains full backward compatibility:

1. **Feature-Based Licenses**: Continue to work through the `product_license_features` junction table
2. **Tier-Based Licenses**: Use the new `product_tier_id` foreign key
3. **Mixed Environment**: Both licensing models can coexist in the same database

## License Type Detection

The system automatically detects license types:

```sql
CASE 
    WHEN pl.product_tier_id IS NOT NULL THEN 'Tier-Based'
    ELSE 'Feature-Based'
END AS license_type
```

## Version Range Support

Licenses now support version compatibility ranges:

- **valid_product_version_from**: Minimum supported version (required, defaults to '1.0.0')
- **valid_product_version_to**: Maximum supported version (optional, NULL means no upper limit)

## Entity Framework Integration

The database changes align with the updated entities:

1. **ProductLicense**: Added `ProductTierId`, `ValidProductVersionFrom`, `ValidProductVersionTo`
2. **ProductLicenseEntity**: Enhanced mapping with tier support
3. **ProductTierEntity**: Added navigation property for licenses
4. **LicenseGenerationRequest**: Added tier and version properties

## Validation Functions

The v1.2.0 schema includes built-in validation functions:

```sql
-- Validate the tier-based licensing implementation
SELECT * FROM validate_tier_licensing_migration();
```

## Performance Considerations

1. **Conditional Indexes**: New indexes only apply to tier-based licenses (`WHERE product_tier_id IS NOT NULL`)
2. **Query Optimization**: Enhanced views provide optimized queries for common scenarios
3. **Foreign Key Strategy**: `ON DELETE SET NULL` for tier relationships to prevent data loss

## Testing

The schema includes built-in testing capabilities:

1. **Data Validation**: Automatic validation of schema changes
2. **Performance Testing**: Built-in query performance analysis
3. **Sample Data**: Ability to create test tier-based licenses

## Next Steps

1. **Application Code**: Update services to use the new tier-based properties
2. **UI Updates**: Enhance license creation forms to support tier selection
3. **Migration Deployment**: Apply changes to existing databases
4. **Testing**: Validate the new tier-based licensing workflow

## Support

For questions or issues with the database schema updates, refer to:

1. Migration logs and validation output
2. Built-in validation functions
3. Enhanced views for data verification
4. Performance monitoring through new indexes
