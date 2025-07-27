using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Services;

namespace TechWayFit.Licensing.Validation.Attributes
{
    /// <summary>
    /// Attribute to protect controller actions based on license features
    /// This ensures that only licensed features can be accessed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresFeatureAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _featureName;
        private readonly LicenseTier _minimumTier;
        private readonly bool _allowGracePeriod;

        /// <summary>
        /// Initializes the feature requirement attribute
        /// </summary>
        /// <param name="featureName">Name of the required feature</param>
        /// <param name="minimumTier">Minimum license tier required</param>
        /// <param name="allowGracePeriod">Whether to allow access during grace period</param>
        public RequiresFeatureAttribute(string featureName, LicenseTier minimumTier = LicenseTier.Community, bool allowGracePeriod = true)
        {
            _featureName = featureName;
            _minimumTier = minimumTier;
            _allowGracePeriod = allowGracePeriod;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var licenseValidator = context.HttpContext.RequestServices.GetRequiredService<ILicenseValidator>();
            var logger = context.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<RequiresFeatureAttribute>>();

            try
            {
                // Validate the current license
                var validation = await licenseValidator.ValidateFromConfigurationAsync();

                // Check if license is valid (including grace period if allowed)
                if (!validation.IsValid)
                {
                    if (!_allowGracePeriod || validation.Status != LicenseStatus.GracePeriod)
                    {
                        logger?.LogWarning("Feature {FeatureName} access denied: License invalid ({Status})", _featureName, validation.Status);
                        context.Result = CreateLicenseErrorResponse(validation.Status, validation.ValidationMessages.FirstOrDefault() ?? "License validation failed");
                        return;
                    }
                }

                // Check if the specific feature is available
                var isFeatureAvailable = await licenseValidator.IsFeatureAvailableAsync(_featureName);
                if (!isFeatureAvailable)
                {
                    logger?.LogWarning("Feature {FeatureName} access denied: Feature not available in current license", _featureName);
                    context.Result = CreateFeatureNotAvailableResponse(_featureName);
                    return;
                }

                // Check minimum tier requirement
                var supportsMinimumTier = await licenseValidator.SupportsLicenseTierAsync(_minimumTier);
                if (!supportsMinimumTier)
                {
                    logger?.LogWarning("Feature {FeatureName} access denied: License tier insufficient (requires {MinimumTier})", _featureName, _minimumTier);
                    context.Result = CreateInsufficientTierResponse(_featureName, _minimumTier);
                    return;
                }

                // Add license information to response headers for debugging
                if (validation.License != null)
                {
                    context.HttpContext.Response.Headers.Add("X-License-Tier", validation.License.Tier.ToString());
                    context.HttpContext.Response.Headers.Add("X-License-Status", validation.Status.ToString());
                    context.HttpContext.Response.Headers.Add("X-License-Expires", validation.License.ValidTo.ToString("yyyy-MM-dd"));
                    
                    if (validation.IsGracePeriod && validation.GracePeriodExpiry.HasValue)
                    {
                        context.HttpContext.Response.Headers.Add("X-Grace-Period-Expires", validation.GracePeriodExpiry.Value.ToString("yyyy-MM-dd"));
                    }
                }

                logger?.LogDebug("Feature {FeatureName} access granted for license tier {Tier}", _featureName, validation.License?.Tier);

                // Feature is available, proceed with action execution
                await next();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during feature validation for {FeatureName}", _featureName);
                context.Result = CreateServiceErrorResponse();
            }
        }

        private IActionResult CreateLicenseErrorResponse(LicenseStatus status, string message)
        {
            var errorCode = status switch
            {
                LicenseStatus.Expired => "LICENSE_EXPIRED",
                LicenseStatus.NotFound => "LICENSE_NOT_FOUND",
                LicenseStatus.Invalid => "LICENSE_INVALID",
                LicenseStatus.Corrupted => "LICENSE_CORRUPTED",
                LicenseStatus.NotYetValid => "LICENSE_NOT_YET_VALID",
                _ => "LICENSE_ERROR"
            };

            return new ObjectResult(new
            {
                error = errorCode,
                message = message,
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                feature = _featureName
            })
            {
                StatusCode = 403
            };
        }

