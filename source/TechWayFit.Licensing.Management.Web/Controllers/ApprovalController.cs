using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Web.ViewModels.Approval;
using TechWayFit.Licensing.Management.Web.Controllers;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for managing approval workflows and dashboard
/// </summary>
[Authorize]
public class ApprovalController : BaseController
{
    private readonly IWorkflowService<ConsumerAccount> _consumerWorkflowService;
    private readonly IWorkflowService<EnterpriseProduct> _productWorkflowService;
    private readonly IWorkflowService<ProductLicense> _licenseWorkflowService;
    private readonly ILogger<ApprovalController> _logger;

    public ApprovalController(
        IWorkflowService<ConsumerAccount> consumerWorkflowService,
        IWorkflowService<EnterpriseProduct> productWorkflowService,
        IWorkflowService<ProductLicense> licenseWorkflowService,
        ILogger<ApprovalController> logger)
    {
        _consumerWorkflowService = consumerWorkflowService ?? throw new ArgumentNullException(nameof(consumerWorkflowService));
        _productWorkflowService = productWorkflowService ?? throw new ArgumentNullException(nameof(productWorkflowService));
        _licenseWorkflowService = licenseWorkflowService ?? throw new ArgumentNullException(nameof(licenseWorkflowService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Approval dashboard showing all pending items
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var viewModel = new ApprovalDashboardViewModel();

            // Get pending items from all workflow services
            var pendingConsumers = await _consumerWorkflowService.GetPendingApprovalAsync();
            var pendingProducts = await _productWorkflowService.GetPendingApprovalAsync();
            var pendingLicenses = await _licenseWorkflowService.GetPendingApprovalAsync();

            // Convert to view models
            var pendingItems = new List<PendingApprovalItemViewModel>();

            foreach (var consumer in pendingConsumers)
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = consumer.Id,
                    EntityType = "Consumer",
                    EntityDisplayName = consumer.CompanyName,
                    EntityDescription = $"Consumer account for {consumer.CompanyName}",
                    EntityStatus = consumer.EntityStatus,
                    SubmittedBy = consumer.SubmittedBy ?? "Unknown",
                    SubmittedOn = consumer.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = consumer.SubmittedOn.HasValue ? (DateTime.UtcNow - consumer.SubmittedOn.Value).Days : 0,
                    CanApprove = await _consumerWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    CanReject = await _consumerWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    ApprovalUrl = Url.Action("Approve", new { id = consumer.Id, type = "Consumer" }) ?? "",
                    RejectUrl = Url.Action("Reject", new { id = consumer.Id, type = "Consumer" }) ?? "",
                    DetailsUrl = Url.Action("Details", "Consumer", new { id = consumer.Id }) ?? ""
                });
            }

