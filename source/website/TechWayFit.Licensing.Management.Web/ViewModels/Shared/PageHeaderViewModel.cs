namespace TechWayFit.Licensing.Management.Web.ViewModels.Shared
{
    public class PageHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public List<BreadcrumbItem> BreadcrumbItems { get; set; } = new();
        public string? BackButtonText { get; set; }
        public string? BackButtonController { get; set; }
        public string? BackButtonAction { get; set; }
        public object? BackButtonRouteValues { get; set; }
        public List<ActionButtonItem> ActionButtons { get; set; } = new();
        public ActionButtonItem? PrimaryAction { get; set; }
        public List<ActionButtonItem> DropdownActions { get; set; } = new();
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public object? RouteValues { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActionButtonItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public object? RouteValues { get; set; }
        public string? Url { get; set; }
        public string ButtonClass { get; set; } = "btn btn-outline-primary";
        public string? OnClick { get; set; }
        public string? TargetId { get; set; }
    }
}
