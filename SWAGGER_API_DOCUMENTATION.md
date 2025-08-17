# TechWayFit Licensing Management API - Swagger Documentation

## Overview

The TechWayFit Licensing Management API provides comprehensive REST endpoints for managing enterprise products, features, and pricing tiers. The API includes full CRUD operations with detailed documentation via Swagger/OpenAPI.

## Swagger UI Access

When running in **Development** environment, Swagger UI is available at:
- **URL**: `http://localhost:5001/api/docs`
- **Swagger JSON**: `http://localhost:5001/swagger/v1/swagger.json`

## Authentication

All API endpoints require authentication via cookie-based authentication. Users must be logged in to access any API endpoint.

## Product Management API Endpoints

### Products

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/product` | GET | Retrieve all products with pagination and filtering |
| `/api/product/{id}` | GET | Get specific product by ID with features and tiers |
| `/api/product` | POST | Create a new product |
| `/api/product/{id}` | PUT | Update an existing product |
| `/api/product/{id}` | DELETE | Delete a product (permanent) |

### Product Features

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/product/{id}/features` | GET | Get all features for a specific product |
| `/api/product/{id}/features` | POST | Create a new feature for a product |

### Product Tiers

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/product/{id}/tiers` | GET | Get all pricing tiers for a specific product |
| `/api/product/{id}/tiers` | POST | Create a new pricing tier for a product |

## Request/Response Models

### CreateProductRequest

```json
{
  "name": "Enterprise License Manager",
  "description": "A comprehensive enterprise licensing management solution",
  "version": "1.0.0",
  "supportEmail": "support@techwayfit.com",
  "supportPhone": "+1-800-555-0123",
  "releaseDate": "2025-08-17T00:00:00Z",
  "status": "Active",
  "features": [],
  "tiers": []
}
```

### ProductResponse

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Enterprise License Manager",
  "description": "A comprehensive enterprise licensing management solution",
  "version": "1.0.0",
  "supportEmail": "support@techwayfit.com",
  "supportPhone": "+1-800-555-0123",
  "releaseDate": "2025-08-17T00:00:00Z",
  "status": "Active",
  "createdOn": "2025-08-17T16:25:05Z",
  "updatedOn": "2025-08-17T16:25:05Z",
  "licenseCount": 0,
  "features": [],
  "tiers": []
}
```

### GetProductsRequest (Query Parameters)

- `page` (int): Page number for pagination (default: 1)
- `pageSize` (int): Number of items per page (default: 20)
- `search` (string): Search term to filter products
- `status` (string): Filter by product status

## Features Implemented

✅ **Comprehensive API Documentation**
- Full Swagger/OpenAPI 3.0 specification
- Detailed endpoint descriptions
- Request/response examples
- Authentication requirements
- Error response codes

✅ **Enhanced Request Models**
- Data validation attributes
- Comprehensive descriptions
- Schema documentation
- Error message specifications

✅ **Rich Response Documentation**
- Detailed HTTP status codes (200, 201, 400, 401, 404, 500)
- Response type specifications
- Error handling documentation

✅ **Organized Endpoint Grouping**
- Products category
- Product Features category
- Product Tiers category

✅ **Developer-Friendly Interface**
- Interactive API testing
- Schema visualization
- Example requests/responses
- Authentication testing

## Configuration

The Swagger configuration includes:

1. **API Information**
   - Title: "TechWayFit Licensing Management API"
   - Version: "v1"
   - Description: Complete API documentation
   - Contact information

2. **Security Configuration**
   - Cookie-based authentication
   - Automatic security requirement application

3. **Documentation Features**
   - Annotations enabled for rich descriptions
   - XML comments integration (if available)
   - Model schema documentation

## Usage Notes

1. **Development Only**: Swagger UI is only enabled in Development environment for security
2. **Authentication Required**: All endpoints require valid authentication
3. **CRUD Operations**: Full Create, Read, Update, Delete operations available
4. **Data Validation**: Comprehensive validation with detailed error messages
5. **Pagination Support**: List endpoints support pagination for performance

## Next Steps

To extend the API documentation:

1. Add XML documentation comments to generate even richer documentation
2. Include example responses for different scenarios
3. Add more detailed error response schemas
4. Include API versioning documentation
5. Add rate limiting information

## Testing

Use the Swagger UI interface to:
1. Explore available endpoints
2. Test API calls interactively
3. View request/response schemas
4. Understand authentication requirements
5. Validate API responses

The Swagger documentation provides a complete, interactive interface for developers to understand and test the TechWayFit Licensing Management API.
