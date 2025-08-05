using Microsoft.AspNetCore.Mvc;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    public class BaseController : Controller
    {
        protected string CurrentUserName => User.Identity?.Name ?? "Anonymous";

    }
}
