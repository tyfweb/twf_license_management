using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    public class BaseController : Controller
    {
        protected string CurrentUserName => User.Identity?.Name ?? "Anonymous";

        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   User.FindFirst("sub")?.Value ?? 
                   User.Identity?.Name ?? 
                   "Anonymous";
        }
    }
}
