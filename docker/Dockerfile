# TechWayFit Licensing Management - Multi-stage Docker Build
# This Dockerfile creates a production-ready container for the licensing management system

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY source/*.sln .
COPY source/Directory.Build.props .

# Copy all project files
COPY source/TechWayFit.Licensing.Core/ TechWayFit.Licensing.Core/
COPY source/TechWayFit.Licensing.Generator/ TechWayFit.Licensing.Generator/
COPY source/TechWayFit.Licensing.Management.Core/ TechWayFit.Licensing.Management.Core/
COPY source/TechWayFit.Licensing.Management.Infrastructure/ TechWayFit.Licensing.Management.Infrastructure/
COPY source/TechWayFit.Licensing.Management.Infrastructure.Abstractions/ TechWayFit.Licensing.Management.Infrastructure.Abstractions/
COPY source/TechWayFit.Licensing.Management.Infrastructure.PostgreSql/ TechWayFit.Licensing.Management.Infrastructure.PostgreSql/
COPY source/TechWayFit.Licensing.Management.Services/ TechWayFit.Licensing.Management.Services/
COPY source/TechWayFit.Licensing.Management.Web/ TechWayFit.Licensing.Management.Web/
COPY source/TechWayFit.Licensing.Validation/ TechWayFit.Licensing.Validation/

# Restore dependencies
RUN dotnet restore TechWayFit.Licensing.sln

# Build the application
RUN dotnet build TechWayFit.Licensing.sln -c Release --no-restore

# Publish the web application
RUN dotnet publish TechWayFit.Licensing.Management.Web/TechWayFit.Licensing.Management.Web.csproj \
    -c Release \
    --no-build \
    -o /app/publish \
    --self-contained false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 techway && \
    adduser --system --uid 1001 --ingroup techway --shell /bin/false techway

# Install required packages for PostgreSQL connectivity
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directories for logs and keys with proper permissions
RUN mkdir -p /app/Logs /app/Keys && \
    chown -R techway:techway /app

# Copy database scripts (optional for migrations)
COPY source/Database/ /app/Database/

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Switch to non-root user
USER techway

# Entry point
ENTRYPOINT ["dotnet", "TechWayFit.Licensing.Management.Web.dll"]
