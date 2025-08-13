using Hangfire.Dashboard;

namespace TechWayFit.Licensing.Management.Web.Services.Jobs;

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// </summary>
public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // In development, allow all access
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }
        
        // In production, check if user is authenticated and has admin role
        return httpContext.User.Identity?.IsAuthenticated == true && 
               httpContext.User.IsInRole("Administrator");
    }
}
