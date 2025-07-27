# Contributing to TechWayFit.Licensing.Core

Thank you for your interest in contributing to TechWayFit.Licensing.Core! This document provides guidelines and information for contributors.

## 🤝 How to Contribute

### Reporting Issues

- **Security Issues**: Please email security@techway.fit instead of creating public issues
- **Bug Reports**: Use the bug report template in GitHub Issues
- **Feature Requests**: Use the feature request template in GitHub Issues

### Development Workflow

1. **Fork the repository**
2. **Create a feature branch** from `main`
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes** following our coding standards
4. **Add tests** for any new functionality
5. **Run the test suite** to ensure nothing is broken
   ```bash
   dotnet test
   ```
6. **Submit a pull request** with a clear description

### Pull Request Guidelines

- **Clear Title**: Use descriptive titles (e.g., "Add support for ECDSA signatures")
- **Description**: Explain what changes you made and why
- **Tests**: Include unit tests for new features
- **Documentation**: Update documentation for API changes
- **Breaking Changes**: Clearly mark any breaking changes

## 🏗️ Development Setup

### Prerequisites

- **.NET 8.0 SDK** or later
- **Git**
- **IDE**: Visual Studio, VS Code, or Rider

### Getting Started

```bash
# Clone your fork
git clone https://github.com/your-username/TechWayFit.Licensing.Core.git
cd TechWayFit.Licensing.Core

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Project Structure

```
src/
├── TechWayFit.Licensing.Core/
│   ├── Contracts/          # Interfaces
│   ├── Models/             # Data models and DTOs
│   ├── Services/           # Core validation services
│   └── Helpers/            # Utility classes
tests/
├── TechWayFit.Licensing.Core.Tests/
│   ├── Unit/               # Unit tests
│   ├── Integration/        # Integration tests
│   └── TestData/           # Test fixtures and data
samples/
├── AspNetCoreWebApi/       # Sample web API
├── ConsoleApp/             # Sample console app
└── BlazorApp/              # Sample Blazor app
```

## 📋 Coding Standards

### C# Guidelines

- **Follow Microsoft C# Conventions**: [Official Guidelines](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Use XML Documentation**: Document all public APIs
- **Async/Await**: Use async patterns consistently
- **Nullable Reference Types**: Enable and use nullable reference types
- **Code Analysis**: Fix all analyzer warnings

### Example Code Style

```csharp
/// <summary>
/// Validates a license using RSA signature verification.
/// </summary>
/// <param name="licenseJson">The signed license JSON.</param>
/// <param name="publicKey">The RSA public key for validation.</param>
/// <returns>A <see cref="LicenseValidationResult"/> containing validation results.</returns>
public async Task<LicenseValidationResult> ValidateFromJsonAsync(
    string licenseJson, 
    string publicKey)
{
    ArgumentException.ThrowIfNullOrEmpty(licenseJson);
    ArgumentException.ThrowIfNullOrEmpty(publicKey);
    
    try
    {
        // Implementation here
        return LicenseValidationResult.Success(license);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "License validation failed");
        return LicenseValidationResult.Failure(
            LicenseStatus.ValidationError,
            $"Validation failed: {ex.Message}");
    }
}
```

### Testing Guidelines

- **Unit Tests**: Test individual methods and classes
- **Integration Tests**: Test component interactions
- **Test Data**: Use realistic but anonymized test data
- **Mocking**: Use Moq for dependency mocking
- **Assertions**: Use FluentAssertions for readable tests

### Example Test

```csharp
[Fact]
public async Task ValidateFromJsonAsync_WithValidLicense_ReturnsSuccess()
{
    // Arrange
    var licenseJson = TestData.ValidLicenseJson;
    var publicKey = TestData.PublicKey;
    var validator = new LicenseValidationService(_logger.Object, _cache.Object);
    
    // Act
    var result = await validator.ValidateFromJsonAsync(licenseJson, publicKey);
    
    // Assert
    result.IsValid.Should().BeTrue();
    result.Status.Should().Be(LicenseStatus.Valid);
    result.License.Should().NotBeNull();
}
```

## 🔒 Security Considerations

### What's Acceptable

- **Cryptographic Validation**: RSA, ECDSA signature algorithms
- **Temporal Validation**: Date/time checking
- **License Parsing**: JSON deserialization and validation
- **Caching**: In-memory validation result caching
- **Logging**: Structured logging for debugging

### What's NOT Acceptable

- **Network Communication**: No external API calls
- **Telemetry**: No usage tracking or analytics
- **Business Logic**: No feature-specific business rules
- **License Generation**: No private key operations
- **Customer Data**: No customer-specific code or data

### Security Review Process

All contributions undergo security review to ensure:

1. **No Secrets**: No hardcoded keys, passwords, or sensitive data
2. **No External Dependencies**: No unnecessary NuGet packages
3. **Cryptographic Safety**: Proper use of cryptographic libraries
4. **Input Validation**: Proper handling of untrusted input
5. **Error Handling**: No information leakage in error messages

## 🧪 Testing Requirements

### Required Test Coverage

- **Unit Tests**: All public methods must have tests
- **Edge Cases**: Test boundary conditions and error scenarios
- **Performance Tests**: Validate performance characteristics
- **Security Tests**: Test with malformed/malicious input

### Test Categories

```csharp
[Trait("Category", "Unit")]
public class LicenseValidationServiceTests { }

[Trait("Category", "Integration")]
public class FileBasedValidationTests { }

[Trait("Category", "Performance")]
public class ValidationPerformanceTests { }

[Trait("Category", "Security")]
public class SecurityValidationTests { }
```

### Running Tests

```bash
# All tests
dotnet test

# Specific category
dotnet test --filter "Category=Unit"

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Performance tests
dotnet test --filter "Category=Performance" --logger:console;verbosity=detailed
```

## 📝 Documentation Requirements

### API Documentation

- **XML Comments**: All public APIs require XML documentation
- **Code Examples**: Include usage examples in XML comments
- **Parameter Validation**: Document parameter requirements
- **Exception Documentation**: Document all thrown exceptions

### README Updates

- Update README.md for new features
- Add integration examples for major features
- Update version compatibility information

### Sample Applications

- Update sample applications to demonstrate new features
- Ensure samples compile and run correctly
- Include comments explaining integration patterns

## 🚀 Release Process

### Version Numbering

We use [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Checklist

- [ ] All tests pass
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Version number bumped in .csproj
- [ ] Sample applications tested
- [ ] Security review completed
- [ ] Performance benchmarks run

## ❓ Questions?

- **General Questions**: Use GitHub Discussions
- **Technical Support**: Create a GitHub Issue
- **Private Inquiries**: Email opensource@techway.fit

## 📜 Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). Please read it before participating.

## 🙏 Recognition

Contributors will be recognized in:

- **CHANGELOG.md**: Listed in release notes
- **README.md**: Contributors section
- **GitHub**: Contributor graphs and statistics

Thank you for helping make TechWayFit.Licensing.Core better! 🎉
