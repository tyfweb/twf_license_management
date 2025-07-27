# Public Repository Setup Guide

This guide helps you set up the public `TechWayFit.Licensing.Core` repository.

## Repository Structure

```
TechWayFit.Licensing.Core/ (PUBLIC REPO)
├── src/
│   └── TechWayFit.Licensing.Core/
│       ├── Contracts/
│       ├── Models/
│       ├── Services/
│       ├── Helpers/
│       └── TechWayFit.Licensing.Core.csproj
├── samples/
│   ├── AspNetCoreWebApi/
│   ├── ConsoleApp/
│   └── BlazorApp/
├── docs/
│   ├── API.md
│   ├── INTEGRATION.md
│   └── SECURITY.md
├── tests/
│   └── TechWayFit.Licensing.Core.Tests/
├── .github/
│   ├── workflows/
│   │   ├── build.yml
│   │   └── publish.yml
│   └── ISSUE_TEMPLATE/
├── README.md
├── LICENSE
├── CHANGELOG.md
├── CONTRIBUTING.md
└── .gitignore
```

## Migration Steps

### 1. Copy Core Library Files

```bash
# Navigate to the new public repo
cd /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core

# Create src directory structure
mkdir -p src/TechWayFit.Licensing.Core

# Copy core files (run from old location)
cp -r Contracts/ /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/src/TechWayFit.Licensing.Core/
cp -r Models/ /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/src/TechWayFit.Licensing.Core/
cp -r Services/ /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/src/TechWayFit.Licensing.Core/
cp -r Helpers/ /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/src/TechWayFit.Licensing.Core/
cp TechWayFit.Licensing.Core.csproj /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/src/TechWayFit.Licensing.Core/
```

### 2. Update Project File for NuGet Publishing

The `.csproj` file should be updated for public NuGet distribution:

```xml
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
    <PackageIcon>icon.png</PackageIcon>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    
    <!-- Documentation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.4" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
    <None Include="..\..\icon.png" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

### 3. Create Documentation Structure

```bash
# Create documentation directories
mkdir -p docs samples tests .github/workflows

# Move documentation files
cp README.md /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/
cp SAMPLE_USAGE.md /Users/manasnayak/Projects/TechWayFit/PublicRepo/TechWayFit.Licensing.Core/docs/INTEGRATION.md
```

### 4. Create Sample Applications

Create sample projects that demonstrate integration:

#### ASP.NET Core Web API Sample
```bash
mkdir -p samples/AspNetCoreWebApi
cd samples/AspNetCoreWebApi
dotnet new webapi
dotnet add package TechWayFit.Licensing.Core
```

#### Console Application Sample
```bash
mkdir -p samples/ConsoleApp
cd samples/ConsoleApp
dotnet new console
dotnet add package TechWayFit.Licensing.Core
```

### 5. Files to EXCLUDE from Public Repo

❌ **Do NOT copy these files/folders:**
- `Repositories/` (contains private implementation details)
- Any customer-specific license files
- Private keys or sensitive configuration
- Internal business logic
- Customer management code
- Billing/payment integration code

✅ **Only include these core components:**
- `Contracts/ILicenseValidationService.cs`
- `Models/` (all model classes)
- `Services/LicenseValidationService.cs`
- `Helpers/` (utility functions)
- Public documentation and samples

## Security Checklist Before Publishing

- [ ] No private keys or secrets in code
- [ ] No customer-specific data or configuration
- [ ] No internal API endpoints or business logic
- [ ] No proprietary algorithms (beyond standard RSA)
- [ ] All dependencies are public NuGet packages
- [ ] Documentation doesn't reveal internal architecture
- [ ] License file examples use dummy/sample data only

## GitHub Repository Settings

### Repository Configuration
- ✅ **Public** repository
- ✅ Enable **Issues** for community support
- ✅ Enable **Discussions** for community Q&A
- ✅ Enable **Wikis** for extended documentation
- ✅ Disable **Projects** (not needed for library)

### Branch Protection
Set up branch protection for `main`:
- Require pull request reviews
- Require status checks to pass
- Require up-to-date branches
- Include administrators in restrictions

### Repository Secrets (for CI/CD)
- `NUGET_API_KEY`: For automated package publishing
- `GITHUB_TOKEN`: For automated releases

## Recommended Next Steps

1. **Create the repository structure** using the commands above
2. **Review all code** for any private/proprietary content
3. **Set up GitHub Actions** for automated building and testing
4. **Create comprehensive README** for the public audience
5. **Add MIT license** or your preferred open-source license
6. **Set up NuGet publishing pipeline**
7. **Create initial release** (v1.0.0)
8. **Update private repositories** to reference the public NuGet package

## Benefits of This Structure

🎯 **Clear Separation**: Public validation vs private management  
🔒 **Security**: No sensitive IP exposed  
📦 **Easy Distribution**: Standard NuGet package workflow  
🤝 **Community Trust**: Open-source validation builds confidence  
📚 **Better Documentation**: Public samples and integration guides  
🚀 **Faster Adoption**: Easier for customers to evaluate and integrate  
