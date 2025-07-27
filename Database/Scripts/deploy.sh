#!/bin/bash

# TechWayFit Licensing Database Deployment Script
# Usage: ./deploy.sh [environment] [version]
# Example: ./deploy.sh production v1.0.0

set -e  # Exit on any error

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATABASE_DIR="$(dirname "$SCRIPT_DIR")"
VERSIONS_DIR="$DATABASE_DIR/Versions"
MIGRATIONS_DIR="$DATABASE_DIR/Migrations"

# Default values
ENVIRONMENT=${1:-development}
VERSION=${2:-v1.0.0}

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Load environment-specific configuration
load_config() {
    local config_file="$DATABASE_DIR/config/$ENVIRONMENT.env"
    
    if [[ -f "$config_file" ]]; then
        source "$config_file"
        log_info "Loaded configuration for environment: $ENVIRONMENT"
    else
        log_warning "No configuration file found for environment: $ENVIRONMENT"
        log_info "Using default PostgreSQL connection settings"
        
        # Default settings
        export DB_HOST=${DB_HOST:-localhost}
        export DB_PORT=${DB_PORT:-5432}
        export DB_NAME=${DB_NAME:-techwayfit_licensing}
        export DB_USER=${DB_USER:-twf_license_user}
        export DB_PASSWORD=${DB_PASSWORD:-M@n@s0000}
    fi
}

# Check if database exists
check_database_exists() {
    log_info "Checking if database '$DB_NAME' exists..."
    
    if PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -lqt | cut -d \| -f 1 | grep -qw "$DB_NAME"; then
        return 0  # Database exists
    else
        return 1  # Database does not exist
    fi
}

# Create database if it doesn't exist
create_database() {
    log_info "Creating database '$DB_NAME'..."
    
    PGPASSWORD="$DB_PASSWORD" createdb -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" "$DB_NAME"
    
    if [[ $? -eq 0 ]]; then
        log_success "Database '$DB_NAME' created successfully"
    else
        log_error "Failed to create database '$DB_NAME'"
        exit 1
    fi
}

# Execute SQL script
execute_sql() {
    local sql_file="$1"
    local description="$2"
    
    log_info "Executing: $description"
    log_info "SQL File: $sql_file"
    
    if [[ ! -f "$sql_file" ]]; then
        log_error "SQL file not found: $sql_file"
        exit 1
    fi
    
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$sql_file"
    
    if [[ $? -eq 0 ]]; then
        log_success "Successfully executed: $description"
    else
        log_error "Failed to execute: $description"
        exit 1
    fi
}

# Deploy new installation
deploy_new_installation() {
    log_info "Deploying new installation with version: $VERSION"
    
    local schema_file="$VERSIONS_DIR/$VERSION-initial-schema.sql"
    
    if [[ ! -f "$schema_file" ]]; then
        log_error "Schema file not found: $schema_file"
        exit 1
    fi
    
    # Create database if it doesn't exist
    if ! check_database_exists; then
        create_database
    fi
    
    # Execute schema
    execute_sql "$schema_file" "Initial schema deployment ($VERSION)"
    
    # Execute seed data if available
    local seed_file="$DATABASE_DIR/Scripts/seed-data.sql"
    if [[ -f "$seed_file" ]]; then
        execute_sql "$seed_file" "Seed data"
    fi
    
    log_success "New installation completed successfully!"
}

# Upgrade existing installation
upgrade_installation() {
    local from_version="$1"
    local to_version="$2"
    
    log_info "Upgrading from $from_version to $to_version"
    
    local migration_dir="$MIGRATIONS_DIR/$from_version-to-$to_version"
    local upgrade_script="$migration_dir/001-upgrade.sql"
    
    if [[ ! -d "$migration_dir" ]]; then
        log_error "Migration directory not found: $migration_dir"
        exit 1
    fi
    
    if [[ ! -f "$upgrade_script" ]]; then
        log_error "Upgrade script not found: $upgrade_script"
        exit 1
    fi
    
    # Backup database before upgrade
    backup_database "$from_version"
    
    # Execute upgrade
    execute_sql "$upgrade_script" "Database upgrade ($from_version → $to_version)"
    
    log_success "Upgrade completed successfully!"
}

# Backup database
backup_database() {
    local version_tag="$1"
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local backup_file="$DATABASE_DIR/backups/${DB_NAME}_${version_tag}_${timestamp}.sql"
    
    log_info "Creating database backup: $backup_file"
    
    mkdir -p "$DATABASE_DIR/backups"
    
    PGPASSWORD="$DB_PASSWORD" pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" > "$backup_file"
    
    if [[ $? -eq 0 ]]; then
        log_success "Database backup created: $backup_file"
    else
        log_error "Failed to create database backup"
        exit 1
    fi
}

# Validate deployment
validate_deployment() {
    log_info "Validating deployment..."
    
    local validate_script="$DATABASE_DIR/Scripts/validate-schema.sql"
    
    if [[ -f "$validate_script" ]]; then
        execute_sql "$validate_script" "Schema validation"
    else
        log_warning "No validation script found, skipping validation"
    fi
    
    # Basic connectivity test
    log_info "Testing database connectivity..."
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 'Database connection successful' as status;"
    
    log_success "Deployment validation completed"
}

# Main deployment function
main() {
    log_info "=== TechWayFit Licensing Database Deployment ==="
    log_info "Environment: $ENVIRONMENT"
    log_info "Version: $VERSION"
    log_info "Timestamp: $(date)"
    
    # Load configuration
    load_config
    
    # Check if this is a new installation or upgrade
    if ! check_database_exists; then
        deploy_new_installation
    else
        log_info "Database exists, checking for upgrade path..."
        # For now, assume new installation. In future, add version detection
        log_warning "Upgrade detection not implemented yet. Treating as new installation."
        deploy_new_installation
    fi
    
    # Validate deployment
    validate_deployment
    
    log_success "=== Deployment completed successfully! ==="
}

# Script execution
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
