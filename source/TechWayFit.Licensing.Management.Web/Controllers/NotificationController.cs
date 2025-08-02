using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Web.ViewModels.Notification;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Notification Management Controller
    /// Handles notification history, templates, and delivery management
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Display notification dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var statistics = await _notificationService.GetNotificationStatisticsAsync();
                var recentNotifications = await _notificationService.GetConsumerNotificationHistoryAsync(
                    "", // All consumers
                    DateTime.UtcNow.AddDays(-30),
                    null,
                    1,
                    10);

                var templates = await _notificationService.GetNotificationTemplatesAsync();
                var popularTemplates = templates
                    .OrderByDescending(t => t.TemplateVariables.Count)
                    .Take(5)
                    .Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive,
                        CreatedBy = t.CreatedBy,
                        CreatedDate = t.CreatedDate,
                        UsageCount = 0, // TODO: Implement usage tracking
                        LastUsed = null
                    }).ToList();

                var viewModel = new NotificationDashboardViewModel
                {
                    Statistics = new NotificationStatisticsViewModel
                    {
                        TotalNotifications = statistics.TotalNotifications,
                        SuccessfulDeliveries = statistics.SuccessfulDeliveries,
                        FailedDeliveries = statistics.FailedDeliveries,
                        PendingDeliveries = statistics.TotalNotifications - statistics.SuccessfulDeliveries - statistics.FailedDeliveries,
                        DeliverySuccessRate = statistics.DeliverySuccessRate,
                        NotificationsByType = statistics.NotificationsByType,
                        NotificationsByDate = statistics.NotificationsByDate
                    },
                    RecentNotifications = recentNotifications.Select(n => new NotificationItemViewModel
                    {
                        NotificationId = n.NotificationId,
                        EntityId = n.EntityId,
                        EntityType = n.EntityType,
                        NotificationMode = n.NotificationMode,
                        NotificationTemplateId = n.NotificationTemplateId,
                        NotificationType = n.NotificationType,
                        Recipients = n.Recipients,
                        SentDate = n.SentDate,
                        DeliveryStatus = n.DeliveryStatus,
                        DeliveryError = n.DeliveryError,
                        RecipientDisplay = GetRecipientDisplay(n.Recipients)
                    }).ToList(),
                    PopularTemplates = popularTemplates,
                    Trends = new NotificationTrendsViewModel
                    {
                        DeliveryTrends = statistics.NotificationsByDate,
                        TypeTrends = statistics.NotificationsByType,
                        TotalGrowthPercentage = 0, // TODO: Calculate trends
                        TrendDirection = "stable"
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification dashboard");
                return View(new NotificationDashboardViewModel());
            }
        }

        /// <summary>
        /// Display notification history with filtering and pagination
        /// </summary>
        public async Task<IActionResult> History(
            string searchTerm = "",
            NotificationType? notificationType = null,
            NotificationMode? notificationMode = null,
            DeliveryStatus? deliveryStatus = null,
            DateTime? sentFromDate = null,
            DateTime? sentToDate = null,
            string entityType = "",
            string entityId = "",
            int page = 1,
            int pageSize = 25)
        {
            try
            {
                // Get filtered notifications - using consumer history as base
                var notifications = await _notificationService.GetConsumerNotificationHistoryAsync(
                    entityId,
                    sentFromDate,
                    sentToDate,
                    page,
                    pageSize);

                // TODO: Apply additional filters (this would need to be implemented in the service)
                var filteredNotifications = notifications.AsEnumerable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredNotifications = filteredNotifications.Where(n =>
                        n.EntityId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        n.EntityType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (notificationType.HasValue)
                {
                    filteredNotifications = filteredNotifications.Where(n => n.NotificationType == notificationType.Value);
                }

                if (notificationMode.HasValue)
                {
                    filteredNotifications = filteredNotifications.Where(n => n.NotificationMode == notificationMode.Value);
                }

                if (deliveryStatus.HasValue)
                {
                    filteredNotifications = filteredNotifications.Where(n => n.DeliveryStatus == deliveryStatus.Value);
                }

                if (!string.IsNullOrEmpty(entityType))
                {
                    filteredNotifications = filteredNotifications.Where(n => n.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
                }

                var notificationList = filteredNotifications.ToList();
                var totalCount = notificationList.Count; // TODO: Get actual total from service

                var statistics = await _notificationService.GetNotificationStatisticsAsync(sentFromDate, sentToDate);

                var viewModel = new NotificationListViewModel
                {
                    Notifications = notificationList.Select(n => new NotificationItemViewModel
                    {
                        NotificationId = n.NotificationId,
                        EntityId = n.EntityId,
                        EntityType = n.EntityType,
                        NotificationMode = n.NotificationMode,
                        NotificationTemplateId = n.NotificationTemplateId,
                        NotificationType = n.NotificationType,
                        Recipients = n.Recipients,
                        SentDate = n.SentDate,
                        DeliveryStatus = n.DeliveryStatus,
                        DeliveryError = n.DeliveryError,
                        TemplateName = "Unknown", // TODO: Get template name
                        Subject = "N/A", // TODO: Get subject from template
                        RecipientDisplay = GetRecipientDisplay(n.Recipients)
                    }).ToList(),
                    Filter = new NotificationFilterViewModel
                    {
                        SearchTerm = searchTerm,
                        NotificationType = notificationType,
                        NotificationMode = notificationMode,
                        DeliveryStatus = deliveryStatus,
                        SentFromDate = sentFromDate,
                        SentToDate = sentToDate,
                        EntityType = entityType,
                        EntityId = entityId
                    },
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    },
                    Statistics = new NotificationStatisticsViewModel
                    {
                        TotalNotifications = statistics.TotalNotifications,
                        SuccessfulDeliveries = statistics.SuccessfulDeliveries,
                        FailedDeliveries = statistics.FailedDeliveries,
                        PendingDeliveries = statistics.TotalNotifications - statistics.SuccessfulDeliveries - statistics.FailedDeliveries,
                        DeliverySuccessRate = statistics.DeliverySuccessRate
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification history");
                return View(new NotificationListViewModel());
            }
        }

        /// <summary>
        /// Display notification templates
        /// </summary>
        public async Task<IActionResult> Templates(
            string searchTerm = "",
            NotificationType? filterType = null,
            bool showInactive = false)
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync(filterType);
                var filteredTemplates = templates.AsEnumerable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredTemplates = filteredTemplates.Where(t =>
                        t.TemplateName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Subject.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (!showInactive)
                {
                    filteredTemplates = filteredTemplates.Where(t => t.IsActive);
                }

                var viewModel = new NotificationTemplateListViewModel
                {
                    Templates = filteredTemplates.Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive,
                        CreatedBy = t.CreatedBy,
                        CreatedDate = t.CreatedDate,
                        UsageCount = 0, // TODO: Implement usage tracking
                        LastUsed = null
                    }).ToList(),
                    SearchTerm = searchTerm,
                    FilterType = filterType,
                    ShowInactiveTemplates = showInactive
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification templates");
                return View(new NotificationTemplateListViewModel());
            }
        }

        /// <summary>
        /// Display template details
        /// </summary>
        public async Task<IActionResult> TemplateDetails(string id)
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                var template = templates.FirstOrDefault(t => t.TemplateId == id);
                
                if (template == null)
                {
                    return NotFound();
                }

                // Get recent usage of this template
                var recentUsage = await _notificationService.GetConsumerNotificationHistoryAsync(
                    "", // All consumers
                    DateTime.UtcNow.AddDays(-30),
                    null,
                    1,
                    10);

                var templateUsage = recentUsage
                    .Where(n => n.NotificationTemplateId == id)
                    .Select(n => new NotificationItemViewModel
                    {
                        NotificationId = n.NotificationId,
                        EntityId = n.EntityId,
                        EntityType = n.EntityType,
                        NotificationMode = n.NotificationMode,
                        NotificationTemplateId = n.NotificationTemplateId,
                        NotificationType = n.NotificationType,
                        Recipients = n.Recipients,
                        SentDate = n.SentDate,
                        DeliveryStatus = n.DeliveryStatus,
                        DeliveryError = n.DeliveryError,
                        RecipientDisplay = GetRecipientDisplay(n.Recipients)
                    }).ToList();

                var viewModel = new NotificationTemplateDetailViewModel
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    NotificationType = template.NotificationType,
                    Preferences = template.Preferences,
                    Subject = template.Subject,
                    MessageTemplate = template.MessageTemplate,
                    IsActive = template.IsActive,
                    CreatedBy = template.CreatedBy,
                    CreatedDate = template.CreatedDate,
                    TemplateVariables = template.TemplateVariables,
                    RecentUsage = templateUsage,
                    Statistics = new NotificationTemplateStatisticsViewModel
                    {
                        TotalUsage = templateUsage.Count,
                        SuccessfulDeliveries = templateUsage.Count(u => u.DeliveryStatus == DeliveryStatus.Sent),
                        FailedDeliveries = templateUsage.Count(u => u.DeliveryStatus == DeliveryStatus.Failed),
                        LastUsed = templateUsage.OrderByDescending(u => u.SentDate).FirstOrDefault()?.SentDate,
                        SuccessRate = templateUsage.Count > 0 
                            ? (double)templateUsage.Count(u => u.DeliveryStatus == DeliveryStatus.Sent) / templateUsage.Count * 100 
                            : 0
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading template details for {TemplateId}", id);
                return NotFound();
            }
        }

        /// <summary>
        /// Create new notification template
        /// </summary>
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            var viewModel = new NotificationTemplateEditViewModel
            {
                AvailableVariables = GetAvailableVariables()
            };

            return View("EditTemplate", viewModel);
        }

        /// <summary>
        /// Edit existing notification template
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditTemplate(string id)
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                var template = templates.FirstOrDefault(t => t.TemplateId == id);

                if (template == null)
                {
                    return NotFound();
                }

                var viewModel = new NotificationTemplateEditViewModel
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    NotificationType = template.NotificationType,
                    NotificationMode = template.Preferences.Mode,
                    Subject = template.Subject,
                    MessageTemplate = template.MessageTemplate,
                    IsActive = template.IsActive,
                    TemplateVariables = template.TemplateVariables.ToDictionary(
                        kvp => kvp.Key, 
                        kvp => kvp.Value?.ToString() ?? ""),
                    AvailableVariables = GetAvailableVariables()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading template for editing: {TemplateId}", id);
                return NotFound();
            }
        }

        /// <summary>
        /// Save notification template
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTemplate(NotificationTemplateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableVariables = GetAvailableVariables();
                return View("EditTemplate", model);
            }

            try
            {
                var template = new NotificationTemplate
                {
                    TemplateId = string.IsNullOrEmpty(model.TemplateId) ? Guid.NewGuid().ToString() : model.TemplateId,
                    TemplateName = model.TemplateName,
                    NotificationType = model.NotificationType,
                    Preferences = new NotificationPreferences { Mode = model.NotificationMode },
                    Subject = model.Subject,
                    MessageTemplate = model.MessageTemplate,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name ?? "system",
                    CreatedDate = DateTime.UtcNow,
                    TemplateVariables = model.TemplateVariables.ToDictionary(
                        kvp => kvp.Key, 
                        kvp => (object)kvp.Value)
                };

                await _notificationService.SaveNotificationTemplateAsync(template, User.Identity?.Name ?? "system");

                TempData["SuccessMessage"] = "Template saved successfully.";
                return RedirectToAction(nameof(Templates));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification template");
                ModelState.AddModelError("", "Failed to save template. Please try again.");
                model.AvailableVariables = GetAvailableVariables();
                return View("EditTemplate", model);
            }
        }

        /// <summary>
        /// Send custom notification
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Send()
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                var activeTemplates = templates.Where(t => t.IsActive)
                    .Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive
                    }).ToList();

                var viewModel = new SendNotificationViewModel
                {
                    AvailableTemplates = activeTemplates,
                    SuggestedRecipients = GetSuggestedRecipients()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading send notification page");
                return View(new SendNotificationViewModel());
            }
        }

        /// <summary>
        /// Send custom notification
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendNotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSendNotificationViewModel(model);
                return View(model);
            }

            try
            {
                var metadata = model.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                
                var success = await _notificationService.SendCustomNotificationAsync(
                    model.Recipients,
                    model.Subject,
                    model.Message,
                    model.NotificationType,
                    metadata);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Notification sent successfully to {model.Recipients.Count} recipient(s).";
                    return RedirectToAction(nameof(History));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to send notification. Please check the logs for details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending custom notification");
                ModelState.AddModelError("", "An error occurred while sending the notification.");
            }

            await PopulateSendNotificationViewModel(model);
            return View(model);
        }

        /// <summary>
        /// Test notification template
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                var activeTemplates = templates.Where(t => t.IsActive)
                    .Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive
                    }).ToList();

                var viewModel = new NotificationTestViewModel
                {
                    AvailableTemplates = activeTemplates,
                    TestRecipient = User.Identity?.Name ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test notification page");
                return View(new NotificationTestViewModel());
            }
        }

        /// <summary>
        /// Send test notification
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(NotificationTestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTestNotificationViewModel(model);
                return View(model);
            }

            try
            {
                // For testing, we'll send a custom notification with test data
                var testSubject = $"[TEST] Notification Template Test - {DateTime.Now:yyyy-MM-dd HH:mm}";
                var testMessage = $"This is a test notification sent at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.\n\nTemplate Data: {string.Join(", ", model.TestData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}";

                var success = await _notificationService.SendCustomNotificationAsync(
                    new[] { model.TestRecipient },
                    testSubject,
                    testMessage,
                    NotificationType.Custom,
                    model.TestData.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));

                model.TestSent = true;
                model.TestResult = success 
                    ? "Test notification sent successfully!" 
                    : "Failed to send test notification. Check the logs for details.";

                TempData[success ? "SuccessMessage" : "ErrorMessage"] = model.TestResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                model.TestResult = "An error occurred while sending the test notification.";
                TempData["ErrorMessage"] = model.TestResult;
            }

            await PopulateTestNotificationViewModel(model);
            return View(model);
        }

        /// <summary>
        /// Get notification details via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotificationDetails(string id)
        {
            try
            {
                // TODO: Implement GetNotificationById in service
                var history = await _notificationService.GetConsumerNotificationHistoryAsync("", null, null, 1, 1000);
                var notification = history.FirstOrDefault(n => n.NotificationId == id);

                if (notification == null)
                {
                    return NotFound();
                }

                var viewModel = new NotificationItemViewModel
                {
                    NotificationId = notification.NotificationId,
                    EntityId = notification.EntityId,
                    EntityType = notification.EntityType,
                    NotificationMode = notification.NotificationMode,
                    NotificationTemplateId = notification.NotificationTemplateId,
                    NotificationType = notification.NotificationType,
                    Recipients = notification.Recipients,
                    SentDate = notification.SentDate,
                    DeliveryStatus = notification.DeliveryStatus,
                    DeliveryError = notification.DeliveryError,
                    RecipientDisplay = GetRecipientDisplay(notification.Recipients)
                };

                return PartialView("_NotificationDetails", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification details for {NotificationId}", id);
                return NotFound();
            }
        }

        #region Private Helper Methods

        private static string GetRecipientDisplay(NotificationPreferences recipients)
        {
            return recipients.Mode switch
            {
                NotificationMode.Email => "Email",
                NotificationMode.Sms => "SMS",
                NotificationMode.AppNotification => "App",
                _ => recipients.Mode.ToString()
            };
        }

        private static List<string> GetAvailableVariables()
        {
            return new List<string>
            {
                "{{ConsumerName}}",
                "{{ProductName}}",
                "{{LicenseId}}",
                "{{ExpiryDate}}",
                "{{ValidFromDate}}",
                "{{ValidToDate}}",
                "{{ContactEmail}}",
                "{{ContactName}}",
                "{{CompanyName}}",
                "{{LicenseTier}}",
                "{{CurrentDate}}",
                "{{SystemName}}",
                "{{SupportEmail}}",
                "{{DaysUntilExpiry}}",
                "{{LicenseStatus}}",
                "{{ProductVersion}}",
                "{{Features}}",
                "{{Reason}}",
                "{{ActionRequired}}"
            };
        }

        private static List<string> GetSuggestedRecipients()
        {
            // TODO: Get from actual consumer/user data
            return new List<string>
            {
                "admin@techway.fit",
                "support@techway.fit",
                "notifications@techway.fit"
            };
        }

        private async Task PopulateSendNotificationViewModel(SendNotificationViewModel model)
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                model.AvailableTemplates = templates.Where(t => t.IsActive)
                    .Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive
                    }).ToList();

                model.SuggestedRecipients = GetSuggestedRecipients();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating send notification view model");
                model.AvailableTemplates = new List<NotificationTemplateItemViewModel>();
                model.SuggestedRecipients = new List<string>();
            }
        }

        private async Task PopulateTestNotificationViewModel(NotificationTestViewModel model)
        {
            try
            {
                var templates = await _notificationService.GetNotificationTemplatesAsync();
                model.AvailableTemplates = templates.Where(t => t.IsActive)
                    .Select(t => new NotificationTemplateItemViewModel
                    {
                        TemplateId = t.TemplateId,
                        TemplateName = t.TemplateName,
                        NotificationType = t.NotificationType,
                        NotificationMode = t.Preferences.Mode,
                        Subject = t.Subject,
                        IsActive = t.IsActive
                    }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating test notification view model");
                model.AvailableTemplates = new List<NotificationTemplateItemViewModel>();
            }
        }

        #endregion
    }
}
