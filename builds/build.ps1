# TechWayFit Licensing Build Script (PowerShell)
# This script builds all projects and creates NuGet packages

param(
    [string]$Configuration = "Release",
    [string]$VersionSuffix = ""
)

Write-Host "🚀 TechWayFit Licensing Build Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Configuration
$OutputDir = "../nupkgs"
$SolutionPath = "../source/TechWayFit.Licensing.sln"

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output Directory: $OutputDir" -ForegroundColor Yellow  
Write-Host "Solution Path: $SolutionPath" -ForegroundColor Yellow
if ($VersionSuffix) {
    Write-Host "Version Suffix: $VersionSuffix" -ForegroundColor Yellow
}
Write-Host ""

try {
    # Clean previous builds
    Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Blue
    dotnet clean $SolutionPath --configuration $Configuration
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Blue
    dotnet restore $SolutionPath

    # Build solution
    Write-Host "🔨 Building solution..." -ForegroundColor Blue
    if ($VersionSuffix) {
        dotnet build $SolutionPath --configuration $Configuration --no-restore --version-suffix $VersionSuffix
    } else {
        dotnet build $SolutionPath --configuration $Configuration --no-restore
    }

    # Create NuGet packages
    Write-Host "📦 Creating NuGet packages..." -ForegroundColor Blue

    # Pack Core library
    Write-Host "  📦 Packing TechWayFit.Licensing.Core..." -ForegroundColor Cyan
    if ($VersionSuffix) {
        dotnet pack ../source/TechWayFit.Licensing.Core/TechWayFit.Licensing.Core.csproj `
            --configuration $Configuration `
            --no-build `
            --output $OutputDir `
            --version-suffix $VersionSuffix
    } else {
        dotnet pack ./TechWayFit.Licensing.Core/TechWayFit.Licensing.Core.csproj `
            --configuration $Configuration `
            --no-build `
            --output $OutputDir
    }

    # Pack Validation library
    Write-Host "  📦 Packing TechWayFit.Licensing.Validation..." -ForegroundColor Cyan
    if ($VersionSuffix) {
        dotnet pack ./TechWayFit.Licensing.Validation/TechWayFit.Licensing.Validation.csproj `
            --configuration $Configuration `
            --no-build `
            --output $OutputDir `
            --version-suffix $VersionSuffix
    } else {
        dotnet pack ./TechWayFit.Licensing.Validation/TechWayFit.Licensing.Validation.csproj `
            --configuration $Configuration `
            --no-build `
            --output $OutputDir
    }

    # List created packages
    Write-Host ""
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    Write-Host "📦 Created packages:" -ForegroundColor Green
    Get-ChildItem $OutputDir -Filter "*.nupkg" | ForEach-Object { Write-Host "   $($_.Name)" -ForegroundColor White }

    Write-Host ""
    Write-Host "🚀 To publish packages to NuGet.org:" -ForegroundColor Yellow
    Write-Host "   dotnet nuget push $OutputDir/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 To publish to a private feed:" -ForegroundColor Yellow
    Write-Host "   dotnet nuget push $OutputDir/*.nupkg --api-key YOUR_API_KEY --source YOUR_PRIVATE_FEED_URL" -ForegroundColor White
    Write-Host ""
    Write-Host "📋 Usage examples:" -ForegroundColor Yellow
    Write-Host "   .\build.ps1                           # Build Release configuration" -ForegroundColor White
    Write-Host "   .\build.ps1 -Configuration Debug      # Build Debug configuration" -ForegroundColor White
    Write-Host "   .\build.ps1 -VersionSuffix alpha       # Build with version suffix (e.g., 1.0.0-alpha)" -ForegroundColor White

} catch {
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
