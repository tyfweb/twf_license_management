# TechWayFit.Licensing.Core

[![NuGet Version](https://img.shields.io/nuget/v/TechWayFit.Licensing.Core)](https://www.nuget.org/packages/TechWayFit.Licensing.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TechWayFit.Licensing.Core)](https://www.nuget.org/packages/TechWayFit.Licensing.Core/)
[![Build Status](https://github.com/tyfweb/TechWayFit.Licensing.Core/workflows/Build/badge.svg)](https://github.com/tyfweb/TechWayFit.Licensing.Core/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A **lean, cryptographic license validation library** for .NET applications. Provides RSA signature verification, temporal validation, and flexible license management without business logic dependencies.

## 🚀 Quick Start

### Installation

```bash
dotnet add package TechWayFit.Licensing.Core
```

### Basic Usage

```csharp
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;

// Register the service
services.AddSingleton<ILicenseValidationService, LicenseValidationService>();

// Validate a license
var validator = serviceProvider.GetService<ILicenseValidationService>();
var result = await validator.ValidateFromJsonAsync(licenseJson, publicKey);

if (result.IsValid)
{
    Console.WriteLine($"License valid for: {result.License.LicensedTo}");
    Console.WriteLine($"Expires: {result.License.ExpiresOn}");
}
```

## ✨ Features

- **🔐 Cryptographic Validation**: RSA signature verification with configurable algorithms
- **⏰ Temporal Validation**: Expiration date and activation date checking
- **📁 Multiple Input Sources**: JSON strings, files, or custom license objects
- **🏗️ Lean Architecture**: No business logic - pure validation library
- **🔄 Caching Support**: Built-in memory caching for performance
- **📝 Comprehensive Logging**: Structured logging with Microsoft.Extensions.Logging
- **🎯 .NET 8+ Ready**: Built for modern .NET applications
- **🔒 Security-First**: Open-source validation builds trust and transparency

## 📋 Supported License Features

- **Product Information**: Product ID, version, and type categorization
- **Customer Details**: Licensed organization and contact information  
- **Feature Licensing**: Named features with individual validity periods
- **Usage Limits**: Configurable limits (users, requests, endpoints, etc.)
- **Tier Management**: Basic, Professional, Enterprise tier support
- **Grace Periods**: Configurable grace period for expired licenses
- **Multi-Environment**: Development, staging, and production license support

## 🔧 Integration Examples

### ASP.NET Core Web API

```csharp
// Program.cs
builder.Services.AddSingleton<ILicenseValidationService, LicenseValidationService>();
builder.Services.AddScoped<IAppLicenseService, AppLicenseService>();

// appsettings.json
{
  "License": {
    "LicenseFilePath": "license/app.license",
    "PublicKeyFilePath": "license/public.key"
  }
}
```

### File-Based License Storage

```csharp
// Validate from files (recommended for production)
var result = await validator.ValidateFromFileAsync(
    "license/app.license", 
    "license/public.key");
```

### Feature-Based Access Control

```csharp
[RequireFeature("Advanced Analytics")]
public async Task<IActionResult> GetAnalytics()
{
    // Feature-specific endpoint
    return Ok(analyticsData);
}
```

## 📖 Documentation

- **[Integration Guide](docs/INTEGRATION.md)** - Complete implementation examples
- **[API Reference](docs/API.md)** - Detailed API documentation  
- **[Security Best Practices](docs/SECURITY.md)** - Production deployment guidance
- **[Sample Applications](samples/)** - Working examples for different scenarios

## 🔒 Security & Trust

This library is **open-source** to provide complete transparency in license validation:

✅ **No Hidden Behavior**: You can audit every line of validation code  
✅ **No Telemetry**: Zero data collection or external communication  
✅ **No Backdoors**: Pure cryptographic validation only  
✅ **Industry Standards**: Uses standard RSA-PSS and RSA-PKCS1 algorithms  

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                Your Application                      │
├─────────────────────────────────────────────────────┤
│            Business Logic Layer                     │
│  ┌─────────────────┐  ┌─────────────────────────┐   │
│  │ Feature Control │  │   Usage Enforcement     │   │
│  │ Access Control  │  │   Billing Integration   │   │
│  └─────────────────┘  └─────────────────────────┘   │
├─────────────────────────────────────────────────────┤
│         TechWayFit.Licensing.Core (This Library)   │
│  ┌─────────────────┐  ┌─────────────────────────┐   │
│  │ Cryptographic   │  │    Temporal             │   │
│  │ Validation      │  │    Validation           │   │
│  └─────────────────┘  └─────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

The core library handles **validation only** - your application implements business logic, feature control, and user experience.

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
git clone https://github.com/tyfweb/TechWayFit.Licensing.Core.git
cd TechWayFit.Licensing.Core
dotnet restore
dotnet build
dotnet test
```

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **📝 Documentation**: Check our [comprehensive guides](docs/)
- **🐛 Issues**: Report bugs via [GitHub Issues](https://github.com/tyfweb/TechWayFit.Licensing.Core/issues)
- **💬 Discussions**: Ask questions in [GitHub Discussions](https://github.com/tyfweb/TechWayFit.Licensing.Core/discussions)
- **📧 Enterprise Support**: Contact us for commercial licensing and support

## 🌟 Why Choose TechWayFit.Licensing.Core?

### vs. Competitors
- **🔍 Transparent**: Open-source validation vs. black-box solutions
- **🏃 Lean**: No bloated frameworks or unnecessary dependencies  
- **🎯 Focused**: Pure validation library, not a complete licensing platform
- **💰 Cost-Effective**: Use with any license generation system
- **🔧 Flexible**: Integrate with existing authentication and billing systems

### Industry Trust
Used by applications requiring:
- **🏢 Enterprise Security**: Banks, healthcare, government contractors
- **🔒 Audit Compliance**: SOX, HIPAA, ISO 27001 certified environments  
- **🌐 SaaS Platforms**: Multi-tenant applications with complex licensing
- **📱 Desktop Applications**: Offline-capable license validation

---

**Made with ❤️ by TechWayFit** | [Website](https://techway.fit) | [Blog](https://blog.techway.fit)
