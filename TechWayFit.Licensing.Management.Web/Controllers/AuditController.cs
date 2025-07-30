using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.WebUI.ViewModels.Audit;
using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.WebUI.Controllers;

/// <summary>
/// Controller for audit log management and viewing
/// </summary>
[Authorize]
[Route("[controller]")]
public class AuditController : Controller
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Main audit dashboard
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // Default to last 30 days if no dates provided
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var viewModel = new AuditDashboardViewModel
            {
                DateRangeStart = start,
                DateRangeEnd = end
            };

            // Get statistics
            viewModel.Statistics = await _auditService.GetAuditStatisticsAsync(start, end);

            // Get recent entries (last 10)
            var recentEntries = await _auditService.GetAuditEntriesAsync(
                pageNumber: 1, 
                pageSize: 10,
                fromDate: start,
                toDate: end);

            viewModel.RecentEntries = recentEntries.Select(MapToViewModel);

            // Get available filters
            viewModel.AvailableEntityTypes = await _auditService.GetDistinctEntityTypesAsync();
            viewModel.AvailableActions = await _auditService.GetDistinctActionsAsync();

            // Calculate summary counts
            viewModel.TotalEntriesToday = await _auditService.GetAuditEntryCountAsync(
                fromDate: DateTime.UtcNow.Date,
                toDate: DateTime.UtcNow.Date.AddDays(1));

            viewModel.TotalEntriesWeek = await _auditService.GetAuditEntryCountAsync(
                fromDate: DateTime.UtcNow.AddDays(-7),
                toDate: DateTime.UtcNow);

            viewModel.TotalEntriesMonth = await _auditService.GetAuditEntryCountAsync(
                fromDate: DateTime.UtcNow.AddDays(-30),
                toDate: DateTime.UtcNow);            // Generate activity trends
            viewModel.ActivityTrends = GenerateActivityTrends(viewModel.Statistics.EntriesByDate);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit dashboard");
            TempData["ErrorMessage"] = "Error loading audit dashboard. Please try again.";
            return View(new AuditDashboardViewModel());
        }
    }

    /// <summary>
    /// Audit entries list with filtering and pagination
    /// </summary>
    [HttpGet("Entries")]
    public async Task<IActionResult> Entries(
        string? searchTerm = null,
        string? entityType = null,
        string? entityId = null,
        string? actionType = null,
        string? userName = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        string sortBy = "Timestamp",
        string sortOrder = "desc")
    {
        try
        {
            var filter = new AuditFilterViewModel
            {
                SearchTerm = searchTerm,
                EntityType = entityType,
                EntityId = entityId,
                ActionType = actionType,
                UserName = userName,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = Math.Max(1, page),
                PageSize = Math.Min(100, Math.Max(10, pageSize)),
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Get audit entries
            var entries = await _auditService.GetAuditEntriesAsync(
                entityType: filter.EntityType,
                entityId: filter.EntityId,
                actionType: filter.ActionType,
                userId: filter.UserName,
                fromDate: filter.StartDate,
                toDate: filter.EndDate,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            // Get total count
            var totalCount = await _auditService.GetAuditEntryCountAsync(
                entityType: filter.EntityType,
                entityId: filter.EntityId,
                actionType: filter.ActionType,
                userId: filter.UserName,
                fromDate: filter.StartDate,
                toDate: filter.EndDate);

            var viewModel = new AuditListViewModel
            {
                Entries = entries.Select(MapToViewModel),
                Filter = filter,
                TotalEntries = totalCount,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalCount
                }
            };

            // Get available filters
            viewModel.AvailableEntityTypes = await _auditService.GetDistinctEntityTypesAsync();
            viewModel.AvailableActions = await _auditService.GetDistinctActionsAsync();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit entries");
            TempData["ErrorMessage"] = "Error loading audit entries. Please try again.";
            return View(new AuditListViewModel());
        }
    }

    /// <summary>
    /// Detailed view of a specific audit entry
    /// </summary>
    [HttpGet("Entry/{entryId}")]
    public async Task<IActionResult> EntryDetails(string entryId)
    {
        try
        {
            if (string.IsNullOrEmpty(entryId))
            {
                TempData["ErrorMessage"] = "Invalid audit entry ID.";
                return RedirectToAction(nameof(Entries));
            }

            // Get the main entry
            var entries = await _auditService.GetAuditEntriesAsync(pageNumber: 1, pageSize: 1);
            var entry = entries.FirstOrDefault(e => e.EntryId == entryId);

            if (entry == null)
            {
                TempData["ErrorMessage"] = "Audit entry not found.";
                return RedirectToAction(nameof(Entries));
            }

            var viewModel = new AuditEntryDetailViewModel
            {
                Entry = MapToViewModel(entry)
            };

            // Get related entries for the same entity
            if (!string.IsNullOrEmpty(entry.EntityType) && !string.IsNullOrEmpty(entry.EntityId))
            {
                var relatedEntries = await _auditService.GetAuditEntriesAsync(
                    entityType: entry.EntityType,
                    entityId: entry.EntityId,
                    pageNumber: 1,
                    pageSize: 10);

                viewModel.RelatedEntries = relatedEntries
                    .Where(e => e.EntryId != entryId)
                    .Select(MapToViewModel);
            }

            // Parse changes if available
            viewModel.Changes = ParseChanges(entry);
            viewModel.FormattedOldValue = FormatValue(entry.OldValue);
            viewModel.FormattedNewValue = FormatValue(entry.NewValue);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit entry details for {EntryId}", entryId);
            TempData["ErrorMessage"] = "Error loading audit entry details.";
            return RedirectToAction(nameof(Entries));
        }
    }

    /// <summary>
    /// Security-focused audit view
    /// </summary>
    [HttpGet("Security")]
    public async Task<IActionResult> Security(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var viewModel = new SecurityAuditViewModel
            {
                DateRangeStart = start,
                DateRangeEnd = end
            };            // Get security-related entries
            var securityEntries = await _auditService.GetSecurityAuditEntriesAsync(start, end, 1, 50);
            viewModel.SecurityEntries = securityEntries.Select(MapToViewModel);

            // Get specific security event types
            var loginEntries = await _auditService.GetAuditEntriesByActionAsync("LOGIN_FAILED", start, end, 1, 20);
            viewModel.FailedLogins = loginEntries.Select(MapToViewModel);

            // Calculate risk metrics
            viewModel.TotalSecurityEvents = await _auditService.GetAuditEntryCountAsync(
                fromDate: start,
                toDate: end);

            // Simple risk classification (can be enhanced with business rules)
            var allEntries = viewModel.SecurityEntries.ToList();
            viewModel.HighRiskEvents = allEntries.Count(e => IsHighRiskAction(e.ActionType));
            viewModel.MediumRiskEvents = allEntries.Count(e => IsMediumRiskAction(e.ActionType));
            viewModel.LowRiskEvents = allEntries.Count() - viewModel.HighRiskEvents - viewModel.MediumRiskEvents;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security audit view");
            TempData["ErrorMessage"] = "Error loading security audit information.";
            return View(new SecurityAuditViewModel());
        }
    }

    /// <summary>
    /// Entity-specific audit history
    /// </summary>
    [HttpGet("Entity/{entityType}/{entityId}")]
    public async Task<IActionResult> EntityAudit(string entityType, string entityId, 
        DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        try
        {
            if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId))
            {
                TempData["ErrorMessage"] = "Invalid entity information.";
                return RedirectToAction(nameof(Entries));
            }

            var viewModel = new EntityAuditViewModel
            {
                EntityType = entityType,
                EntityId = entityId,
                EntityDisplayName = $"{entityType} ({entityId})",
                DateRangeStart = startDate,
                DateRangeEnd = endDate
            };

            // Get entity-specific entries
            var entries = await _auditService.GetAuditEntriesAsync(
                entityType: entityType,
                entityId: entityId,
                fromDate: startDate,
                toDate: endDate,
                pageNumber: page,
                pageSize: pageSize);

            viewModel.Entries = entries.Select(MapToViewModel);

            // Get total count for pagination
            var totalCount = await _auditService.GetAuditEntryCountAsync(
                entityType: entityType,
                entityId: entityId,
                fromDate: startDate,
                toDate: endDate);

            viewModel.Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount
            };

            // Get entity-specific statistics
            var stats = await _auditService.GetAuditStatisticsAsync(startDate, endDate);
            viewModel.Statistics = new AuditStatisticsViewModel
            {
                TotalEntries = totalCount,
                TopActions = stats.EntriesByAction
                    .OrderByDescending(kv => kv.Value)
                    .Take(5),
                DailyActivity = stats.EntriesByDate
                    .OrderBy(kv => kv.Key),
                DateRangeStart = startDate,
                DateRangeEnd = endDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading entity audit for {EntityType}/{EntityId}", entityType, entityId);
            TempData["ErrorMessage"] = "Error loading entity audit history.";
            return RedirectToAction(nameof(Entries));
        }
    }

    /// <summary>
    /// Export audit data
    /// </summary>
    [HttpPost("Export")]
    public async Task<IActionResult> Export(AuditExportViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid export parameters.";
                return RedirectToAction(nameof(Entries));
            }

            var exportData = await _auditService.ExportAuditEntriesAsync(
                model.Format,
                model.EntityType,
                model.EntityId,
                model.StartDate,
                model.EndDate);

            var contentType = model.Format.ToLowerInvariant() switch
            {
                "json" => "application/json",
                "xml" => "application/xml",
                _ => "text/csv"
            };

            return File(exportData, contentType, model.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit data");
            TempData["ErrorMessage"] = "Error exporting audit data. Please try again.";
            return RedirectToAction(nameof(Entries));
        }
    }

    /// <summary>
    /// AJAX endpoint for audit entry details modal
    /// </summary>
    [HttpGet("GetEntryDetails/{entryId}")]
    public async Task<IActionResult> GetEntryDetails(string entryId)
    {
        try
        {
            var entries = await _auditService.GetAuditEntriesAsync(pageNumber: 1, pageSize: 1);
            var entry = entries.FirstOrDefault(e => e.EntryId == entryId);

            if (entry == null)
            {
                return NotFound();
            }

            var viewModel = new AuditEntryDetailViewModel
            {
                Entry = MapToViewModel(entry),
                Changes = ParseChanges(entry),
                FormattedOldValue = FormatValue(entry.OldValue),
                FormattedNewValue = FormatValue(entry.NewValue)
            };

            return PartialView("_AuditEntryDetails", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit entry details for {EntryId}", entryId);
            return StatusCode(500, "Error loading entry details");
        }
    }

    /// <summary>
    /// AJAX endpoint for activity chart data
    /// </summary>
    [HttpGet("GetActivityData")]
    public async Task<IActionResult> GetActivityData(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await _auditService.GetAuditStatisticsAsync(start, end);            var chartData = statistics.EntriesByDate
                .OrderBy(kv => kv.Key)
                .Select(kv => new
                {
                    Date = kv.Key.ToString("yyyy-MM-dd"),
                    Count = kv.Value
                });

            return Json(chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity chart data");
            return StatusCode(500, "Error loading chart data");
        }
    }

    #region Private Helper Methods

    private static AuditEntryItemViewModel MapToViewModel(AuditEntry entry)
    {
        return new AuditEntryItemViewModel
        {
            EntryId = entry.EntryId,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            ActionType = entry.ActionType,
            OldValue = entry.OldValue,
            NewValue = entry.NewValue,
            UserName = entry.UserName,
            Timestamp = entry.Timestamp,
            IpAddress = entry.IpAddress,
            UserAgent = entry.UserAgent,
            Reason = entry.Reason,
            Metadata = entry.Metadata
        };
    }

    private static IEnumerable<AuditChangeViewModel> ParseChanges(AuditEntry entry)
    {
        var changes = new List<AuditChangeViewModel>();

        // Simple change detection - can be enhanced with JSON comparison
        if (!string.IsNullOrEmpty(entry.OldValue) || !string.IsNullOrEmpty(entry.NewValue))
        {
            changes.Add(new AuditChangeViewModel
            {
                PropertyName = "Value",
                OldValue = entry.OldValue,
                NewValue = entry.NewValue,
                ChangeType = string.IsNullOrEmpty(entry.OldValue) ? "Added" :
                           string.IsNullOrEmpty(entry.NewValue) ? "Removed" : "Modified"
            });
        }

        return changes;
    }

    private static string? FormatValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        // Try to format JSON
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(value);
            return System.Text.Json.JsonSerializer.Serialize(jsonDoc, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return value; // Return as-is if not JSON
        }
    }

    private static IEnumerable<AuditTrendViewModel> GenerateActivityTrends(IEnumerable<KeyValuePair<DateTime, int>> dailyActivity)
    {
        return dailyActivity
            .OrderBy(kv => kv.Key)
            .Select(kv => new AuditTrendViewModel
            {
                Date = kv.Key,
                EntryCount = kv.Value,
                ActionType = "All"
            });
    }

    private static bool IsHighRiskAction(string actionType)
    {
        var highRiskActions = new[]
        {
            "DELETE", "REMOVE", "PURGE", "ADMIN_ACCESS", "PRIVILEGE_ESCALATION",
            "PASSWORD_RESET", "SECURITY_OVERRIDE", "SYSTEM_SHUTDOWN"
        };

        return highRiskActions.Any(action => 
            actionType.ToUpperInvariant().Contains(action));
    }

    private static bool IsMediumRiskAction(string actionType)
    {
        var mediumRiskActions = new[]
        {
            "UPDATE", "MODIFY", "ACTIVATE", "DEACTIVATE", "SUSPEND",
            "PERMISSION_CHANGE", "CONFIG_CHANGE", "LICENSE_ISSUE"
        };

        return mediumRiskActions.Any(action => 
            actionType.ToUpperInvariant().Contains(action));
    }

    #endregion
}
