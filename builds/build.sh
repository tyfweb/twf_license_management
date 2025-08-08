#!/bin/bash

# TechWayFit Licensing Build Script
# This script builds all projects and creates NuGet packages

set -e

echo "🚀 TechWayFit Licensing Build Script"
echo "====================================="

# Configuration
CONFIGURATION=${1:-Release}
OUTPUT_DIR="../nupkgs"
VERSION_SUFFIX=${2:-""}
SOLUTION_PATH="../source/TechWayFit.Licensing.sln"

echo "Configuration: $CONFIGURATION"
echo "Output Directory: $OUTPUT_DIR"
echo "Solution Path: $SOLUTION_PATH"
if [ ! -z "$VERSION_SUFFIX" ]; then
    echo "Version Suffix: $VERSION_SUFFIX"
fi
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean $SOLUTION_PATH --configuration $CONFIGURATION
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore $SOLUTION_PATH

# Build solution
echo "🔨 Building solution..."
if [ ! -z "$VERSION_SUFFIX" ]; then
    dotnet build $SOLUTION_PATH --configuration $CONFIGURATION --no-restore --version-suffix $VERSION_SUFFIX
else
    dotnet build $SOLUTION_PATH --configuration $CONFIGURATION --no-restore
fi

# Create NuGet packages
echo "📦 Creating NuGet packages..."

# Pack Core library
echo "  📦 Packing TechWayFit.Licensing.Management.Core..."
if [ ! -z "$VERSION_SUFFIX" ]; then
    dotnet pack $SOLUTION_PATH --configuration $CONFIGURATION --no-build --output $OUTPUT_DIR --version-suffix $VERSION_SUFFIX
else
    dotnet pack $SOLUTION_PATH --configuration $CONFIGURATION --no-build --output $OUTPUT_DIR
fi

# List created packages
echo ""
echo "✅ Build completed successfully!"
echo "📦 Created packages:"
if [ -d "$OUTPUT_DIR" ] && [ "$(ls -A $OUTPUT_DIR/*.nupkg 2>/dev/null)" ]; then
    ls -la $OUTPUT_DIR/*.nupkg
else
    echo "No packages found in $OUTPUT_DIR"
fi

echo ""
echo "🚀 To publish packages to NuGet.org:"
echo "   dotnet nuget push $OUTPUT_DIR/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
echo ""
echo "📋 Usage examples:"
echo "   ./build.sh                    # Build Release configuration"
echo "   ./build.sh Debug              # Build Debug configuration"
echo "   ./build.sh Release alpha      # Build with version suffix (e.g., 1.0.0-alpha)"
