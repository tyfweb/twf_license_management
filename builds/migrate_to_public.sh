#!/bin/bash

# Migration script for TechWayFit.Licensing.Core Public Repository
# This script copies the necessary files from the private repo to the public repo

set -e

# Configuration
SOURCE_DIR="/Users/manasnayak/Projects/TechWayFit/APIGW/two_license_management"
PUBLIC_REPO="/Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core"
CORE_DIR="$SOURCE_DIR/TechWayFit.Licensing.Core"

echo "🚀 Starting migration to public repository..."
echo "Source: $SOURCE_DIR"
echo "Target: $PUBLIC_REPO"

# Create directory structure
echo "📁 Creating directory structure..."
mkdir -p "$PUBLIC_REPO/src/TechWayFit.Licensing.Core"
mkdir -p "$PUBLIC_REPO/tests"
mkdir -p "$PUBLIC_REPO/samples/AspNetCoreWebApi"
mkdir -p "$PUBLIC_REPO/samples/ConsoleApp"
mkdir -p "$PUBLIC_REPO/docs"
mkdir -p "$PUBLIC_REPO/.github/workflows"

# Copy core library files
echo "📦 Copying core library files..."
cp -r "$CORE_DIR/Contracts" "$PUBLIC_REPO/src/TechWayFit.Licensing.Core/"
cp -r "$CORE_DIR/Models" "$PUBLIC_REPO/src/TechWayFit.Licensing.Core/"
cp -r "$CORE_DIR/Services" "$PUBLIC_REPO/src/TechWayFit.Licensing.Core/"
cp -r "$CORE_DIR/Helpers" "$PUBLIC_REPO/src/TechWayFit.Licensing.Core/"

# Copy and update project file
echo "📄 Updating project file..."
cat > "$PUBLIC_REPO/src/TechWayFit.Licensing.Core/TechWayFit.Licensing.Core.csproj" << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- NuGet Package Properties -->
    <PackageId>TechWayFit.Licensing.Core</PackageId>
    <Version>1.0.0</Version>
    <Authors>TechWayFit</Authors>
    <Company>TechWayFit</Company>
    <Product>TechWayFit Licensing Core</Product>
    <Description>A lean, cryptographic license validation library for .NET applications. Provides RSA signature verification, temporal validation, and flexible license management without business logic dependencies.</Description>
    <PackageTags>licensing;validation;cryptography;rsa;dotnet;nuget</PackageTags>
    <PackageProjectUrl>https://github.com/tyfweb/TechWayFit.Licensing.Core</PackageProjectUrl>
    <RepositoryUrl>https://github.com/tyfweb/TechWayFit.Licensing.Core</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    
    <!-- Documentation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.4" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
EOF

# Copy documentation files
echo "📚 Copying documentation..."
cp "$SOURCE_DIR/PUBLIC_README.md" "$PUBLIC_REPO/README.md"
cp "$CORE_DIR/SAMPLE_USAGE.md" "$PUBLIC_REPO/docs/INTEGRATION.md"
cp "$SOURCE_DIR/LICENSE" "$PUBLIC_REPO/LICENSE"
cp "$SOURCE_DIR/CONTRIBUTING.md" "$PUBLIC_REPO/CONTRIBUTING.md"
cp "$SOURCE_DIR/PUBLIC_GITIGNORE" "$PUBLIC_REPO/.gitignore"

# Copy GitHub Actions workflow
echo "⚙️ Setting up CI/CD..."
cp "$SOURCE_DIR/GITHUB_WORKFLOW.yml" "$PUBLIC_REPO/.github/workflows/build.yml"

# Create additional documentation files
echo "📖 Creating additional documentation..."

# API.md
cat > "$PUBLIC_REPO/docs/API.md" << 'EOF'
# API Reference

## ILicenseValidationService

### Methods

#### ValidateFromJsonAsync
Validates a license from JSON string and public key.

#### ValidateFromFileAsync  
Validates a license from file paths.

#### ValidateAsync
Validates a license object directly.

## Models

### LicenseValidationResult
Contains the result of license validation.

### License
Represents a license with all its properties.

### SignedLicense
Represents a cryptographically signed license.

For detailed examples, see [Integration Guide](INTEGRATION.md).
EOF

# SECURITY.md
cat > "$PUBLIC_REPO/docs/SECURITY.md" << 'EOF'
# Security Best Practices

## File-Based License Storage

### Production Deployment
- Store license files outside application directory
- Use proper file permissions (600 for license, 644 for public key)
- Never commit actual license files to source control

### Container Deployment
- Use secrets management (Docker secrets, Kubernetes secrets)
- Mount license files as read-only volumes
- Set appropriate file ownership and permissions

