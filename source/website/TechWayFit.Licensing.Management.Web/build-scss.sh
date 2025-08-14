#!/bin/bash

# SCSS Build Script for TechWayFit License Management
# This script compiles SCSS files and handles the build process

set -e  # Exit on any error

echo "🎨 TechWayFit License Management - SCSS Build Script"
echo "=================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if we're in the right directory
if [ ! -f "package.json" ]; then
    echo -e "${RED}❌ Error: package.json not found. Please run this script from the web project directory.${NC}"
    exit 1
fi

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}📦 Installing dependencies...${NC}"
    npm install
fi

# Function to display help
show_help() {
    echo -e "${BLUE}Usage: ./build-scss.sh [OPTION]${NC}"
    echo ""
    echo "Options:"
    echo "  watch, w     Watch for changes and compile automatically"
    echo "  dev, d       Compile for development (with source maps)"
    echo "  prod, p      Compile for production (compressed)"
    echo "  clean, c     Clean compiled CSS files"
    echo "  help, h      Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./build-scss.sh watch    # Start watching for changes"
    echo "  ./build-scss.sh prod     # Build for production"
}

# Function to clean compiled files
clean_files() {
    echo -e "${YELLOW}🧹 Cleaning compiled CSS files...${NC}"
    rm -f wwwroot/css/compiled.css
    rm -f wwwroot/css/compiled.css.map
    rm -f wwwroot/css/compiled.min.css
    echo -e "${GREEN}✅ Cleaned successfully!${NC}"
}

# Function to check if SCSS files exist
check_scss_files() {
    if [ ! -f "wwwroot/scss/main.scss" ]; then
        echo -e "${RED}❌ Error: SCSS files not found. Please ensure the SCSS architecture is set up.${NC}"
        exit 1
    fi
}

# Function to ensure output directory exists
ensure_output_dir() {
    mkdir -p wwwroot/css
}

# Main script logic
case "${1:-}" in
    watch|w)
        echo -e "${BLUE}👀 Starting SCSS watch mode...${NC}"
        echo -e "${YELLOW}Press Ctrl+C to stop watching${NC}"
        check_scss_files
        ensure_output_dir
        npm run scss:watch
        ;;
    dev|d)
        echo -e "${BLUE}🔧 Compiling SCSS for development...${NC}"
        check_scss_files
        ensure_output_dir
        npm run scss:dev
        echo -e "${GREEN}✅ Development build completed!${NC}"
        echo -e "${BLUE}📁 Output: wwwroot/css/compiled.css${NC}"
        ;;
    prod|p)
        echo -e "${BLUE}🚀 Compiling SCSS for production...${NC}"
        check_scss_files
        ensure_output_dir
        npm run scss:prod
        echo -e "${GREEN}✅ Production build completed!${NC}"
        echo -e "${BLUE}📁 Output: wwwroot/css/compiled.css (expanded)${NC}"
        echo -e "${BLUE}📁 Output: wwwroot/css/compiled.min.css (minified)${NC}"
        ;;
    clean|c)
        clean_files
        ;;
    help|h)
        show_help
        ;;
    "")
        echo -e "${YELLOW}⚡ Quick build for development...${NC}"
        check_scss_files
        ensure_output_dir
        npm run scss:dev
        echo -e "${GREEN}✅ Build completed!${NC}"
        echo -e "${BLUE}📁 Output: wwwroot/css/compiled.css${NC}"
        echo ""
        echo -e "${YELLOW}💡 Tip: Use './build-scss.sh help' to see all available options${NC}"
        ;;
    *)
        echo -e "${RED}❌ Unknown option: $1${NC}"
        echo ""
        show_help
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}🎉 SCSS build process completed successfully!${NC}"
