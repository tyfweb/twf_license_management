using TechWayFit.Licensing.Management.Web.ViewModels.Shared;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for creating page headers
/// </summary>
public static class PageHeaderExtensions
{
    /// <summary>
    /// Creates a basic page header
    /// </summary>
    public static PageHeaderViewModel CreatePageHeader(
        string title,
        string? icon = null,
        string? backButtonText = null,
        string? backButtonController = null,
        string? backButtonAction = null,
        object? backButtonRouteValues = null)
    {
        return new PageHeaderViewModel
        {
            Title = title,
            Icon = icon,
            BackButtonText = backButtonText,
            BackButtonController = backButtonController,
            BackButtonAction = backButtonAction,
            BackButtonRouteValues = backButtonRouteValues
        };
    }

    /// <summary>
    /// Adds a breadcrumb item to the page header
    /// </summary>
    public static PageHeaderViewModel AddBreadcrumb(
        this PageHeaderViewModel header,
        string text,
        string? controller = null,
        string? action = null,
        bool isActive = false)
    {
        header.BreadcrumbItems.Add(new BreadcrumbItem
        {
            Text = text,
            Controller = controller,
            Action = action,
            IsActive = isActive
        });
        return header;
    }

    /// <summary>
    /// Adds an action button to the page header
    /// </summary>
    public static PageHeaderViewModel AddActionButton(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string buttonClass = "btn btn-outline-primary",
        string? onClick = null)
    {
        header.ActionButtons.Add(new ActionButtonItem
        {
            Text = text,
            Icon = icon,
            Controller = controller,
            Action = action,
            RouteValues = routeValues,
            Url = url,
            ButtonClass = buttonClass,
            OnClick = onClick
        });
        return header;
    }

    /// <summary>
    /// Adds a primary action button to the page header
    /// </summary>
    public static PageHeaderViewModel AddPrimaryButton(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string? onClick = null)
    {
        return header.AddActionButton(text, icon, controller, action, routeValues, url, "btn btn-primary", onClick);
    }

    /// <summary>
    /// Adds a success action button to the page header
    /// </summary>
    public static PageHeaderViewModel AddSuccessButton(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string? onClick = null)
    {
        return header.AddActionButton(text, icon, controller, action, routeValues, url, "btn btn-success", onClick);
    }

    /// <summary>
    /// Adds a warning action button to the page header
    /// </summary>
    public static PageHeaderViewModel AddWarningButton(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string? onClick = null)
    {
        return header.AddActionButton(text, icon, controller, action, routeValues, url, "btn btn-outline-warning", onClick);
    }

    /// <summary>
    /// Adds a danger action button to the page header
    /// </summary>
    public static PageHeaderViewModel AddDangerButton(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string? onClick = null)
    {
        return header.AddActionButton(text, icon, controller, action, routeValues, url, "btn btn-danger", onClick);
    }

    /// <summary>
    /// Sets a primary action for the header (will be displayed prominently)
    /// </summary>
    public static PageHeaderViewModel SetPrimaryAction(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string buttonClass = "btn btn-success",
        string? onClick = null)
    {
        header.PrimaryAction = new ActionButtonItem
        {
            Text = text,
            Icon = icon,
            Controller = controller,
            Action = action,
            RouteValues = routeValues,
            Url = url,
            ButtonClass = buttonClass,
            OnClick = onClick
        };
        return header;
    }

    /// <summary>
    /// Adds an action to the "More Actions" dropdown
    /// </summary>
    public static PageHeaderViewModel AddDropdownAction(
        this PageHeaderViewModel header,
        string text,
        string? icon = null,
        string? controller = null,
        string? action = null,
        object? routeValues = null,
        string? url = null,
        string? onClick = null)
    {
        header.DropdownActions.Add(new ActionButtonItem
        {
            Text = text,
            Icon = icon,
            Controller = controller,
            Action = action,
            RouteValues = routeValues,
            Url = url,
            OnClick = onClick
        });
        return header;
    }
}
