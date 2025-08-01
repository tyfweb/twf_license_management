using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.Management.Services.Implementations.Product;
using TechWayFit.Licensing.Management.Services.Implementations.Consumer;
using TechWayFit.Licensing.Management.Services.Implementations;
using TechWayFit.Licensing.Management.Web.Models.Authentication;
using TechWayFit.Licensing.Management.Web.Services;
// using TechWayFit.Licensing.Management.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Management.Services.Implementations.User;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories;
using TechWayFit.Licensing.Management.Services.Implementations.Account;
using Serilog;
using Serilog.Events;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Services.Implementations.OperationsDashboard;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Web.Middleware;

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

    // Add Razor runtime compilation for development
    builder.Services.AddRazorPages()
        .AddRazorRuntimeCompilation();

    // Add memory cache for performance
    builder.Services.AddMemoryCache();

    // Configure EF Core logging services
    builder.Services.ConfigureEfCoreLogging();

    // Configure Entity Framework with PostgreSQL (Database First approach)
    builder.Services.AddDbContext<LicensingDbContext>((serviceProvider, options) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Database First: Assume database schema exists
            // No migrations - schema managed via SQL scripts
            npgsqlOptions.MigrationsAssembly((string?)null);
        })
        // Configure PostgreSQL to use snake_case naming convention
        .UseSnakeCaseNamingConvention();

        // Add custom SQL logging interceptor
        var sqlInterceptor = serviceProvider.GetService<SqlLoggingInterceptor>();
        if (sqlInterceptor != null)
        {
            options.AddInterceptors(sqlInterceptor);
        }

        // Add enhanced SQL interceptor for operations dashboard metrics
        var enhancedSqlInterceptor = serviceProvider.GetService<EnhancedSqlInterceptor>();
        if (enhancedSqlInterceptor != null)
        {
            options.AddInterceptors(enhancedSqlInterceptor);
        }

        // Enable sensitive data logging in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    RegisterRepositories(builder);
    RegisterServices(builder);    
    builder.Services.AddScoped<AuthenticationManager>();
    
    // Register operations dashboard data collection services
    builder.Services.AddScoped<EnhancedSqlInterceptor>();
    builder.Services.AddSingleton<MetricsBufferService>();
    builder.Services.AddHostedService<MetricsBufferService>(provider => provider.GetService<MetricsBufferService>()!);
    builder.Services.AddHostedService<SystemHealthCollectionService>();
    
    var app = builder.Build();

    // Add correlation ID middleware first to ensure all logs have correlation ID
    app.UseCorrelationId();

    // Use Serilog for request logging with correlation ID support
    app.ConfigureSerilogRequestLogging();

    // Add operations dashboard middlewares for data collection
    app.UsePerformanceTracking();
    app.UseErrorTracking();

    // Create logs directory
    var logsPath = Path.Combine(app.Environment.ContentRootPath, "Logs");
    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
        Log.Information("Created logs directory: {LogsPath}", logsPath);
    }

    // Initialize database if using PostgreSQL
    // await app.InitializeDatabaseAsync(builder.Configuration);

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();
    app.UseRouting();

    // Add authentication middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

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

static void RegisterRepositories(WebApplicationBuilder builder)
{
    // Register repositories
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IConsumerAccountRepository, ConsumerAccountRepository>();
    builder.Services.AddScoped<IProductFeatureRepository, ProductFeatureRepository>();
    builder.Services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();
    builder.Services.AddScoped<IProductLicenseRepository, ProductLicenseRepository>();
    builder.Services.AddScoped<INotificationHistoryRepository, NotificationHistoryRepository>();
    builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
    builder.Services.AddScoped<IProductTierRepository, ProductTierRepository>();
    builder.Services.AddScoped<IProductVersionRepository, ProductVersionRepository>();
    builder.Services.AddScoped<ISettingRepository, SettingRepository>();

    // Register user management repositories
    builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
    builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    builder.Services.AddScoped<IUserRoleMappingRepository, UserRoleMappingRepository>();
    
    // Register operations dashboard repositories
    builder.Services.AddScoped<ISystemMetricRepository, SystemMetricRepository>();
    builder.Services.AddScoped<IErrorLogSummaryRepository, ErrorLogSummaryRepository>();
    builder.Services.AddScoped<IPagePerformanceMetricRepository, PagePerformanceMetricRepository>();
    builder.Services.AddScoped<IQueryPerformanceMetricRepository, QueryPerformanceMetricRepository>();
    builder.Services.AddScoped<ISystemHealthSnapshotRepository, SystemHealthSnapshotRepository>();
}

static void RegisterServices(WebApplicationBuilder builder)
{
    // Register real services (replacing mock)
    builder.Services.AddScoped<IEnterpriseProductService, EnterpriseProductService>();

    // Step 4: Consumer management services - IMPLEMENTING NOW
    builder.Services.AddScoped<IConsumerAccountService, ConsumerAccountService>();

    // Step 5: License management services
    builder.Services.AddScoped<ILicenseGenerator, StatelessLicenseGenerator>();
    builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();
    builder.Services.AddScoped<IProductLicenseService, ProductLicenseService>();
    builder.Services.AddSingleton<ILicenseValidationService, LicenseValidationService>();

    // Step 6: Settings management services
    builder.Services.AddScoped<ISettingService, SettingService>();

    // Step 6a: Settings helper services
    //builder.Services.AddScoped<TechWayFit.Licensing.Management.Web.Helpers.SettingsHelper>();

    // Step 7: Audit management services
    builder.Services.AddScoped<IAuditService, TechWayFit.Licensing.Management.Services.Implementations.Audit.AuditService>();

    // Step 8: Notification management services
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // Step 9: User management services
    builder.Services.AddScoped<IUserService, UserService>();

    // Step 10: Operations Dashboard services
    builder.Services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();

    // Register authentication service
    builder.Services.AddScoped<IAuthenticationService, AccountService>();
}