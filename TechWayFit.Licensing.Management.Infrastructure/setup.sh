#!/bin/bash

# TechWayFit Licensing Infrastructure Setup Script
# This script helps set up the database and run initial migrations

set -e

echo "🚀 TechWayFit Licensing Infrastructure Setup"
echo "============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRASTRUCTURE_PROJECT="$PROJECT_DIR/TechWayFit.Licensing.Infrastructure.csproj"
MIGRATIONS_DIR="$PROJECT_DIR/Migrations"

echo -e "${BLUE}Project Directory: $PROJECT_DIR${NC}"

# Function to print status
print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET CLI is not installed. Please install .NET 8.0 or later."
    exit 1
fi

print_status ".NET CLI is available"

# Check if Entity Framework tools are installed
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    print_warning "Entity Framework tools not found globally. Installing..."
    dotnet tool install --global dotnet-ef
    print_status "Entity Framework tools installed"
else
    print_status "Entity Framework tools are available"
fi

# Restore packages
echo -e "\n${BLUE}Restoring NuGet packages...${NC}"
dotnet restore "$INFRASTRUCTURE_PROJECT"
print_status "NuGet packages restored"

# Build the project
echo -e "\n${BLUE}Building infrastructure project...${NC}"
dotnet build "$INFRASTRUCTURE_PROJECT" --no-restore
if [ $? -eq 0 ]; then
    print_status "Project built successfully"
else
    print_error "Build failed. Please check the errors above."
    exit 1
fi

# Check for migrations directory
echo -e "\n${BLUE}Checking migrations...${NC}"
if [ ! -d "$MIGRATIONS_DIR" ]; then
    print_warning "No migrations directory found. Creating initial migration..."
    
    # Create initial migration
    dotnet ef migrations add InitialCreate \
        --project "$INFRASTRUCTURE_PROJECT" \
        --startup-project "$INFRASTRUCTURE_PROJECT" \
        --output-dir Migrations \
        --context LicensingDbContext
    
    if [ $? -eq 0 ]; then
        print_status "Initial migration created"
    else
        print_error "Failed to create initial migration"
        exit 1
    fi
else
    print_status "Migrations directory exists"
fi

# List migrations
echo -e "\n${BLUE}Available migrations:${NC}"
dotnet ef migrations list \
    --project "$INFRASTRUCTURE_PROJECT" \
    --startup-project "$INFRASTRUCTURE_PROJECT" \
    --context LicensingDbContext

echo -e "\n${GREEN}🎉 Infrastructure setup completed!${NC}"
echo -e "\n${BLUE}Next steps:${NC}"
echo "1. Update your connection string in appsettings.json"
echo "2. Run database update: dotnet ef database update"
echo "3. Test the infrastructure with the example console app"

echo -e "\n${BLUE}Example connection string for PostgreSQL:${NC}"
echo "Host=localhost;Database=licensing_dev;Username=postgres;Password=your_password;Port=5432"

echo -e "\n${BLUE}To update the database:${NC}"
echo "cd $PROJECT_DIR"
echo "dotnet ef database update --context LicensingDbContext"

echo -e "\n${BLUE}To add a new migration:${NC}"
echo "dotnet ef migrations add YourMigrationName --context LicensingDbContext"

echo -e "\n${BLUE}To rollback migrations:${NC}"
echo "dotnet ef database update PreviousMigrationName --context LicensingDbContext"

echo -e "\n${YELLOW}Remember to:${NC}"
echo "- Configure your PostgreSQL connection string"
echo "- Ensure PostgreSQL server is running"
echo "- Create the database if it doesn't exist"
echo "- Test the connection before running migrations"
