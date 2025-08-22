using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.Management.Services.Implementations.Product;
using TechWayFit.Licensing.Management.Services.Implementations.Consumer;
using TechWayFit.Licensing.Management.Services.Implementations;
using TechWayFit.Licensing.Management.Web.Services;
// using TechWayFit.Licensing.Management.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using TechWayFit.Licensing.Management.Infrastructure.Extensions;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Services.Implementations.User;
using TechWayFit.Licensing.Management.Services.Implementations.Account;
using TechWayFit.Licensing.Management.Services.Implementations.Workflow;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using Serilog;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Web.Middleware;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.SqlServer.Extensions;
using Hangfire;
using Hangfire.MemoryStorage;
using TechWayFit.Licensing.Management.Web.Services.Jobs;
// OPERATIONS DASHBOARD MIDDLEWARE - DISABLED FOR CORE FOCUS
// using TechWayFit.Licensing.Management.Web.Middleware;

// Configure Serilog early to capture startup issues
Log.Logger = new LoggerConfiguration()
    .ConfigureBootstrapLogging()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TechWayFit Licensing Management Web Application");

    var builder = WebApplication.CreateBuilder(args);

    // Clear default logging providers and use Serilog
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ConfigureStructuredLogging(context.HostingEnvironment.IsDevelopment())
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());


    // Add authentication services
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "TechWayFitLicensing";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

    builder.Services.AddAuthorization();

    // Add services to the container.
    builder.Services.AddControllersWithViews()
        .AddRazorOptions(options =>
        {
            // Ensure tag helpers are enabled
            options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
        });

    // Add Swagger/OpenAPI services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "TechWayFit Licensing Management API",
            Version = "v1",
            Description = "API for managing enterprise licensing, products, features, and consumers",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "TechWayFit Support",
                Email = "support@techwayfit.com"
            }
        });

        // Enable annotations for richer documentation
        c.EnableAnnotations();

        // Include XML comments for better API documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Configure authorization for Swagger
        c.AddSecurityDefinition("Cookie", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authentication",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
            Description = "Cookie-based authentication"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Cookie"
                    }
                },
                new string[] { }
            }
        });
    });

    // Add Razor runtime compilation for development
    builder.Services.AddRazorPages()
        .AddRazorRuntimeCompilation();

    // Add memory cache for performance
    builder.Services.AddMemoryCache();

    // Add session state for tenant switching
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "TwfSession";
    });

    // Configure EF Core logging services
    builder.Services.ConfigureEfCoreLogging();

    // Configure SQLite Database for local development (easier to inspect than in-memory)
    // This will create a local licensing.db file that you can open with any SQLite browser
    builder.Services.AddSqliteInfrastructure("licensing.db");
    builder.Services.AddHttpContextAccessor();

    // Configure Hangfire for job scheduling
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage());
    
    // Add Hangfire server
    builder.Services.AddHangfireServer();

    // Configure product tier settings
    builder.Services.Configure<TechWayFit.Licensing.Management.Web.Configuration.ProductTierConfiguration>(
        builder.Configuration.GetSection(TechWayFit.Licensing.Management.Web.Configuration.ProductTierConfiguration.SectionName));

    // Add product configuration from external file
    builder.Configuration.AddJsonFile("product-config.json", optional: false, reloadOnChange: true);
    
    // Configure product settings
    builder.Services.Configure<TechWayFit.Licensing.Management.Web.Configuration.ProductConfiguration>(
        builder.Configuration.GetSection(TechWayFit.Licensing.Management.Web.Configuration.ProductConfiguration.SectionName));

    // Register tenant scope infrastructure for system operations
    builder.Services.AddSingleton<ITenantScope, TenantScope>();
    builder.Services.AddScoped<IUserContext, TenantAwareUserContext>();
    RegisterServices(builder);    
    builder.Services.AddScoped<AuthenticationManager>();
    
    // Register seeding services
    builder.Services.AddSeedingServices();
    
    // OPERATIONS DASHBOARD - DISABLED FOR CORE FOCUS
    // Register operations dashboard data collection services
    // builder.Services.AddScoped<EnhancedSqlInterceptor>();
    // builder.Services.AddSingleton<MetricsBufferService>();
    // builder.Services.AddHostedService<MetricsBufferService>(provider => provider.GetService<MetricsBufferService>()!);
    // builder.Services.AddHostedService<SystemHealthCollectionService>();
    
    var app = builder.Build();

    // Add correlation ID middleware first to ensure all logs have correlation ID
    app.UseCorrelationId();

    // Use Serilog for request logging with correlation ID support
    app.ConfigureSerilogRequestLogging();

    // OPERATIONS DASHBOARD - DISABLED FOR CORE FOCUS
    // Add operations dashboard middlewares for data collection
    // app.UsePerformanceTracking();
    // app.UseErrorTracking();

    // Create logs directory
    var logsPath = Path.Combine(app.Environment.ContentRootPath, "Logs");
    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
        Log.Information("Created logs directory: {LogsPath}", logsPath);
    }

    // Initialize database if using SQLite
    try
    {
        Log.Information("Initializing SQLite database...");
        
        using var initScope = app.Services.CreateScope();
        var dbContext = initScope.ServiceProvider.GetRequiredService<EfCoreLicensingDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        Log.Information("SQLite database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize SQLite database");
        throw; // Stop the application if database initialization fails
    }

    // Seed database with initial data
    try
    {
        Log.Information("Starting database seeding...");
        
        // Create a scope to get the tenant scope service and wrap seeding in system tenant context
        using var seedingScope = app.Services.CreateScope();
        var tenantScope = seedingScope.ServiceProvider.GetRequiredService<ITenantScope>();
        
        // Execute seeding within system tenant scope
        using var systemScope = tenantScope.CreateSystemScope();
        var seedCount = await app.Services.SeedDatabaseAsync();
        
        Log.Information("Database seeding completed successfully. {SeedCount} seeders executed", seedCount);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to seed database");
        // Don't stop the application if seeding fails
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }
    else
    {
        // Enable Swagger in development environment
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechWayFit Licensing API v1");
            c.RoutePrefix = "api/docs"; // Swagger will be available at /api/docs
            c.DocumentTitle = "TechWayFit Licensing API Documentation";
            c.DefaultModelsExpandDepth(-1); // Hide models section by default
        });
    }

    app.UseStaticFiles();
    app.UseRouting();

    // Add Hangfire Dashboard (only in development or for admin users)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new DashboardAuthorizationFilter() },
        AppPath = "/System", // Custom app path - links back to your System dashboard
        DashboardTitle = "TechWayFit Licensing - Job Dashboard",
        DisplayStorageConnectionString = false,
        DarkModeEnabled = false,
        DefaultRecordsPerPage = 20,
        StatsPollingInterval = 5000 // 5 second refresh
    });

    // Add session middleware (must be before authentication)
    app.UseSession();

    // Add authentication middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Configure recurring jobs
    ConfigureRecurringJobs();

    Log.Information("TechWayFit Licensing Management Web Application started successfully");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("TechWayFit Licensing Management Web Application shutting down");
    Log.CloseAndFlush();
}

