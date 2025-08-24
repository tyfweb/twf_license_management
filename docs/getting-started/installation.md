---
title: Installation
parent: Getting Started
nav_order: 1
---

# Installation Guide
{: .no_toc }

Complete installation instructions for all deployment scenarios.

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Docker Installation

The fastest way to get started is using Docker Compose.

### Prerequisites
- Docker Desktop or Docker Engine
- Docker Compose

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone https://github.com/TechWayFit/licensing-management.git
   cd licensing-management
   ```

2. **Start the services**:
   ```bash
   docker-compose up -d
   ```

3. **Access the application**:
   - Web UI: http://localhost:5000
   - API: http://localhost:5000/api
   - Swagger: http://localhost:5000/swagger

### Docker Configuration

The Docker setup includes:
- **Web Application**: ASP.NET Core application
- **PostgreSQL Database**: For data persistence
- **Redis Cache**: For session management
- **Hangfire Dashboard**: For background job monitoring

To customize the Docker configuration, edit the `docker-compose.yml` file:

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=licensing;Username=postgres;Password=password123
    depends_on:
      - db
      - redis
  
  db:
    image: postgres:14
    environment:
      POSTGRES_DB: licensing
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password123
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
  
  redis:
    image: redis:6-alpine
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

---

## Local Development

For development and customization, set up the application locally.

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL, SQL Server, or SQLite
- Node.js 16+ (for frontend build tools)

### Step-by-Step Installation

#### 1. Clone and Build

```bash
# Clone the repository
git clone https://github.com/TechWayFit/licensing-management.git
cd licensing-management

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build
```

#### 2. Database Setup

Choose your preferred database provider:

**PostgreSQL** (Recommended for production):
```bash
# Install PostgreSQL and create database
createdb licensing_management

# Update appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=licensing_management;Username=your_user;Password=your_password"
  },
  "Infrastructure": {
    "Provider": "PostgreSql"
  }
}
```

**SQL Server**:
```bash
# Create database in SQL Server Management Studio or Azure Data Studio

# Update appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LicensingManagement;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Infrastructure": {
    "Provider": "SqlServer"
  }
}
```

**SQLite** (Development only):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=licensing.db"
  },
  "Infrastructure": {
    "Provider": "Sqlite"
  }
}
```

#### 3. Run Database Migrations

```bash
# Navigate to the web project
cd source/website/TechWayFit.Licensing.Management.Web

# Run migrations
dotnet ef database update
```

#### 4. Run the Application

```bash
# Start the application
dotnet run

# Or for development with hot reload
dotnet watch run
```

The application will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

---

## Production Deployment

### Prerequisites for Production

- **Server**: Windows Server 2016+ or Linux
- **Runtime**: .NET 8.0 Runtime
- **Database**: PostgreSQL 12+ or SQL Server 2019+
- **Web Server**: IIS, Nginx, or Apache (optional)
- **SSL Certificate**: For HTTPS

### Deployment Options

#### Option 1: IIS Deployment (Windows)

1. **Publish the application**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure IIS**:
   - Install ASP.NET Core Hosting Bundle
   - Create a new website in IIS
   - Point to the published folder
   - Configure application pool for "No Managed Code"

3. **Configure SSL**:
   - Bind SSL certificate to the website
   - Redirect HTTP to HTTPS

#### Option 2: Linux with Nginx

1. **Install .NET Runtime**:
   ```bash
   # Ubuntu/Debian
   sudo apt update
   sudo apt install aspnetcore-runtime-8.0
   ```

2. **Deploy application**:
   ```bash
   # Copy published files to /var/www/licensing
   sudo cp -r ./publish/* /var/www/licensing/
   sudo chown -R www-data:www-data /var/www/licensing/
   ```

3. **Configure systemd service**:
   ```ini
   # /etc/systemd/system/licensing.service
   [Unit]
   Description=TechWayFit Licensing Management
   After=network.target

   [Service]
   Type=notify
   ExecStart=/usr/bin/dotnet /var/www/licensing/TechWayFit.Licensing.Management.Web.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=licensing
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production

   [Install]
   WantedBy=multi-user.target
   ```

4. **Configure Nginx**:
   ```nginx
   server {
       listen 80;
       server_name your-domain.com;
       return 301 https://$server_name$request_uri;
   }

   server {
       listen 443 ssl http2;
       server_name your-domain.com;
       
       ssl_certificate /path/to/certificate.crt;
       ssl_certificate_key /path/to/private.key;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```

#### Option 3: Cloud Deployment

**Azure App Service**:
```bash
# Install Azure CLI and login
az login

# Create resource group
az group create --name licensing-rg --location eastus

# Create App Service plan
az appservice plan create --name licensing-plan --resource-group licensing-rg --sku S1

# Create web app
az webapp create --name licensing-app --resource-group licensing-rg --plan licensing-plan --runtime "DOTNETCORE|8.0"

# Deploy from local Git
az webapp deployment source config-local-git --name licensing-app --resource-group licensing-rg
```

**AWS Elastic Beanstalk**:
```bash
# Install EB CLI
pip install awsebcli

# Initialize EB application
eb init

# Create environment and deploy
eb create production
eb deploy
```

### Production Configuration

Update `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Database=licensing;Username=app_user;Password=secure_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Security": {
    "RequireHttps": true,
    "AllowedHosts": ["your-domain.com"]
  },
  "Infrastructure": {
    "Provider": "PostgreSql"
  }
}
```

---

## Verification

After installation, verify the system is working:

### 1. Health Check
Visit `/health` endpoint to verify system status.

### 2. Database Connection
Check that migrations have been applied:
```bash
dotnet ef migrations list
```

### 3. API Functionality
Test the API endpoints:
```bash
curl -X GET "https://your-domain.com/api/health" -H "accept: application/json"
```

### 4. Web Interface
Access the web interface and verify:
- Login functionality
- Dashboard loads correctly
- Navigation works

---

## Next Steps

Once installation is complete:

1. **[Configure the System](configuration.html)** - Set up authentication, authorization, and system settings
2. **[Security Setup](../administration/security.html)** - Configure encryption keys and security policies
3. **[Create Admin User](../administration/user-management.html)** - Set up administrative access
4. **[Configure Tenants](../administration/multi-tenant.html)** - Set up multi-tenant architecture (if needed)

---

## Troubleshooting

### Common Issues

**Database Connection Errors**:
- Verify connection string
- Check database server is running
- Ensure user has proper permissions

**SSL/HTTPS Issues**:
- Verify certificate installation
- Check port bindings
- Review firewall settings

**Performance Issues**:
- Enable application insights
- Configure caching
- Review database indexes

For more detailed troubleshooting, see the [Troubleshooting Guide](troubleshooting.html).
