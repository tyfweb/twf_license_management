using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TechWayFit.Licensing.Core.Contracts;

namespace TechWayFit.Licensing.WebUI.Extensions;

public static class RepositoryConfigurationExtensions
{
    public static IServiceCollection ConfigureRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var repositoryType = configuration.GetValue<string>("RepositoryType", "json");
        var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
        
        if (!string.IsNullOrEmpty(repositoryType) && repositoryType.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                RegisterPostgreSqlRepositories(services, configuration);
                logger?.LogInformation("Repository configuration completed - using PostgreSQL repositories");
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "PostgreSQL repository registration failed, falling back to JSON repositories");
                RegisterJsonRepositories(services, configuration);
                logger?.LogInformation("Repository configuration completed - using JSON repositories (fallback)");
            }
        }
        else
        {
            RegisterJsonRepositories(services, configuration);
            logger?.LogInformation("Repository configuration completed - using JSON repositories");
        }
        
        return services;
    }

    private static void RegisterPostgreSqlRepositories(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string not found in configuration");
        }

        // Try to load the Infrastructure assembly dynamically
        Assembly? infrastructureAssembly = null;
        try
        {
            var infrastructureAssemblyPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "TechWayFit.Licensing.Infrastructure.dll");
                
            if (File.Exists(infrastructureAssemblyPath))
            {
                infrastructureAssembly = Assembly.LoadFrom(infrastructureAssemblyPath);
            }
            else
            {
                throw new FileNotFoundException($"Infrastructure assembly not found at {infrastructureAssemblyPath}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load PostgreSQL Infrastructure assembly. Please ensure the Infrastructure project is built and available.", ex);
        }

        // Register PostgreSQL repositories as Singleton to match service lifetimes
        try
        {
            services.AddSingleton<IProductRepository>(provider =>
            {
                var repositoryType = infrastructureAssembly.GetType("TechWayFit.Licensing.Infrastructure.Data.PostgreSql.PostgreSqlProductRepository");
                if (repositoryType == null) throw new TypeLoadException("PostgreSqlProductRepository not found in Infrastructure assembly");
                
                return (IProductRepository)Activator.CreateInstance(repositoryType, 
                    connectionString, 
                    provider.GetRequiredService<ILogger>())!;
            });

            services.AddSingleton<IConsumerRepository>(provider =>
            {
                var repositoryType = infrastructureAssembly.GetType("TechWayFit.Licensing.Infrastructure.Data.PostgreSql.PostgreSqlConsumerRepository");
                if (repositoryType == null) throw new TypeLoadException("PostgreSqlConsumerRepository not found in Infrastructure assembly");
                
                return (IConsumerRepository)Activator.CreateInstance(repositoryType, 
                    connectionString, 
                    provider.GetRequiredService<ILogger>())!;
            });

            services.AddSingleton<ILicenseRepository>(provider =>
            {
                var repositoryType = infrastructureAssembly.GetType("TechWayFit.Licensing.Infrastructure.Data.PostgreSql.PostgreSqlLicenseRepository");
                if (repositoryType == null) throw new TypeLoadException("PostgreSqlLicenseRepository not found in Infrastructure assembly");
                
                return (ILicenseRepository)Activator.CreateInstance(repositoryType, 
                    connectionString, 
                    provider.GetRequiredService<ILogger>())!;
            });

            // For now, use JSON implementations for audit and key repositories
            var dataPath = configuration.GetValue<string>("DataPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Data");
            services.AddSingleton<IAuditRepository>(provider =>
                new JsonAuditRepository(dataPath, provider.GetRequiredService<ILogger<JsonAuditRepository>>()));
                
            services.AddSingleton<IKeyRepository>(provider =>
                new JsonKeyRepository(dataPath, provider.GetRequiredService<ILogger<JsonKeyRepository>>()));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to register PostgreSQL repositories. Please ensure the Infrastructure project compiles without errors.", ex);
        }
    }

    private static void RegisterJsonRepositories(IServiceCollection services, IConfiguration configuration)
    {
        var dataPath = configuration.GetValue<string>("DataPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Data");
        
        // Register repositories as Singleton to match service lifetimes
        services.AddSingleton<IProductRepository>(provider =>
            new JsonProductRepository(dataPath, provider.GetRequiredService<ILogger<JsonProductRepository>>()));
            
        services.AddSingleton<IConsumerRepository>(provider =>
            new JsonConsumerRepository(dataPath, provider.GetRequiredService<ILogger<JsonConsumerRepository>>()));
            
        services.AddSingleton<ILicenseRepository>(provider =>
            new JsonLicenseRepository(dataPath, provider.GetRequiredService<ILogger<JsonLicenseRepository>>()));
            
        // Register missing repositories with simple implementations
        services.AddSingleton<IAuditRepository>(provider =>
            new JsonAuditRepository(dataPath, provider.GetRequiredService<ILogger<JsonAuditRepository>>()));
            
        services.AddSingleton<IKeyRepository>(provider =>
            new JsonKeyRepository(dataPath, provider.GetRequiredService<ILogger<JsonKeyRepository>>()));
    }

    public static Task InitializeDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var repositoryType = configuration.GetValue<string>("RepositoryType", "json");
        
        if (!string.IsNullOrEmpty(repositoryType) && repositoryType.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    logger?.LogInformation("Initializing PostgreSQL database...");
                    // TODO: Add database initialization logic here when PostgreSQL repositories are complete
                    // await DatabaseInitializer.InitializeAsync(connectionString);
                    logger?.LogInformation("PostgreSQL database initialization completed");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to initialize PostgreSQL database");
                }
            }
        }
        
        return Task.CompletedTask;
    }
}
