using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace TechWayFit.Licensing.Management.Web.ViewComponents
{
    /// <summary>
    /// View Component for dynamically selecting CSS files based on environment
    /// </summary>
    public class CompiledCssViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment _environment;

        public CompiledCssViewComponent(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Returns the appropriate CSS file based on environment
        /// </summary>
        /// <param name="fallback">Fallback CSS file if compiled CSS doesn't exist</param>
        /// <returns>CSS file path</returns>
        public IViewComponentResult Invoke(string fallback = "~/css/site.css")
        {
            string cssFile;
            
            if (_environment.IsDevelopment())
            {
                // Development: Use expanded CSS with source maps
                cssFile = "~/css/compiled.css";
            }
            else
            {
                // Production: Use minified CSS for better performance
                var minifiedPath = Path.Combine(_environment.WebRootPath, "css", "compiled.min.css");
                var regularPath = Path.Combine(_environment.WebRootPath, "css", "compiled.css");
                
                if (File.Exists(minifiedPath))
                {
                    cssFile = "~/css/compiled.min.css";
                }
                else if (File.Exists(regularPath))
                {
                    cssFile = "~/css/compiled.css";
                }
                else
                {
                    // Fallback to original CSS if compiled versions don't exist
                    cssFile = fallback;
                }
            }

            ViewBag.CssFile = cssFile;
            ViewBag.Environment = _environment.EnvironmentName;
            ViewBag.IsMinified = cssFile.Contains(".min.");
            
            return View(new { CssFile = cssFile });
        }
    }
}