/// <summary>
/// Configure Hangfire recurring jobs
/// </summary>
static void ConfigureRecurringJobs()
{
    Log.Information("Configuring Hangfire recurring jobs");
    
    // License management jobs
    RecurringJob.AddOrUpdate<LicenseJobService>(
        "license-expiry-check",
        service => service.CheckExpiringLicensesAsync(),
        Cron.Daily(9)); // Run daily at 9 AM
    
    RecurringJob.AddOrUpdate<LicenseJobService>(
        "deactivate-expired-licenses",
        service => service.DeactivateExpiredLicensesAsync(),
        Cron.Daily(2)); // Run daily at 2 AM
    
    RecurringJob.AddOrUpdate<LicenseJobService>(
        "license-usage-reports",
        service => service.GenerateLicenseUsageReportsAsync(),
        Cron.Weekly(DayOfWeek.Monday, 8)); // Run weekly on Monday at 8 AM
    
    // Audit management jobs
    RecurringJob.AddOrUpdate<AuditJobService>(
        "audit-cleanup",
        service => service.CleanupOldAuditEntriesAsync(),
        Cron.Weekly(DayOfWeek.Sunday, 3)); // Run weekly on Sunday at 3 AM
    
    RecurringJob.AddOrUpdate<AuditJobService>(
        "audit-archival",
        service => service.ArchiveOldAuditEntriesAsync(),
        Cron.Monthly(1, 1)); // Run monthly on the 1st at 1 AM
    
    RecurringJob.AddOrUpdate<AuditJobService>(
        "audit-summary-report",
        service => service.GenerateAuditSummaryReportAsync(),
        Cron.Weekly(DayOfWeek.Monday, 10)); // Run weekly on Monday at 10 AM
    
    // System maintenance jobs
    RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
        "database-maintenance",
        service => service.PerformDatabaseMaintenanceAsync(),
        Cron.Weekly(DayOfWeek.Saturday, 1)); // Run weekly on Saturday at 1 AM
    
    RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
        "cleanup-temp-files",
        service => service.CleanupTemporaryFilesAsync(),
        Cron.Daily(4)); // Run daily at 4 AM
    
    RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
        "system-health-monitoring",
        service => service.MonitorSystemHealthAsync(),
        Cron.Hourly()); // Run every hour
    
    RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
        "performance-reports",
        service => service.GeneratePerformanceReportAsync(),
        Cron.Daily(7)); // Run daily at 7 AM
    
    Log.Information("Hangfire recurring jobs configured successfully");
}