        private IActionResult CreateFeatureNotAvailableResponse(string featureName)
        {
            return new ObjectResult(new
            {
                error = "FEATURE_NOT_AVAILABLE",
                message = $"The feature '{featureName}' is not available in your current license",
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                feature = featureName,
                action = "upgrade_license"
            })
            {
                StatusCode = 403
            };
        }

        private IActionResult CreateInsufficientTierResponse(string featureName, LicenseTier minimumTier)
        {
            return new ObjectResult(new
            {
                error = "INSUFFICIENT_LICENSE_TIER",
                message = $"The feature '{featureName}' requires a {minimumTier} license or higher",
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                feature = featureName,
                requiredTier = minimumTier.ToString(),
                action = "upgrade_license"
            })
            {
                StatusCode = 403
            };
        }

        private IActionResult CreateServiceErrorResponse()
        {
            return new ObjectResult(new
            {
                error = "LICENSE_SERVICE_ERROR",
                message = "License validation service is temporarily unavailable",
                statusCode = 503,
                timestamp = DateTime.UtcNow,
                feature = _featureName
            })
            {
                StatusCode = 503
            };
        }
    }

    /// <summary>
    /// Attribute to require a specific license tier for controller actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresLicenseTierAttribute : Attribute, IAsyncActionFilter
    {
        private readonly LicenseTier _requiredTier;
        private readonly bool _allowGracePeriod;

        public RequiresLicenseTierAttribute(LicenseTier requiredTier, bool allowGracePeriod = true)
        {
            _requiredTier = requiredTier;
            _allowGracePeriod = allowGracePeriod;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var licenseValidator = context.HttpContext.RequestServices.GetRequiredService<ILicenseValidator>();
            var logger = context.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<RequiresLicenseTierAttribute>>();

            try
            {
                // Validate the current license
                var validation = await licenseValidator.ValidateFromConfigurationAsync();

                // Check if license is valid
                if (!validation.IsValid)
                {
                    if (!_allowGracePeriod || validation.Status != LicenseStatus.GracePeriod)
                    {
                        logger?.LogWarning("Tier {RequiredTier} access denied: License invalid ({Status})", _requiredTier, validation.Status);
                        context.Result = CreateLicenseErrorResponse(validation.Status);
                        return;
                    }
                }

                // Check tier requirement
                var supportsRequiredTier = await licenseValidator.SupportsLicenseTierAsync(_requiredTier);
                if (!supportsRequiredTier)
                {
                    logger?.LogWarning("Tier {RequiredTier} access denied: Current tier {CurrentTier} insufficient", 
                        _requiredTier, validation.License?.Tier);
                    context.Result = CreateInsufficientTierResponse();
                    return;
                }

                logger?.LogDebug("Tier {RequiredTier} access granted", _requiredTier);
                await next();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during tier validation for {RequiredTier}", _requiredTier);
                context.Result = CreateServiceErrorResponse();
            }
        }

        private IActionResult CreateLicenseErrorResponse(LicenseStatus status)
        {
            return new ObjectResult(new
            {
                error = "LICENSE_ERROR",
                message = $"License validation failed: {status}",
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                requiredTier = _requiredTier.ToString()
            })
            {
                StatusCode = 403
            };
        }

        private IActionResult CreateInsufficientTierResponse()
        {
            return new ObjectResult(new
            {
                error = "INSUFFICIENT_LICENSE_TIER",
                message = $"This operation requires a {_requiredTier} license or higher",
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                requiredTier = _requiredTier.ToString(),
                action = "upgrade_license"
            })
            {
                StatusCode = 403
            };
        }

        private IActionResult CreateServiceErrorResponse()
        {
            return new ObjectResult(new
            {
                error = "LICENSE_SERVICE_ERROR",
                message = "License validation service is temporarily unavailable",
                statusCode = 503,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = 503
            };
        }
    }
}
