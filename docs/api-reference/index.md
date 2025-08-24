---
title: API Reference
nav_order: 5
has_children: true
---

# API Reference
{: .no_toc }

Complete REST API documentation for the TechWayFit License Management System.

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Overview

The TechWayFit License Management System provides a comprehensive RESTful API that enables integration with external systems, automation of license management tasks, and development of custom applications.

## API Characteristics

### RESTful Design
- **Resource-based URLs**: `/api/v1/products/{id}`
- **HTTP Methods**: GET, POST, PUT, DELETE, PATCH
- **Status Codes**: Standard HTTP response codes
- **JSON Format**: All requests and responses use JSON

### Versioning
- **URL Versioning**: `/api/v1/`, `/api/v2/`
- **Backward Compatibility**: Previous versions supported
- **Deprecation Policy**: 12-month notice for breaking changes

### Rate Limiting
- **Default Limits**: 1000 requests per hour per API key
- **Burst Allowance**: 100 requests per minute
- **Rate Limit Headers**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`

---

## Authentication

### API Key Authentication

Most API endpoints require authentication using API keys:

```http
GET /api/v1/products
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json
```

### JWT Token Authentication

For user-specific operations, use JWT tokens:

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin@company.com",
  "password": "secure_password",
  "tenantId": "123e4567-e89b-12d3-a456-426614174000"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-01T00:00:00Z",
  "user": {
    "id": "user-123",
    "email": "admin@company.com",
    "roles": ["Administrator"]
  }
}
```

Use the token in subsequent requests:
```http
GET /api/v1/user/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Core Endpoints

### Products API

#### List Products
```http
GET /api/v1/products
```

Query Parameters:
- `page` (int): Page number (default: 1)
- `size` (int): Page size (default: 20, max: 100)
- `search` (string): Search term for product name/description
- `status` (string): Filter by status (active, inactive, archived)

Response:
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Enterprise Software Suite",
      "description": "Comprehensive business management software",
      "status": "active",
      "releaseDate": "2024-01-15T00:00:00Z",
      "supportEmail": "support@company.com",
      "supportPhone": "+1-555-0123",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-15T00:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 20,
    "total": 45,
    "pages": 3
  }
}
```

#### Get Product Details
```http
GET /api/v1/products/{id}
```

