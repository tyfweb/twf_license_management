# Views Folder Structure

This document describes the organized folder structure for views and shared components.

## Folder Organization

### `/Views/Shared/`
The main shared folder has been organized into three subfolders:

#### `/Views/Shared/Layout/`
Contains all layout files:
- `_Layout.cshtml` - Default ASP.NET Core layout
- `_LicenseHubLayout.cshtml` - Main application layout

#### `/Views/Shared/Partial/`
Contains all partial views (reusable UI components):
- `_SiteMessages.cshtml` - Application messages display
- `_TopBar.cshtml` - Top navigation bar
- `_Sidebar.cshtml` - Legacy sidebar (replaced by ViewComponent)
- `_ValidationScriptsPartial.cshtml` - Form validation scripts
- `_PageHeaderPartial.cshtml` - Page header component
- `_StatsCardsPartial.cshtml` - Statistics cards component
- `_StatsTiles.cshtml` - Statistics tiles component
- `_ProductKeyInfo.cshtml` - Product key information display
- `_ProductKeyInfoCompact.cshtml` - Compact product key display

#### `/Views/Shared/Components/`
Contains ViewComponents and their views:
- `SidebarNavigation/` - Sidebar navigation ViewComponent
- `TenantSelector/` - Tenant selection ViewComponent
- `PendingApprovals/` - Pending approvals ViewComponent
- `ThemeLoader/` - Theme loading ViewComponent
- `CompiledCss/` - CSS compilation ViewComponent

## View Engine Configuration

The ASP.NET Core view engine has been configured to automatically search in the new folder structure:

```csharp
options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
options.ViewLocationFormats.Add("/Views/Shared/Partial/{0}.cshtml");  // NEW
options.ViewLocationFormats.Add("/Views/Shared/Layout/{0}.cshtml");   // NEW
```

## Usage

### Layouts
Reference layouts from anywhere in the application:
```csharp
Layout = "~/Views/Shared/Layout/_LicenseHubLayout.cshtml";
```

### Partial Views
Reference partial views normally (view engine will find them automatically):
```razor
@await Html.PartialAsync("_SiteMessages")
@await Html.PartialAsync("_TopBar")
@await Html.PartialAsync("_ValidationScriptsPartial")
```

### ViewComponents
Reference ViewComponents normally:
```razor
@await Component.InvokeAsync("SidebarNavigation")
@await Component.InvokeAsync("TenantSelector")
```

## Benefits

1. **Better Organization**: Clear separation between layouts, partials, and components
2. **Easier Maintenance**: Related files are grouped together
3. **Improved Discoverability**: Developers can quickly find the right type of view
4. **Scalability**: Structure supports growth without clutter
5. **Consistency**: Follows established patterns for large applications

## Migration Notes

- All existing references continue to work due to view engine configuration
- No code changes required in existing views
- New views should follow the organized structure
- Legacy `_Sidebar.cshtml` can be removed after full migration to SidebarNavigation ViewComponent
