using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.Management.Services.Implementations.Product;
using TechWayFit.Licensing.Management.Services.Implementations.Consumer;
using TechWayFit.Licensing.Management.Services.Implementations;
using TechWayFit.Licensing.WebUI.Models.Authentication;
using TechWayFit.Licensing.WebUI.Services;
// using TechWayFit.Licensing.WebUI.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Infrastructure.Data.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Data.Repositories.Consumer;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Infrastructure.Implementations.Repositories.Audit;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Infrastructure.Data.Repositories.License;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Infrastructure.Data.Repositories.Notification;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Infrastructure.Data.Repositories.Settings;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure authentication settings
builder.Services.Configure<AuthenticationSettings>(
    builder.Configuration.GetSection("Authentication"));

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

// Register authentication service
builder.Services.AddScoped<IAuthenticationService, SimpleAuthenticationService>();

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

// Configure Entity Framework with PostgreSQL (Database First approach)
builder.Services.AddDbContext<LicensingDbContext>(options =>
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
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure paths
var contentRootPath = builder.Environment.ContentRootPath;
var keyStorePath = Path.Combine(contentRootPath, "Keys");
var dataPath = Path.Combine(contentRootPath, "Data");

// Ensure directories exist
Directory.CreateDirectory(keyStorePath);
Directory.CreateDirectory(dataPath);

// Configure repository type based on configuration
// TODO: Step 1 - Temporarily commented out until we rebuild repository configuration
// builder.Services.ConfigureRepositories(builder.Configuration);

// TODO: Step-by-step service registration as we build each layer

// Step 1: Basic services only (for clean build)
// Step 2: Authentication services will be added here
// Step 3: Product management services - IMPLEMENTED (using real service now)

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
builder.Services.AddScoped<TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Settings.ISettingRepository, TechWayFit.Licensing.Infrastructure.Data.Repositories.Settings.SettingRepository>();

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
builder.Services.AddScoped<TechWayFit.Licensing.Management.Web.Helpers.SettingsHelper>();

// Step 7: Audit management services will be added here
// Step 8: Notification management services
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

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

app.Run();