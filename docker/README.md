# TechWayFit Licensing Management - Docker Deployment

This directory contains Docker configuration files and scripts for containerizing and deploying the TechWayFit Licensing Management system.

## 📁 Directory Structure

```
docker/
├── Dockerfile                    # Multi-stage Docker build for web application
├── docker-compose.yml           # Development deployment configuration
├── docker-compose.prod.yml      # Production deployment configuration
├── .dockerignore                 # Files to exclude from Docker build context
├── .env.template                 # Environment variables template
├── build.sh                     # Docker image build script (Unix/Linux/macOS)
├── build.ps1                    # Docker image build script (Windows PowerShell)
├── deploy.sh                    # Deployment script for Unix/Linux/macOS
└── README.md                    # This file
```

## 🚀 Quick Start

### Prerequisites

- Docker 20.10 or later
- Docker Compose 2.0 or later
- At least 4GB RAM available for containers
- At least 10GB disk space

### 1. Environment Setup

Copy the environment template and configure your settings:

```bash
cp .env.template .env
```

Edit the `.env` file with your specific configuration:
- Database passwords
- Redis password
- SMTP settings (if using email notifications)
- SSL certificates (for production)

### 2. Development Deployment

For local development and testing:

```bash
# Build the Docker image
./build.sh

# Deploy development environment
./deploy.sh development
```

Or using Docker Compose directly:

```bash
docker-compose up -d
```

### 3. Production Deployment

For production environments:

```bash
# Build the Docker image with version tag
./build.sh v1.0.0

# Deploy production environment
./deploy.sh production
```

Or using Docker Compose directly:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 🏗️ Build Process

### Building Docker Image

The build process uses a multi-stage Dockerfile to create an optimized production image:

**Unix/Linux/macOS:**
```bash
./build.sh [version]
```

**Windows PowerShell:**
```powershell
.\build.ps1 -Version "v1.0.0" -Push -SecurityScan
```

### Build Stages

1. **Build Stage**: Uses `mcr.microsoft.com/dotnet/sdk:8.0`
   - Restores NuGet packages
   - Compiles the application
   - Publishes optimized release build

2. **Runtime Stage**: Uses `mcr.microsoft.com/dotnet/aspnet:8.0`
   - Copies published application
   - Creates non-root user for security
   - Configures health checks
   - Sets up logging and key directories

## 🐳 Container Architecture

### Services

#### 1. PostgreSQL Database (`postgres`)
- **Image**: `postgres:15-alpine`
- **Purpose**: Primary data store
- **Port**: 5432
- **Volumes**: 
  - `postgres_data`: Database files
  - Database initialization scripts
- **Health Check**: `pg_isready` command

#### 2. Web Application (`licensing-web`)
- **Image**: `techway/licensing-management:latest`
- **Purpose**: Main web interface and API
- **Port**: 8080
- **Volumes**:
  - `licensing_keys`: License key storage
  - `licensing_logs`: Application logs
- **Health Check**: HTTP GET `/health`

#### 3. Redis Cache (`redis`)
- **Image**: `redis:7-alpine`
- **Purpose**: Session storage and caching
- **Port**: 6379
- **Volume**: `redis_data`: Cache persistence
- **Health Check**: `redis-cli ping`

#### 4. Nginx Reverse Proxy (`nginx`) - Production Only
- **Image**: `nginx:alpine`
- **Purpose**: SSL termination and load balancing
- **Ports**: 80, 443
- **Volumes**: SSL certificates and configuration

### Networking

All services communicate through a dedicated Docker network:
- **Development**: `techway-licensing-network`
- **Production**: `techway-licensing-network-prod`

## ⚙️ Configuration

### Environment Variables

Key configuration options available in `.env`:

| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_PASSWORD` | Database password | `SecureP@ssw0rd123!` |
| `REDIS_PASSWORD` | Redis password | `RedisP@ssw0rd123!` |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Development` |
| `LICENSE_DEFAULT_VALIDITY_DAYS` | Default license validity | `365` |
| `ENABLE_HTTPS` | Enable HTTPS enforcement | `false` |
| `LOG_LEVEL` | Application log level | `Information` |

