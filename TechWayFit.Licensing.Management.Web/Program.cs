using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.WebUI.Models.Authentication;
using TechWayFit.Licensing.WebUI.Services;
// using TechWayFit.Licensing.WebUI.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;

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
// Step 3: Product management services will be added here  
// Step 4: Consumer management services will be added here
// Step 5: License management services will be added here
// Step 6: Audit management services will be added here
// Step 7: Notification management services will be added here

// Commented out for now - will be added back step by step:
// builder.Services.AddScoped<ILicenseGenerator, StatelessLicenseGenerator>();
// builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();
// builder.Services.AddScoped<IProductLicenseService, ProductLicenseService>();
// builder.Services.AddSingleton<ILicenseValidationService, LicenseValidationService>();

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