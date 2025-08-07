#!/bin/bash

# Script to update namespaces from PostgreSql to EntityFramework
PROJECT_DIR="TechWayFit.Licensing.Management.Infrastructure.EntityFramework"

echo "Updating namespaces in $PROJECT_DIR..."

# Update all .cs files
find "$PROJECT_DIR" -name "*.cs" -type f -exec sed -i '' 's/TechWayFit\.Licensing\.Management\.Infrastructure\.PostgreSql/TechWayFit.Licensing.Management.Infrastructure.EntityFramework/g' {} \;

# Update project file name
mv "$PROJECT_DIR/TechWayFit.Licensing.Management.Infrastructure.PostgreSql.csproj" "$PROJECT_DIR/TechWayFit.Licensing.Management.Infrastructure.EntityFramework.csproj" 2>/dev/null || true

# Update project file content
if [ -f "$PROJECT_DIR/TechWayFit.Licensing.Management.Infrastructure.EntityFramework.csproj" ]; then
    sed -i '' 's/TechWayFit\.Licensing\.Management\.Infrastructure\.PostgreSql/TechWayFit.Licensing.Management.Infrastructure.EntityFramework/g' "$PROJECT_DIR/TechWayFit.Licensing.Management.Infrastructure.EntityFramework.csproj"
fi

# Update any remaining references in other files
find "$PROJECT_DIR" -name "*.json" -o -name "*.xml" -o -name "*.md" -type f -exec sed -i '' 's/TechWayFit\.Licensing\.Management\.Infrastructure\.PostgreSql/TechWayFit.Licensing.Management.Infrastructure.EntityFramework/g' {} \; 2>/dev/null || true

echo "Namespace update completed!"
echo "Updated files:"
find "$PROJECT_DIR" -name "*.cs" -type f | head -10
echo "..."
