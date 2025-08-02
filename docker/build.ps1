# TechWayFit Licensing Management - Docker Build Script (PowerShell)
# This script builds the Docker image for the licensing management system

param(
    [string]$Version = "latest",
    [switch]$Push = $false,
    [switch]$SecurityScan = $false
)

Write-Host "🐳 TechWayFit Licensing Docker Build Script" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green

# Configuration
$ImageName = "techway/licensing-management"
$BuildContext = "../"
$Dockerfile = "./Dockerfile"

Write-Host "Image Name: $ImageName" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Build Context: $BuildContext" -ForegroundColor Yellow
Write-Host "Dockerfile: $Dockerfile" -ForegroundColor Yellow
Write-Host ""

try {
    # Check if Docker is running
    Write-Host "🔍 Checking Docker status..." -ForegroundColor Blue
    docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not running. Please start Docker and try again."
    }

    # Build the Docker image
    Write-Host "🔨 Building Docker image..." -ForegroundColor Blue
    docker build `
        -t "${ImageName}:${Version}" `
        -t "${ImageName}:latest" `
        -f $Dockerfile `
        $BuildContext

    if ($LASTEXITCODE -ne 0) {
        throw "Docker build failed"
    }

    Write-Host ""
    Write-Host "✅ Docker image built successfully!" -ForegroundColor Green
    
    # Get image size
    $ImageSize = docker images "${ImageName}:${Version}" --format "{{.Size}}"
    Write-Host "   Image: ${ImageName}:${Version}" -ForegroundColor Green
    Write-Host "   Size: $ImageSize" -ForegroundColor Green

    # Show image details
    Write-Host ""
    Write-Host "📋 Image Details:" -ForegroundColor Blue
    docker images "${ImageName}:${Version}" --format "table {{.Repository}}\t{{.Tag}}\t{{.ID}}\t{{.CreatedAt}}\t{{.Size}}"

    # Optional: Run security scan
    if ($SecurityScan -and (Get-Command docker-scout -ErrorAction SilentlyContinue)) {
        Write-Host ""
        Write-Host "🔍 Running security scan..." -ForegroundColor Blue
        docker scout cves "${ImageName}:${Version}"
    }

    # Optional: Push to registry
    if ($Push) {
        Write-Host ""
        Write-Host "📤 Pushing image to registry..." -ForegroundColor Blue
        docker push "${ImageName}:${Version}"
        docker push "${ImageName}:latest"
        Write-Host "✅ Image pushed successfully!" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "🚀 Ready to deploy! Use the following commands:" -ForegroundColor Cyan
    Write-Host "   Development: docker-compose up -d" -ForegroundColor White
    Write-Host "   Production:  docker-compose -f docker-compose.prod.yml up -d" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Copy .env.template to .env and configure your environment" -ForegroundColor White
    Write-Host "   2. Run 'docker-compose up -d' to start the application" -ForegroundColor White
    Write-Host "   3. Access the application at http://localhost:8080" -ForegroundColor White

} catch {
    Write-Host ""
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
