#!/bin/bash

echo "Fixing remaining constructor and DbContext references..."

# Find and fix all remaining LicensingDbContext references
find /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql -name "*.cs" -exec sed -i '' 's/LicensingDbContext/PostgreSqlLicensingDbContext/g' {} \;

# Find and fix remaining BaseRepository references  
find /Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management/TechWayFit.Licensing.Management.Infrastructure.PostgreSql -name "*.cs" -exec sed -i '' 's/using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories;//g' {} \;

echo "All fixes applied!"
