#!/bin/bash

echo "Updating all remaining PostgreSQL repositories..."

# Update AuditEntryRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Audit/PostgreSqlAuditEntryRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.Audit;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Audit;/g' "$file"
sed -i '' 's/public class AuditEntryRepository/public class PostgreSqlAuditEntryRepository/g' "$file"
sed -i '' 's/public AuditEntryRepository(LicensingDbContext context)/public PostgreSqlAuditEntryRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories;//' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

# Update SettingRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Settings/PostgreSqlSettingRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Settings;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Settings;/g' "$file"
sed -i '' 's/public class SettingRepository/public class PostgreSqlSettingRepository/g' "$file"
sed -i '' 's/public SettingRepository(LicensingDbContext context)/public PostgreSqlSettingRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

# Update UserRoleRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/User/PostgreSqlUserRoleRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.User;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.User;/g' "$file"
sed -i '' 's/public class UserRoleRepository/public class PostgreSqlUserRoleRepository/g' "$file"
sed -i '' 's/public UserRoleRepository(LicensingDbContext context)/public PostgreSqlUserRoleRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories;//' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

# Update UserRoleMappingRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/User/PostgreSqlUserRoleMappingRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.User;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.User;/g' "$file"
sed -i '' 's/public class UserRoleMappingRepository/public class PostgreSqlUserRoleMappingRepository/g' "$file"
sed -i '' 's/public UserRoleMappingRepository(LicensingDbContext context)/public PostgreSqlUserRoleMappingRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories;//' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

# Update NotificationTemplateRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Notification/PostgreSqlNotificationTemplateRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Notification;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Notification;/g' "$file"
sed -i '' 's/public class NotificationTemplateRepository/public class PostgreSqlNotificationTemplateRepository/g' "$file"
sed -i '' 's/public NotificationTemplateRepository(LicensingDbContext context)/public PostgreSqlNotificationTemplateRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

# Update NotificationHistoryRepository
file="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Notification/PostgreSqlNotificationHistoryRepository.cs"
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Notification;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Notification;/g' "$file"
sed -i '' 's/public class NotificationHistoryRepository/public class PostgreSqlNotificationHistoryRepository/g' "$file"
sed -i '' 's/public NotificationHistoryRepository(LicensingDbContext context)/public PostgreSqlNotificationHistoryRepository(PostgreSqlLicensingDbContext context)/g' "$file"
sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' "$file"
sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' "$file"

echo "All repository updates completed!"
