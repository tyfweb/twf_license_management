using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Shared;

namespace TechWayFit.Licensing.Management.Web.ViewComponents
{
    public class SidebarNavigationViewComponent : ViewComponent
    {
        private readonly INavigationService _navigationService;

        public SidebarNavigationViewComponent(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public IViewComponentResult Invoke()
        {
            var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

            var model = new SidebarNavigationViewModel
            {
                CurrentController = currentController,
                CurrentAction = currentAction,
                NavigationItems = _navigationService.GetNavigationItems()
            };

            return View(model);
        }
    }
}
