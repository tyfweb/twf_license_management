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
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Extensions;
using TechWayFit.Licensing.Management.Services.Implementations.User;
using TechWayFit.Licensing.Management.Services.Implementations.Account;
using TechWayFit.Licensing.Management.Services.Implementations.Workflow;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using Serilog;
using Serilog.Events;
// OPERATIONS DASHBOARD - DISABLED FOR CORE FOCUS
// using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
// using TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;
// using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
// using TechWayFit.Licensing.Management.Services.Implementations.OperationsDashboard;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Web.Middleware;
using TechWayFit.Licensing.Management.Core.Contracts;
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

    // Add Razor runtime compilation for development
    builder.Services.AddRazorPages()
        .AddRazorRuntimeCompilation();

    // Add memory cache for performance
    builder.Services.AddMemoryCache();

    // Configure EF Core logging services
    builder.Services.ConfigureEfCoreLogging();

    // Configure PostgreSQL Database to be used
    // This will register the PostgreSQL DbContext and configure it with the connection string
    // and other options from the configuration
    builder.Services.AddPostgreSqlInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();

    // Register User Context
    builder.Services.AddScoped<IUserContext, TwfUserContext>();
    RegisterServices(builder);    
    builder.Services.AddScoped<AuthenticationManager>();
    
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

static void RegisterServices(WebApplicationBuilder builder)
{
    // Register real services  
    builder.Services.AddScoped<IEnterpriseProductService, EnterpriseProductService>();
    builder.Services.AddScoped<IProductTierService, ProductTierService>();

    // Step 4: Consumer management services  
    builder.Services.AddScoped<IConsumerAccountService, ConsumerAccountService>();

    // Step 5: License management services
    builder.Services.AddScoped<ILicenseGenerator, StatelessLicenseGenerator>();
    builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();
    builder.Services.AddScoped<IProductLicenseService, ProductLicenseService>();
    builder.Services.AddScoped<IProductFeatureService, ProductFeatureService>();
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

    // OPERATIONS DASHBOARD - DISABLED FOR CORE FOCUS
    // Step 10: Operations Dashboard services
    // builder.Services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();

    // Register authentication service
    builder.Services.AddScoped<IAuthenticationService, AccountService>();
    
    // Register generic workflow services for approval system
    builder.Services.AddScoped(typeof(IWorkflowService<>), typeof(WorkflowService<,>));
    
    // Register specific workflow services for approval system
    builder.Services.AddScoped<IConsumerAccountWorkflowService, ConsumerAccountWorkflowService>();
    builder.Services.AddScoped<IEnterpriseProductWorkflowService, EnterpriseProductWorkflowService>();
    builder.Services.AddScoped<IProductLicenseWorkflowService, ProductLicenseWorkflowService>();
}