Response:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Enterprise Software Suite",
  "description": "Comprehensive business management software",
  "status": "active",
  "releaseDate": "2024-01-15T00:00:00Z",
  "decommissionDate": null,
  "supportEmail": "support@company.com",
  "supportPhone": "+1-555-0123",
  "metadata": {
    "category": "Business Software",
    "tags": ["enterprise", "business", "management"]
  },
  "tiers": [
    {
      "id": "tier-123",
      "name": "Professional",
      "description": "Full feature access for professional use",
      "maxUsers": 100,
      "maxDevices": 50
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-15T00:00:00Z"
}
```

#### Create Product
```http
POST /api/v1/products
Content-Type: application/json

{
  "name": "New Product",
  "description": "Product description",
  "releaseDate": "2024-12-01T00:00:00Z",
  "supportEmail": "support@company.com",
  "supportPhone": "+1-555-0123",
  "metadata": {
    "category": "Software",
    "tags": ["new", "product"]
  }
}
```

### Licenses API

#### Create License
```http
POST /api/v1/licenses
Content-Type: application/json

{
  "productId": "123e4567-e89b-12d3-a456-426614174000",
  "consumerId": "456e7890-e89b-12d3-a456-426614174000",
  "licenseType": "subscription",
  "licenseModel": "user_based",
  "validFrom": "2024-01-01T00:00:00Z",
  "validTo": "2024-12-31T23:59:59Z",
  "maxAllowedUsers": 50,
  "productTierId": "tier-123",
  "features": [
    "advanced_reporting",
    "api_access",
    "priority_support"
  ]
}
```

Response:
```json
{
  "id": "789e0123-e89b-12d3-a456-426614174000",
  "licenseKey": "LWKY-ABCD-1234-EFGH-5678",
  "licenseCode": "ENT-2024-PRO-001",
  "status": "active",
  "licenseFile": "base64_encoded_license_file",
  "downloadUrl": "https://api.licensing.com/v1/licenses/789e0123/download",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

#### Validate License
```http
POST /api/v1/licenses/validate
Content-Type: application/json

{
  "licenseKey": "LWKY-ABCD-1234-EFGH-5678",
  "productVersion": "2.1.0",
  "clientIdentifier": "client-machine-id",
  "features": ["advanced_reporting", "api_access"]
}
```

Response:
```json
{
  "isValid": true,
  "status": "active",
  "expiresAt": "2024-12-31T23:59:59Z",
  "allowedFeatures": [
    "advanced_reporting",
    "api_access",
    "priority_support"
  ],
  "restrictions": {
    "maxUsers": 50,
    "maxDevices": 25
  },
  "productInfo": {
    "name": "Enterprise Software Suite",
    "version": "2.1.0"
  },
  "validationDetails": {
    "signatureValid": true,
    "notExpired": true,
    "notRevoked": true,
    "featureMatch": true
  }
}
```

### Consumers API

#### List Consumers
```http
GET /api/v1/consumers
```

Query Parameters:
- `page`, `size`: Pagination
- `search`: Search by company name or contact
- `status`: Filter by status

#### Create Consumer
```http
POST /api/v1/consumers
Content-Type: application/json

{
  "companyName": "Acme Corporation",
  "accountCode": "ACME-001",
  "primaryContactName": "John Doe",
  "primaryContactEmail": "john.doe@acme.com",
  "primaryContactPhone": "+1-555-0100",
  "addressStreet": "123 Business Ave",
  "addressCity": "Business City",
  "addressState": "BC",
  "addressPostalCode": "12345",
  "addressCountry": "United States",
  "notes": "Enterprise customer with custom requirements"
}
```

---

## Advanced Operations

### Bulk Operations

#### Bulk License Creation
```http
POST /api/v1/licenses/bulk
Content-Type: application/json

{
  "productId": "123e4567-e89b-12d3-a456-426614174000",
  "licenses": [
    {
      "consumerId": "consumer-1",
      "validFrom": "2024-01-01T00:00:00Z",
      "validTo": "2024-12-31T23:59:59Z",
      "maxAllowedUsers": 10
    },
    {
      "consumerId": "consumer-2",
      "validFrom": "2024-01-01T00:00:00Z",
      "validTo": "2024-12-31T23:59:59Z",
      "maxAllowedUsers": 25
    }
  ]
}
```

#### Bulk License Validation
```http
POST /api/v1/licenses/validate/bulk
Content-Type: application/json

{
  "licenses": [
    {
      "licenseKey": "LWKY-ABCD-1234-EFGH-5678",
      "productVersion": "2.1.0"
    },
    {
      "licenseKey": "LWKY-EFGH-5678-IJKL-9012",
      "productVersion": "2.1.0"
    }
  ]
}
```

### Reporting API

#### License Usage Report
```http
GET /api/v1/reports/license-usage
```

Query Parameters:
- `startDate`, `endDate`: Date range
- `productId`: Filter by product
- `consumerId`: Filter by consumer
- `format`: Response format (json, csv, excel)

### Webhooks

#### Register Webhook
```http
POST /api/v1/webhooks
Content-Type: application/json

{
  "url": "https://your-app.com/webhooks/licensing",
  "events": [
    "license.created",
    "license.validated",
    "license.expired",
    "license.revoked"
  ],
  "secret": "webhook_secret_for_verification"
}
```

#### Webhook Payload Example
```json
{
  "id": "webhook-event-123",
  "type": "license.validated",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": {
    "licenseId": "789e0123-e89b-12d3-a456-426614174000",
    "licenseKey": "LWKY-ABCD-1234-EFGH-5678",
    "productId": "123e4567-e89b-12d3-a456-426614174000",
    "consumerId": "456e7890-e89b-12d3-a456-426614174000",
    "validationResult": {
      "isValid": true,
      "clientIdentifier": "client-machine-id"
    }
  }
}
```

---

## Error Handling

### Standard Error Response
```json
{
  "error": {
    "code": "INVALID_LICENSE_KEY",
    "message": "The provided license key is invalid or has expired",
    "details": {
      "licenseKey": "LWKY-INVALID-KEY",
      "validationErrors": [
        "License signature verification failed",
        "License has expired"
      ]
    },
    "timestamp": "2024-01-15T10:30:00Z",
    "requestId": "req-123456789"
  }
}
```

### Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `UNAUTHORIZED` | 401 | Invalid or missing authentication |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `NOT_FOUND` | 404 | Resource does not exist |
| `VALIDATION_ERROR` | 400 | Invalid request data |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Server error |
| `INVALID_LICENSE_KEY` | 400 | License validation failed |
| `EXPIRED_LICENSE` | 400 | License has expired |
| `REVOKED_LICENSE` | 400 | License has been revoked |

---

## Rate Limiting

### Headers
Every API response includes rate limiting headers:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
X-RateLimit-Retry-After: 3600
```

### Rate Limit Response
When rate limit is exceeded:

```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1640995200
X-RateLimit-Retry-After: 3600

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "API rate limit exceeded. Please retry after 3600 seconds.",
    "retryAfter": 3600
  }
}
```

---

## SDK and Libraries

### Official SDKs

#### .NET SDK
```csharp
// Install-Package TechWayFit.Licensing.Client
using TechWayFit.Licensing.Client;

