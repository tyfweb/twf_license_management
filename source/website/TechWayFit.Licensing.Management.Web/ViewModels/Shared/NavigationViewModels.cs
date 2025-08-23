namespace TechWayFit.Licensing.Management.Web.ViewModels.Shared
{
    public class SidebarNavigationViewModel
    {
        public string? CurrentController { get; set; }
        public string? CurrentAction { get; set; }
        public List<NavigationItem> NavigationItems { get; set; } = new();
        
        public bool IsControllerActive(params string[] controllers)
        {
            return controllers.Any(c => string.Equals(c, CurrentController, StringComparison.OrdinalIgnoreCase));
        }
        
        public bool IsActionActive(string action)
        {
            return string.Equals(action, CurrentAction, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool IsControllerAndActionActive(string controller, string action)
        {
            return IsControllerActive(controller) && IsActionActive(action);
        }

        public bool IsNavigationItemActive(NavigationItem item)
        {
            if (item.IsSubmenu && item.ActiveControllers?.Any() == true)
            {
                return IsControllerActive(item.ActiveControllers);
            }

            if (!string.IsNullOrEmpty(item.Controller))
            {
                if (!string.IsNullOrEmpty(item.Action))
                {
                    return IsControllerAndActionActive(item.Controller, item.Action);
                }
                return IsControllerActive(item.Controller);
            }

            return false;
        }
    }

    public class NavigationItem
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
        public List<NavigationItem> Children { get; set; } = new();
        public bool RequiresPermission { get; set; }
        public string? Permission { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
