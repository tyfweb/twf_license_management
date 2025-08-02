#!/bin/bash

# Script to update PostgreSQL repository namespaces and class names

echo "Updating PostgreSQL repositories..."

# Update ProductFeatureRepository
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductFeatureRepository.cs

sed -i '' 's/public class ProductFeatureRepository/public class PostgreSqlProductFeatureRepository/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductFeatureRepository.cs

sed -i '' 's/public ProductFeatureRepository(LicensingDbContext context)/public PostgreSqlProductFeatureRepository(PostgreSqlLicensingDbContext context)/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductFeatureRepository.cs

sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductFeatureRepository.cs

sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductFeatureRepository.cs

# Update ProductTierRepository
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductTierRepository.cs

sed -i '' 's/public class ProductTierRepository/public class PostgreSqlProductTierRepository/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductTierRepository.cs

sed -i '' 's/public ProductTierRepository(LicensingDbContext context)/public PostgreSqlProductTierRepository(PostgreSqlLicensingDbContext context)/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductTierRepository.cs

sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductTierRepository.cs

sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductTierRepository.cs

# Update ProductVersionRepository
sed -i '' 's/namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;/namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductVersionRepository.cs

sed -i '' 's/public class ProductVersionRepository/public class PostgreSqlProductVersionRepository/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductVersionRepository.cs

sed -i '' 's/public ProductVersionRepository(LicensingDbContext context)/public PostgreSqlProductVersionRepository(PostgreSqlLicensingDbContext context)/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductVersionRepository.cs

sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Context;/using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;\nusing TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;/g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductVersionRepository.cs

sed -i '' 's/: BaseRepository</: PostgreSqlBaseRepository</g' /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/Repositories/Product/PostgreSqlProductVersionRepository.cs

echo "Repository updates completed!"
