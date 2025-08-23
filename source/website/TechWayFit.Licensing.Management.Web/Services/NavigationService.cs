using TechWayFit.Licensing.Management.Web.ViewModels.Shared;

namespace TechWayFit.Licensing.Management.Web.Services
{
    public interface INavigationService
    {
        List<NavigationItem> GetNavigationItems();
        List<NavigationItem> GetNavigationItemsForUser(string? userId = null);
    }

    public class NavigationService : INavigationService
    {
        // You can inject IConfiguration, database context, or other services here
        // for dynamic navigation based on user roles, permissions, or configuration

        public List<NavigationItem> GetNavigationItems()
        {
            return GetNavigationItemsForUser();
        }

        public List<NavigationItem> GetNavigationItemsForUser(string? userId = null)
        {
            // You can add logic here to filter navigation based on user permissions
            // For now, return all navigation items
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    Id = "dashboard",
                    Title = "Dashboard",
                    Icon = "fas fa-home",
                    Controller = "Home",
                    Action = "Index",
                    Order = 1
                },
                new NavigationItem
                {
                    Id = "products",
                    Title = "Products",
                    Icon = "fas fa-box",
                    Controller = "Product",
                    Action = "Index",
                    Order = 2
                },
                new NavigationItem
                {
                    Id = "licenses",
                    Title = "Licenses",
                    Icon = "fas fa-key",
                    Controller = "License",
                    Action = "Index",
                    Order = 3
                },
                new NavigationItem
                {
                    Id = "consumers",
                    Title = "Consumers",
                    Icon = "fas fa-users",
                    Controller = "Consumer",
                    Action = "Index",
                    Order = 4
                },
                new NavigationItem
                {
                    Id = "notifications",
                    Title = "Notifications",
                    Icon = "fas fa-bell",
                    Controller = "Notification",
                    Action = "Index",
                    Order = 5
                },
                new NavigationItem
                {
                    Id = "audit",
                    Title = "Audit Logs",
                    Icon = "fas fa-clipboard-list",
                    Controller = "Audit",
                    Action = "Index",
                    Order = 6
                },
                new NavigationItem
                {
                    Id = "user-management",
                    Title = "User Management",
                    Icon = "fas fa-user-cog",
                    IsSubmenu = true,
                    SubmenuId = "userManagementSubmenu",
                    ActiveControllers = new[] { "User", "Role", "Tenant" },
                    Order = 7,
                    RequiresPermission = true,
                    Permission = "UserManagement.View",
                    Children = new List<NavigationItem>
                    {
                        new NavigationItem
                        {
                            Id = "users",
                            Title = "Users",
                            Icon = "fas fa-users",
                            Controller = "User",
                            Action = "Index",
                            RequiresPermission = true,
                            Permission = "Users.View"
                        },
                        new NavigationItem
                        {
                            Id = "roles",
                            Title = "Roles",
                            Icon = "fas fa-user-tag",
                            Controller = "Role",
                            Action = "Index",
                            RequiresPermission = true,
                            Permission = "Roles.View"
                        },
                        new NavigationItem
                        {
                            Id = "tenants",
                            Title = "Tenants",
                            Icon = "fas fa-building",
                            Controller = "Tenant",
                            Action = "Index",
                            RequiresPermission = true,
                            Permission = "Tenants.View"
                        }
                    }
                },
                new NavigationItem
                {
                    Id = "system-admin",
                    Title = "System Administration",
                    Icon = "fas fa-cogs",
                    IsSubmenu = true,
                    SubmenuId = "systemAdminSubmenu",
                    ActiveControllers = new[] { "System", "HangfireDashboard" },
                    Order = 8,
                    RequiresPermission = true,
                    Permission = "System.Admin",
                    Children = new List<NavigationItem>
                    {
                        new NavigationItem
                        {
                            Id = "system-dashboard",
                            Title = "Dashboard",
                            Icon = "fas fa-tachometer-alt",
                            Controller = "System",
                            Action = "Index"
                        },
                        new NavigationItem
                        {
                            Id = "job-dashboard",
                            Title = "Job Dashboard",
                            Icon = "fas fa-tasks",
                            Controller = "HangfireDashboard",
                            Action = "Index"
                        },
                        new NavigationItem
                        {
                            Id = "scheduled-jobs",
                            Title = "Scheduled Jobs",
                            Icon = "fas fa-clock",
                            Controller = "System",
                            Action = "Jobs"
                        },
                        new NavigationItem
                        {
                            Id = "embedded-dashboard",
                            Title = "Embedded Dashboard",
                            Icon = "fas fa-desktop",
                            Controller = "HangfireDashboard",
                            Action = "Embedded"
                        },
                        new NavigationItem
                        {
                            Id = "full-dashboard",
                            Title = "Full Dashboard",
                            Icon = "fas fa-external-link-alt",
                            Url = "/hangfire",
                            IsExternal = true
                        }
                    }
                },
                new NavigationItem
                {
                    Id = "settings",
                    Title = "Settings",
                    Icon = "fas fa-cog",
                    Controller = "Settings",
                    Action = "Index",
                    Order = 9
                }
            };
        }
    }
}