            foreach (var product in pendingProducts)
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = product.Id,
                    EntityType = "Product",
                    EntityDisplayName = product.Name,
                    EntityDescription = $"Product: {product.Name} - {product.Description}",
                    EntityStatus = product.EntityStatus,
                    SubmittedBy = product.SubmittedBy ?? "Unknown",
                    SubmittedOn = product.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = product.SubmittedOn.HasValue ? (DateTime.UtcNow - product.SubmittedOn.Value).Days : 0,
                    CanApprove = await _productWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    CanReject = await _productWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    ApprovalUrl = Url.Action("Approve", new { id = product.Id, type = "Product" }) ?? "",
                    RejectUrl = Url.Action("Reject", new { id = product.Id, type = "Product" }) ?? "",
                    DetailsUrl = Url.Action("Details", "Product", new { id = product.Id }) ?? ""
                });
            }

            foreach (var license in pendingLicenses)
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = license.Id,
                    EntityType = "License",
                    EntityDisplayName = $"License {license.LicenseCode}",
                    EntityDescription = $"License for {license.LicenseConsumer.Consumer.CompanyName}",
                    EntityStatus = license.EntityStatus,
                    SubmittedBy = license.SubmittedBy ?? "Unknown",
                    SubmittedOn = license.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = license.SubmittedOn.HasValue ? (DateTime.UtcNow - license.SubmittedOn.Value).Days : 0,
                    CanApprove = await _licenseWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    CanReject = await _licenseWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                    ApprovalUrl = Url.Action("Approve", new { id = license.Id, type = "License" }) ?? "",
                    RejectUrl = Url.Action("Reject", new { id = license.Id, type = "License" }) ?? "",
                    DetailsUrl = Url.Action("Details", "License", new { id = license.Id }) ?? ""
                });
            }

            // Sort by submission date (oldest first)
            viewModel.PendingItems = pendingItems.OrderBy(x => x.SubmittedOn).ToList();

            // Calculate statistics
            viewModel.Statistics = new ApprovalStatisticsViewModel
            {
                TotalPending = pendingItems.Count,
                OldestPendingDays = pendingItems.Any() ? pendingItems.Max(x => x.DaysInQueue) : 0,
                PendingByType = pendingItems.GroupBy(x => x.EntityType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading approval dashboard");
            TempData["ErrorMessage"] = "Error loading approval dashboard. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Approve an entity
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Approve(Guid id, string type, string? comments)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.ApproveAsync(id, userId, comments);
                    TempData["SuccessMessage"] = "Consumer account approved successfully.";
                    break;
                case "product":
                    await _productWorkflowService.ApproveAsync(id, userId, comments);
                    TempData["SuccessMessage"] = "Product approved successfully.";
                    break;
                case "license":
                    await _licenseWorkflowService.ApproveAsync(id, userId, comments);
                    TempData["SuccessMessage"] = "License approved successfully.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Invalid entity type.";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving {Type} with ID {Id}", type, id);
            TempData["ErrorMessage"] = "Error approving item. Please try again.";
        }

        return RedirectToAction("Dashboard");
    }

    /// <summary>
    /// Reject an entity
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, string type, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToAction("Dashboard");
        }

        try
        {
            var userId = GetCurrentUserId();
            
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.RejectAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "Consumer account rejected.";
                    break;
                case "product":
                    await _productWorkflowService.RejectAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "Product rejected.";
                    break;
                case "license":
                    await _licenseWorkflowService.RejectAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "License rejected.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Invalid entity type.";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting {Type} with ID {Id}", type, id);
            TempData["ErrorMessage"] = "Error rejecting item. Please try again.";
        }

        return RedirectToAction("Dashboard");
    }

    /// <summary>
    /// Get user's own submissions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MySubmissions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var viewModel = new UserSubmissionsViewModel();

            // Get user's submissions from all services
            var consumerSubmissions = await _consumerWorkflowService.GetUserEntitiesAsync(userId);
            var productSubmissions = await _productWorkflowService.GetUserEntitiesAsync(userId);
            var licenseSubmissions = await _licenseWorkflowService.GetUserEntitiesAsync(userId);

            var submissions = new List<UserSubmissionItemViewModel>();

            foreach (var consumer in consumerSubmissions)
            {
                submissions.Add(new UserSubmissionItemViewModel
                {
                    EntityId = consumer.Id,
                    EntityType = "Consumer",
                    EntityDisplayName = consumer.CompanyName,
                    EntityStatus = consumer.EntityStatus,
                    SubmittedOn = consumer.SubmittedOn ?? consumer.CreatedOn,
                    ReviewedBy = consumer.ReviewedBy,
                    ReviewedOn = consumer.ReviewedOn,
                    ReviewComments = consumer.ReviewComments,
                    CanWithdraw = consumer.EntityStatus == EntityStatus.PendingApproval,
                    CanEdit = consumer.EntityStatus == EntityStatus.Draft || consumer.EntityStatus == EntityStatus.Rejected,
                    EditUrl = Url.Action("Edit", "Consumer", new { id = consumer.Id }) ?? "",
                    DetailsUrl = Url.Action("Details", "Consumer", new { id = consumer.Id }) ?? ""
                });
            }

            viewModel.Submissions = submissions.OrderByDescending(x => x.SubmittedOn).ToList();
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user submissions");
            TempData["ErrorMessage"] = "Error loading your submissions. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Withdraw a submission
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Withdraw(Guid id, string type, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.WithdrawAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "Consumer account withdrawn from approval.";
                    break;
                case "product":
                    await _productWorkflowService.WithdrawAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "Product withdrawn from approval.";
                    break;
                case "license":
                    await _licenseWorkflowService.WithdrawAsync(id, userId, reason);
                    TempData["SuccessMessage"] = "License withdrawn from approval.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Invalid entity type.";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing {Type} with ID {Id}", type, id);
            TempData["ErrorMessage"] = "Error withdrawing item. Please try again.";
        }

        return RedirectToAction("MySubmissions");
    }

    /// <summary>
    /// Submit an entity for approval
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitForApproval(Guid id, string type)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.SubmitForApprovalAsync(id, userId);
                    TempData["SuccessMessage"] = "Consumer account submitted for approval.";
                    break;
                case "product":
                    await _productWorkflowService.SubmitForApprovalAsync(id, userId);
                    TempData["SuccessMessage"] = "Product submitted for approval.";
                    break;
                case "license":
                    await _licenseWorkflowService.SubmitForApprovalAsync(id, userId);
                    TempData["SuccessMessage"] = "License submitted for approval.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Invalid entity type.";
                    return BadRequest();
            }

            return RedirectToAction("MySubmissions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting {Type} with ID {Id} for approval", type, id);
            TempData["ErrorMessage"] = "Error submitting item for approval. Please try again.";
            return RedirectToAction("MySubmissions");
        }
    }

    private string GetCurrentUserId()
    {
        return User.Identity?.Name ?? "Anonymous";
    }
}
