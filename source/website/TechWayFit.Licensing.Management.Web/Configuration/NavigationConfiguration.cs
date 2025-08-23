namespace TechWayFit.Licensing.Management.Web.Configuration
{
    public class NavigationConfiguration
    {
        public const string SectionName = "Navigation";
        
        public List<NavigationItemConfig> Items { get; set; } = new();
        public bool EnablePermissionFiltering { get; set; } = true;
        public bool EnableRoleBasedNavigation { get; set; } = true;
    }

    public class NavigationItemConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Url { get; set; }
        public bool IsExternal { get; set; }
        public bool IsSubmenu { get; set; }
        public string? SubmenuId { get; set; }
        public string[]? ActiveControllers { get; set; }
        public int Order { get; set; }
        public List<NavigationItemConfig> Children { get; set; } = new();
        public bool RequiresPermission { get; set; }
        public string? Permission { get; set; }
        public string[]? RequiredRoles { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
    }
}
