#!/bin/bash

# TechWayFit Licensing Management - Docker Build Script
# This script builds the Docker image for the licensing management system

set -e

echo "🐳 TechWayFit Licensing Docker Build Script"
echo "============================================"

# Configuration
IMAGE_NAME="techway/licensing-management"
VERSION=${1:-"latest"}
BUILD_CONTEXT="../"
DOCKERFILE="./Dockerfile"

echo "Image Name: $IMAGE_NAME"
echo "Version: $VERSION"
echo "Build Context: $BUILD_CONTEXT"
echo "Dockerfile: $DOCKERFILE"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Build the Docker image
echo "🔨 Building Docker image..."
docker build \
    -t "$IMAGE_NAME:$VERSION" \
    -t "$IMAGE_NAME:latest" \
    -f "$DOCKERFILE" \
    "$BUILD_CONTEXT"

echo ""
echo "✅ Docker image built successfully!"
echo "   Image: $IMAGE_NAME:$VERSION"
echo "   Size: $(docker images $IMAGE_NAME:$VERSION --format "{{.Size}}")"

# Show image details
echo ""
echo "📋 Image Details:"
docker images "$IMAGE_NAME:$VERSION" --format "table {{.Repository}}\t{{.Tag}}\t{{.ID}}\t{{.CreatedAt}}\t{{.Size}}"

# Optional: Run security scan
if command -v docker-scout &> /dev/null; then
    echo ""
    echo "🔍 Running security scan..."
    docker scout cves "$IMAGE_NAME:$VERSION" || echo "⚠️  Security scan completed with findings"
fi

echo ""
echo "🚀 Ready to deploy! Use the following commands:"
echo "   Development: docker-compose up -d"
echo "   Production:  docker-compose -f docker-compose.prod.yml up -d"
echo ""
echo "💡 Next steps:"
echo "   1. Copy .env.template to .env and configure your environment"
echo "   2. Run 'docker-compose up -d' to start the application"
echo "   3. Access the application at http://localhost:8080"
