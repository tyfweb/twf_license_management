#!/bin/bash

# Script to recreate Operations Dashboard tables
# Run this from the project root directory

echo "Recreating Operations Dashboard tables..."

# Check if PostgreSQL client is available
if command -v psql &> /dev/null; then
    echo "Using psql client..."
    psql "postgresql://twf_license_user:M@n@s0000@localhost:5433/twf_license_management" -f Database/Scripts/create-operations-dashboard-tables.sql
elif command -v docker &> /dev/null; then
    echo "Using Docker to execute SQL..."
    docker exec -i $(docker ps -q --filter "expose=5433" 2>/dev/null | head -1) psql -U twf_license_user -d twf_license_management < Database/Scripts/create-operations-dashboard-tables.sql 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "Trying alternative Docker approach..."
        docker exec -i $(docker ps -q --filter "name=postgres" 2>/dev/null | head -1) psql -U twf_license_user -d twf_license_management < Database/Scripts/create-operations-dashboard-tables.sql 2>/dev/null
    fi
else
    echo "Neither psql nor docker found. Please install PostgreSQL client or Docker."
    echo "Manual command:"
    echo "psql \"postgresql://twf_license_user:M@n@s0000@localhost:5433/twf_license_management\" -f Database/Scripts/create-operations-dashboard-tables.sql"
    exit 1
fi

if [ $? -eq 0 ]; then
    echo "✅ Operations Dashboard tables created successfully!"
else
    echo "❌ Failed to create tables. Please check the connection and try manually:"
    echo "psql \"postgresql://twf_license_user:M@n@s0000@localhost:5433/twf_license_management\" -f Database/Scripts/create-operations-dashboard-tables.sql"
    exit 1
fi
