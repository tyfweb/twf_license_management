using Serilog;
using TechWayFit.Licensing.Management.Infrastructure.Extensions;
using TechWayFit.Licensing.Management.Web.Extensions;

// Configure Serilog early to capture startup issues
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TechWayFit Licensing Management Web Application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure comprehensive Serilog logging
    builder.ConfigureSerilog();

    // Add product configuration from external file
    builder.Configuration.AddJsonFile("product-config.json", optional: false, reloadOnChange: true);
    
    // Configure product settings
    builder.Services.Configure<TechWayFit.Licensing.Management.Web.Configuration.ProductConfiguration>(
        builder.Configuration.GetSection(TechWayFit.Licensing.Management.Web.Configuration.ProductConfiguration.SectionName));

    // Configure services using extension methods
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddMvcAndSwaggerServices(builder.Environment);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddBusinessServices();

    var app = builder.Build();

    // Configure middleware pipeline
    app.ConfigureMiddlewarePipeline(builder.Environment);

    // Seed database with initial data (for InMemory provider)
    try
    {
        Log.Information("Seeding database with initial data...");
        var seedCount = await app.Services.SeedDatabaseAsync();
        Log.Information("Database seeding completed successfully. {SeedCount} seeders executed", seedCount);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database seeding failed, but application will continue");
    }

    // Configure recurring jobs
    HangfireJobExtensions.ConfigureRecurringJobs();

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