static void RegisterServices(WebApplicationBuilder builder)
{
    // Register real services  
    builder.Services.AddScoped<IEnterpriseProductService, EnterpriseProductService>();
    builder.Services.AddScoped<IProductTierService, ProductTierService>();

    // Step 4: Consumer management services  
    builder.Services.AddScoped<IConsumerAccountService, ConsumerAccountService>();

    // Step 5: License management services
    builder.Services.AddScoped<ILicenseGenerator, StatelessLicenseGenerator>();
    builder.Services.AddScoped<IKeyManagementService, DatabaseKeyManagementService>();
    builder.Services.AddScoped<IProductLicenseService, ProductLicenseService>();
    builder.Services.AddScoped<IProductFeatureService, ProductFeatureService>();
    builder.Services.AddScoped<IProductActivationService, ProductActivationService>();
    
    // Legacy validation service from TechWayFit.Licensing.Core
    builder.Services.AddSingleton<TechWayFit.Licensing.Core.Contracts.ILicenseValidationService, TechWayFit.Licensing.Core.Services.LicenseValidationService>();

    // Step 6: Settings management services
    builder.Services.AddScoped<ISettingService, SettingService>();

    // Step 6a: Settings helper services
    //builder.Services.AddScoped<TechWayFit.Licensing.Management.Web.Helpers.SettingsHelper>();

    // Step 7: Audit management services
    builder.Services.AddScoped<IAuditService, TechWayFit.Licensing.Management.Services.Implementations.Audit.AuditService>();

    // Step 8: Notification management services
    builder.Services.AddScoped<INotificationService, NotificationService>();    // Step 9: User management services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPermissionService, TechWayFit.Licensing.Management.Services.User.PermissionService>();

    // Step 10: Tenant management services
    builder.Services.AddScoped<ITenantService, TechWayFit.Licensing.Management.Services.Implementations.Tenant.TenantService>();

    // OPERATIONS DASHBOARD - DISABLED FOR CORE FOCUS
    // Step 11: Operations Dashboard services
    // builder.Services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();

    // Register authentication service
    builder.Services.AddScoped<IAuthenticationService, AccountService>();
    
    // Register generic workflow services for approval system
    builder.Services.AddScoped(typeof(IWorkflowService<>), typeof(WorkflowService<>));
    
    // Register specific workflow services for approval system
    builder.Services.AddScoped<IConsumerAccountWorkflowService, ConsumerAccountWorkflowService>();
    builder.Services.AddScoped<IEnterpriseProductWorkflowService, EnterpriseProductWorkflowService>();
    builder.Services.AddScoped<IProductLicenseWorkflowService, ProductLicenseWorkflowService>();
    
    // Register Hangfire job services
    builder.Services.AddScoped<LicenseJobService>();
    builder.Services.AddScoped<AuditJobService>();
    builder.Services.AddScoped<SystemMaintenanceJobService>();
}