For complete security guidance, see [Integration Guide](INTEGRATION.md).
EOF

# Create sample ASP.NET Core project
echo "🌐 Creating ASP.NET Core sample..."
cd "$PUBLIC_REPO/samples/AspNetCoreWebApi"
cat > "Program.cs" << 'EOF'
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ILicenseValidationService, LicenseValidationService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
EOF

cat > "SampleWebApi.csproj" << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="TechWayFit.Licensing.Core" Version="1.0.0" />
  </ItemGroup>

</Project>
EOF

# Create sample Console project  
echo "🖥️ Creating Console sample..."
cd "$PUBLIC_REPO/samples/ConsoleApp"
cat > "Program.cs" << 'EOF'
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;

var services = new ServiceCollection()
    .AddSingleton<ILicenseValidationService, LicenseValidationService>()
    .AddMemoryCache()
    .AddLogging(builder => builder.AddConsole())
    .BuildServiceProvider();

var validator = services.GetRequiredService<ILicenseValidationService>();

// Sample validation
var licenseJson = """{"licenseData":"sample","signature":"sample"}""";
var publicKey = "-----BEGIN PUBLIC KEY-----\nsample\n-----END PUBLIC KEY-----";

try
{
    var result = await validator.ValidateFromJsonAsync(licenseJson, publicKey);
    Console.WriteLine($"License Valid: {result.IsValid}");
    Console.WriteLine($"Status: {result.Status}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
EOF

cat > "ConsoleApp.csproj" << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="TechWayFit.Licensing.Core" Version="1.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
  </ItemGroup>

</Project>
EOF

# Create solution file
echo "📋 Creating solution file..."
cd "$PUBLIC_REPO"
cat > "TechWayFit.Licensing.Core.sln" << 'EOF'
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TechWayFit.Licensing.Core", "src\TechWayFit.Licensing.Core\TechWayFit.Licensing.Core.csproj", "{A1B2C3D4-E5F6-7890-1234-567890ABCDEF}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SampleWebApi", "samples\AspNetCoreWebApi\SampleWebApi.csproj", "{B2C3D4E5-F678-9012-3456-7890ABCDEF12}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ConsoleApp", "samples\ConsoleApp\ConsoleApp.csproj", "{C3D4E5F6-7890-1234-5678-90ABCDEF1234}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{A1B2C3D4-E5F6-7890-1234-567890ABCDEF}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1B2C3D4-E5F6-7890-1234-567890ABCDEF}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A1B2C3D4-E5F6-7890-1234-567890ABCDEF}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A1B2C3D4-E5F6-7890-1234-567890ABCDEF}.Release|Any CPU.Build.0 = Release|Any CPU
		{B2C3D4E5-F678-9012-3456-7890ABCDEF12}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{B2C3D4E5-F678-9012-3456-7890ABCDEF12}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{B2C3D4E5-F678-9012-3456-7890ABCDEF12}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{B2C3D4E5-F678-9012-3456-7890ABCDEF12}.Release|Any CPU.Build.0 = Release|Any CPU
		{C3D4E5F6-7890-1234-5678-90ABCDEF1234}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C3D4E5F6-7890-1234-5678-90ABCDEF1234}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C3D4E5F6-7890-1234-5678-90ABCDEF1234}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C3D4E5F6-7890-1234-5678-90ABCDEF1234}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
EOF

# Create CHANGELOG.md
echo "📝 Creating changelog..."
cat > "$PUBLIC_REPO/CHANGELOG.md" << 'EOF'
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-27

### Added
- Initial public release
- RSA signature validation
- Temporal license validation
- File-based and JSON-based license loading
- Memory caching support
- Comprehensive logging
- Sample applications
- Complete documentation

### Security
- Open-source validation for transparency
- No telemetry or external communication
- Cryptographic validation only
EOF

echo "✅ Migration completed successfully!"
echo ""
echo "📋 Next steps:"
echo "1. Navigate to: $PUBLIC_REPO"
echo "2. Review all copied files"
echo "3. Test the build: dotnet build"
echo "4. Initialize git: git init && git add . && git commit -m 'Initial commit'"
echo "5. Add GitHub remote and push"
echo "6. Set up GitHub repository settings"
echo "7. Create initial release"
echo ""
echo "🔍 Files to review:"
echo "- README.md (update any TechWayFit-specific references)"
echo "- All source files (ensure no private/proprietary content)"
echo "- Sample applications (test functionality)"
echo "- Documentation (verify accuracy)"
echo ""
echo "Happy coding! 🚀"