var client = new LicensingClient("your-api-key", "https://api.licensing.com");

// Validate a license
var result = await client.ValidateLicenseAsync("LWKY-ABCD-1234-EFGH-5678");
if (result.IsValid)
{
    Console.WriteLine($"License valid until: {result.ExpiresAt}");
}
```

#### JavaScript SDK
```javascript
// npm install @techwayfit/licensing-client
import { LicensingClient } from '@techwayfit/licensing-client';

const client = new LicensingClient({
  apiKey: 'your-api-key',
  baseUrl: 'https://api.licensing.com'
});

// Validate a license
const result = await client.validateLicense('LWKY-ABCD-1234-EFGH-5678');
console.log('License valid:', result.isValid);
```

#### Python SDK
```python
# pip install techwayfit-licensing
from techwayfit_licensing import LicensingClient

client = LicensingClient(api_key='your-api-key', base_url='https://api.licensing.com')

# Validate a license
result = client.validate_license('LWKY-ABCD-1234-EFGH-5678')
print(f"License valid: {result.is_valid}")
```

---

## Testing

### Test Environment
Use the sandbox environment for testing:
- **Base URL**: `https://sandbox-api.licensing.com`
- **Test API Keys**: Provided in developer portal
- **Test Data**: Pre-populated products and licenses

### Example Test License
```
License Key: TEST-1234-5678-9012-ABCD
Product: Test Product Suite
Valid Until: 2030-12-31
Features: All features enabled
```

---

## API Changelog

### Version 1.2 (Current)
- Added bulk operations for licenses
- Enhanced webhook support
- New reporting endpoints
- Improved error messages

### Version 1.1
- Added consumer management endpoints
- License validation improvements
- Rate limiting implementation

### Version 1.0
- Initial API release
- Basic CRUD operations
- Authentication and authorization

---

## Support

### API Documentation
- **Interactive Docs**: [Swagger UI](https://api.licensing.com/swagger)
- **OpenAPI Spec**: [Download OpenAPI 3.0 spec](https://api.licensing.com/openapi.json)
- **Postman Collection**: [Import collection](https://api.licensing.com/postman.json)

### Developer Resources
- **SDK Downloads**: Available in developer portal
- **Code Examples**: GitHub repository with samples
- **Integration Guides**: Step-by-step integration tutorials
- **Best Practices**: API usage recommendations

### Getting Help
- **Developer Portal**: [https://developers.licensing.com](https://developers.licensing.com)
- **API Status**: [https://status.licensing.com](https://status.licensing.com)
- **Support Email**: [api-support@techwayfit.com](mailto:api-support@techwayfit.com)
- **Community Forum**: [https://community.licensing.com](https://community.licensing.com)
