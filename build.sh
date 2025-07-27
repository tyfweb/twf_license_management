#!/bin/bash

# TechWayFit Licensing Build Script
# This script builds all projects and creates NuGet packages

set -e

echo "🚀 TechWayFit Licensing Build Script"
echo "====================================="

# Configuration
CONFIGURATION=${1:-Release}
OUTPUT_DIR="./nupkgs"
VERSION_SUFFIX=${2:-""}

echo "Configuration: $CONFIGURATION"
echo "Output Directory: $OUTPUT_DIR"
if [ ! -z "$VERSION_SUFFIX" ]; then
    echo "Version Suffix: $VERSION_SUFFIX"
fi
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration $CONFIGURATION
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build solution
echo "🔨 Building solution..."
if [ ! -z "$VERSION_SUFFIX" ]; then
    dotnet build --configuration $CONFIGURATION --no-restore --version-suffix $VERSION_SUFFIX
else
    dotnet build --configuration $CONFIGURATION --no-restore
fi

# Create NuGet packages
echo "📦 Creating NuGet packages..."

# Pack Core library
echo "  📦 Packing TechWayFit.Licensing.Core..."
if [ ! -z "$VERSION_SUFFIX" ]; then
    dotnet pack ./TechWayFit.Licensing.Core/TechWayFit.Licensing.Core.csproj \
        --configuration $CONFIGURATION \
        --no-build \
        --output $OUTPUT_DIR \
        --version-suffix $VERSION_SUFFIX
else
    dotnet pack ./TechWayFit.Licensing.Core/TechWayFit.Licensing.Core.csproj \
        --configuration $CONFIGURATION \
        --no-build \
        --output $OUTPUT_DIR
fi

# Pack Validation library
echo "  📦 Packing TechWayFit.Licensing.Validation..."
if [ ! -z "$VERSION_SUFFIX" ]; then
    dotnet pack ./TechWayFit.Licensing.Validation/TechWayFit.Licensing.Validation.csproj \
        --configuration $CONFIGURATION \
        --no-build \
        --output $OUTPUT_DIR \
        --version-suffix $VERSION_SUFFIX
else
    dotnet pack ./TechWayFit.Licensing.Validation/TechWayFit.Licensing.Validation.csproj \
        --configuration $CONFIGURATION \
        --no-build \
        --output $OUTPUT_DIR
fi

# List created packages
echo ""
echo "✅ Build completed successfully!"
echo "📦 Created packages:"
ls -la $OUTPUT_DIR/*.nupkg

echo ""
echo "🚀 To publish packages to NuGet.org:"
echo "   dotnet nuget push $OUTPUT_DIR/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
echo ""
echo "🚀 To publish to a private feed:"
echo "   dotnet nuget push $OUTPUT_DIR/*.nupkg --api-key YOUR_API_KEY --source YOUR_PRIVATE_FEED_URL"
echo ""
echo "📋 Usage examples:"
echo "   ./build.sh                    # Build Release configuration"
echo "   ./build.sh Debug              # Build Debug configuration"
echo "   ./build.sh Release alpha      # Build with version suffix (e.g., 1.0.0-alpha)"
