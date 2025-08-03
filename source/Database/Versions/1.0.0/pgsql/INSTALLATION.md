# PostgreSQL Installation Guide

This folder contains PostgreSQL database scripts for the TechWayFit Licensing Management System v1.0.0.

## Files

1. **`v1.0.0-initial-schema.sql`** - Main database schema creation script
2. **`twf_license_user_permissions.sql`** - User creation and permissions script
3. **`README.md`** - Complete documentation

## Installation Instructions

### Prerequisites
- PostgreSQL 12+ installed and running
- PostgreSQL superuser access (postgres user)
- Database created for the application

### Step 1: Create Database (if not exists)
```sql
-- Connect as postgres superuser
CREATE DATABASE twf_license_management 
    WITH ENCODING 'UTF8' 
    LC_COLLATE='en_US.UTF-8' 
    LC_CTYPE='en_US.UTF-8';
```

### Step 2: Run Schema Creation
```bash
# Connect to your database and run the schema script
psql -U postgres -d twf_license_management -f v1.0.0-initial-schema.sql
```

### Step 3: Configure Application User
```bash
# Before running, edit twf_license_user_permissions.sql:
# 1. Replace 'your_secure_password' with a strong password
# 2. Replace 'twf_license_management' with your actual database name

# Run the permissions script
psql -U postgres -d twf_license_management -f twf_license_user_permissions.sql
```

### Step 4: Verify Installation
```sql
-- Verify tables were created
\dt

-- Verify user was created
\du twf_license_user

-- Test user permissions
SET ROLE twf_license_user;
SELECT COUNT(*) FROM products; -- Should work without errors
```

## Connection String Example

For your application configuration:

```csharp
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=twf_license_management;Username=twf_license_user;Password=your_secure_password;SSL Mode=Prefer"
}
```

## Security Notes

1. **Change the default password** in `twf_license_user_permissions.sql`
2. Use SSL connections in production
3. Configure `pg_hba.conf` for IP restrictions
4. Store passwords securely (environment variables, Azure Key Vault)
5. Monitor database activity and audit logs

## Troubleshooting

### Common Issues:

1. **Permission denied errors**
   - Ensure you're running as PostgreSQL superuser
   - Check if database exists

2. **User already exists**
   - The script handles this gracefully with a notice

3. **Connection issues**
   - Verify PostgreSQL is running
   - Check firewall settings
   - Verify connection parameters

### Verification Queries:

```sql
-- Check user permissions
SELECT grantee, table_name, privilege_type
FROM information_schema.table_privileges
WHERE grantee = 'twf_license_user'
ORDER BY table_name;

-- Check user settings
SELECT rolname, rolconnlimit, rolcanlogin
FROM pg_roles 
WHERE rolname = 'twf_license_user';
```

## Next Steps

After successful installation:
1. Configure your application connection string
2. Run Entity Framework migrations (if using EF Core)
3. Create initial admin user and roles
4. Configure system settings
5. Set up monitoring and backup procedures
