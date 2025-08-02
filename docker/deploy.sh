#!/bin/bash

# TechWayFit Licensing Management - Docker Deployment Script
# This script deploys the licensing management system using Docker Compose

set -e

echo "🚀 TechWayFit Licensing Docker Deployment Script"
echo "================================================"

# Configuration
ENVIRONMENT=${1:-"development"}
COMPOSE_FILE="docker-compose.yml"
PROJECT_NAME="techway-licensing"

case $ENVIRONMENT in
    "production"|"prod")
        COMPOSE_FILE="docker-compose.prod.yml"
        PROJECT_NAME="techway-licensing-prod"
        echo "🏭 Deploying to PRODUCTION environment"
        ;;
    "development"|"dev")
        COMPOSE_FILE="docker-compose.yml"
        PROJECT_NAME="techway-licensing-dev"
        echo "🔧 Deploying to DEVELOPMENT environment"
        ;;
    *)
        echo "❌ Invalid environment: $ENVIRONMENT"
        echo "Usage: $0 [development|production]"
        exit 1
        ;;
esac

echo "Compose File: $COMPOSE_FILE"
echo "Project Name: $PROJECT_NAME"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Check for environment file
if [ ! -f .env ]; then
    if [ -f .env.template ]; then
        echo "⚠️  No .env file found. Copying from .env.template..."
        cp .env.template .env
        echo "📝 Please edit .env file with your configuration before continuing."
        echo "Press any key to continue after editing .env file..."
        read -n 1 -s
    else
        echo "❌ No .env file found and no template available."
        exit 1
    fi
fi

# Function to check if services are healthy
check_health() {
    local service=$1
    local max_attempts=30
    local attempt=1
    
    echo "🔍 Checking health of $service..."
    
    while [ $attempt -le $max_attempts ]; do
        if docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" ps -q "$service" | xargs docker inspect --format='{{.State.Health.Status}}' | grep -q "healthy"; then
            echo "✅ $service is healthy"
            return 0
        fi
        
        echo "⏳ Attempt $attempt/$max_attempts - $service not healthy yet..."
        sleep 10
        ((attempt++))
    done
    
    echo "❌ $service failed to become healthy"
    return 1
}

# Pull latest images (for production)
if [ "$ENVIRONMENT" = "production" ] || [ "$ENVIRONMENT" = "prod" ]; then
    echo "📥 Pulling latest images..."
    docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" pull
fi

# Stop and remove existing containers
echo "🛑 Stopping existing containers..."
docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" down --remove-orphans

# Start services
echo "🚀 Starting services..."
docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" up -d

# Wait for database to be healthy
if check_health "postgres"; then
    echo "✅ Database is ready"
else
    echo "❌ Database failed to start properly"
    echo "📋 Database logs:"
    docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" logs postgres
    exit 1
fi

# Wait for web application to be healthy
if check_health "licensing-web"; then
    echo "✅ Web application is ready"
else
    echo "❌ Web application failed to start properly"
    echo "📋 Web application logs:"
    docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" logs licensing-web
    exit 1
fi

# Show status
echo ""
echo "📊 Service Status:"
docker-compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" ps

# Show useful information
echo ""
echo "🎉 Deployment completed successfully!"
echo ""
echo "📋 Access Information:"
if [ "$ENVIRONMENT" = "production" ] || [ "$ENVIRONMENT" = "prod" ]; then
    echo "   Web Application: https://localhost (via nginx)"
    echo "   Direct Access: http://localhost:8080"
else
    echo "   Web Application: http://localhost:8080"
fi
echo "   Database: localhost:5432"
echo "   Redis Cache: localhost:6379"
echo ""
echo "🔧 Management Commands:"
echo "   View logs: docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE logs -f"
echo "   Stop services: docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE down"
echo "   Restart: docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE restart"
echo "   Update: docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE pull && docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE up -d"
echo ""
echo "📊 Monitoring:"
echo "   System status: docker-compose -p $PROJECT_NAME -f $COMPOSE_FILE ps"
echo "   Resource usage: docker stats"
