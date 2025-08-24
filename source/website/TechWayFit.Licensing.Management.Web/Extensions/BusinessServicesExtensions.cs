using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Services;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Services.Implementations;
using TechWayFit.Licensing.Management.Services.Implementations.Account;
using TechWayFit.Licensing.Management.Services.Implementations.Audit;
using TechWayFit.Licensing.Management.Services.Implementations.Consumer;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.Management.Services.Implementations.Product;
using TechWayFit.Licensing.Management.Services.Implementations.Tenant;
using TechWayFit.Licensing.Management.Services.Implementations.User;
using TechWayFit.Licensing.Management.Services.Implementations.Workflow;
using TechWayFit.Licensing.Management.Web.Services;
using TechWayFit.Licensing.Management.Web.Services.Jobs;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for registering business services
/// </summary>
public static class BusinessServicesExtensions
{
    /// <summary>
    /// Registers all business services for the licensing management system
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The configured service collection</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Product management services
        services.AddScoped<IEnterpriseProductService, EnterpriseProductService>();
        services.AddScoped<IProductTierService, ProductTierService>();

        // Consumer management services
        services.AddScoped<IConsumerAccountService, ConsumerAccountService>();

        // License management services
        services.AddScoped<ILicenseGenerator, StatelessLicenseGenerator>();
        services.AddScoped<IKeyManagementService, DatabaseKeyManagementService>();
        services.AddScoped<IProductLicenseService, ProductLicenseService>();
        services.AddScoped<IProductFeatureService, ProductFeatureService>();
        services.AddScoped<IProductFeatureTierMappingService, ProductFeatureTierMappingService>();
        services.AddScoped<IProductActivationService, ProductActivationService>();

        // License generation factory and strategies
        services.AddScoped<TechWayFit.Licensing.Management.Services.Factories.ILicenseGenerationFactory, 
                          TechWayFit.Licensing.Management.Services.Factories.LicenseGenerationFactory>();
        
        services.AddScoped<TechWayFit.Licensing.Management.Services.Strategies.ProductLicenseFileStrategy>();
        services.AddScoped<TechWayFit.Licensing.Management.Services.Strategies.ProductKeyStrategy>();
        services.AddScoped<TechWayFit.Licensing.Management.Services.Strategies.VolumetricLicenseStrategy>();

        // License file and activation services
        services.AddScoped<ILicenseFileService, LicenseFileService>();
        services.AddScoped<ILicenseActivationService, LicenseActivationService>();

        // Legacy validation service from TechWayFit.Licensing.Core
        services.AddSingleton<TechWayFit.Licensing.Core.Contracts.ILicenseValidationService, 
                             TechWayFit.Licensing.Core.Services.LicenseValidationService>();

        // Settings and navigation services
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<INavigationService, NavigationService>();

        // Audit management services
        services.AddScoped<IAuditService, AuditService>();

        // Notification services
        services.AddScoped<INotificationService, NotificationService>();

        // User management services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPermissionService, TechWayFit.Licensing.Management.Services.User.PermissionService>();

        // Tenant management services
        services.AddScoped<ITenantService, TenantService>();

        // Authentication service
        services.AddScoped<IAuthenticationService, AccountService>();

        // Workflow services for approval system
        services.AddScoped(typeof(IWorkflowService<>), typeof(WorkflowService<>));
        services.AddScoped<IConsumerAccountWorkflowService, ConsumerAccountWorkflowService>();
        services.AddScoped<IEnterpriseProductWorkflowService, EnterpriseProductWorkflowService>();
        services.AddScoped<IProductLicenseWorkflowService, ProductLicenseWorkflowService>();

        // Hangfire job services
        services.AddScoped<LicenseJobService>();
        services.AddScoped<AuditJobService>();
        services.AddScoped<SystemMaintenanceJobService>();

        return services;
    }
}