### Database Configuration

The PostgreSQL container is configured with:
- UTF-8 encoding
- Automatic initialization scripts
- Health checks
- Persistent storage

### Security Features

- Non-root user execution
- Resource limits
- Health checks for all services
- Environment-specific security settings
- SSL support in production

## 📊 Monitoring and Maintenance

### Health Checks

All services include comprehensive health checks:
- Database connectivity
- Web application responsiveness
- Cache availability

### Logging

Application logs are available through:
```bash
# View all service logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f licensing-web
docker-compose logs -f postgres
```

### Resource Monitoring

Monitor container resource usage:
```bash
docker stats
```

### Backup and Recovery

Database backups can be performed using:
```bash
# Create backup
docker-compose exec postgres pg_dump -U licensing_user techway_licensing > backup.sql

# Restore backup
docker-compose exec -T postgres psql -U licensing_user techway_licensing < backup.sql
```

## 🔧 Development Workflow

### Local Development

1. **Start services**: `docker-compose up -d`
2. **View logs**: `docker-compose logs -f licensing-web`
3. **Access application**: http://localhost:8080
4. **Access database**: localhost:5432
5. **Stop services**: `docker-compose down`

### Code Changes

For development with live code changes:
```bash
# Rebuild after code changes
docker-compose build licensing-web
docker-compose up -d licensing-web
```

### Database Migrations

Run Entity Framework migrations:
```bash
docker-compose exec licensing-web dotnet ef database update
```

## 🚨 Troubleshooting

### Common Issues

1. **Port Conflicts**
   ```bash
   # Check what's using the port
   lsof -i :8080
   
   # Change ports in docker-compose.yml if needed
   ```

2. **Database Connection Issues**
   ```bash
   # Check database health
   docker-compose exec postgres pg_isready -U licensing_user
   
   # View database logs
   docker-compose logs postgres
   ```

3. **Permission Issues**
   ```bash
   # Fix volume permissions
   docker-compose exec licensing-web chown -R techway:techway /app/Logs
   ```

4. **Memory Issues**
   ```bash
   # Check container resource usage
   docker stats
   
   # Adjust memory limits in docker-compose.yml
   ```

### Log Analysis

Critical log locations:
- Application logs: `/app/Logs/` (inside container)
- Database logs: Docker logs
- Web server logs: Docker logs

### Performance Tuning

1. **Database Performance**
   - Monitor connection pool usage
   - Optimize queries
   - Configure PostgreSQL settings

2. **Application Performance**
   - Enable Redis caching
   - Configure connection strings
   - Monitor memory usage

3. **Container Performance**
   - Adjust resource limits
   - Use production build configurations
   - Enable health checks

## 📈 Scaling and Production Considerations

### Horizontal Scaling

For high-availability deployments:
1. Use external PostgreSQL service
2. Configure Redis cluster
3. Deploy multiple web application instances
4. Use load balancer (nginx/HAProxy)

### Security Hardening

1. **Network Security**
   - Use internal networks
   - Restrict exposed ports
   - Enable firewall rules

2. **Application Security**
   - Enable HTTPS
   - Configure secure headers
   - Use strong passwords
   - Regular security updates

3. **Data Security**
   - Encrypt data at rest
   - Use secure connection strings
   - Regular backups
   - Access control

### Monitoring and Alerting

Consider integrating with:
- Prometheus/Grafana for metrics
- ELK stack for log analysis
- Health check monitoring
- Performance monitoring tools

## 📞 Support

For issues and questions:
1. Check container logs: `docker-compose logs`
2. Verify service health: `docker-compose ps`
3. Review environment configuration
4. Consult application documentation in `/docs`

---

**Version**: 1.0  
**Last Updated**: August 2, 2025  
**Maintained By**: TechWayFit Development Team
