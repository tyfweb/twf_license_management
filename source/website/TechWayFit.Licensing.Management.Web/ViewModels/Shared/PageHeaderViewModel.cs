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
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public bool IsActive { get; set; }
    }
}
