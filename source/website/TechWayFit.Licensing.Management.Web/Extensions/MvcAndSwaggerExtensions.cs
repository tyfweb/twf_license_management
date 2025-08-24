using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Razor;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring MVC and Swagger services
/// </summary>
public static class MvcAndSwaggerExtensions
{
    /// <summary>
    /// Adds MVC, Swagger, and related services to the service collection
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configured service collection</returns>
    public static IServiceCollection AddMvcAndSwaggerServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Add MVC services
        services.AddControllersWithViews()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

        // Configure Razor view engine for custom layout structure
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Clear();
            options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/Layout/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/Partial/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/Components/{0}.cshtml");

            options.AreaViewLocationFormats.Clear();
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Views/Shared/Layout/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Views/Shared/Partial/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Views/Shared/Components/{0}.cshtml");
        });

        // Add API explorer and Swagger only in development
        if (environment.IsDevelopment())
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "TechWayFit Licensing Management API", 
                    Version = "v1",
                    Description = "API for managing licenses, products, and consumer accounts"
                });

                // Include XML comments for better API documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configure authorization for Swagger
                c.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
                {
                    Name = "Authentication",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Description = "Cookie-based authentication"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Cookie"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        // Add Razor Pages support
        services.AddRazorPages()
            .AddRazorRuntimeCompilation();

        return services;
    }
}
