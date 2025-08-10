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
            var pendingConsumers =  _consumerWorkflowService.GetPendingApprovalAsync();
            var pendingProducts =  _productWorkflowService.GetPendingApprovalAsync();
            var pendingLicenses =  _licenseWorkflowService.GetPendingApprovalAsync();

            // Convert to view models
            var pendingItems = new List<PendingApprovalItemViewModel>();
            var userCanApprove = await _consumerWorkflowService.CanUserApproveAsync(GetCurrentUserId());
            foreach (var consumer in await pendingConsumers)
            {
                var pendingApprovalItem = new PendingApprovalItemViewModel
                {
                    EntityDisplayName = consumer.CompanyName,
                    EntityDescription = $"Consumer account for {consumer.CompanyName}",
                    CanApprove = userCanApprove,
                    CanReject = userCanApprove
                };
                Update(consumer, pendingApprovalItem, "Consumer", consumer.ConsumerId);
                pendingItems.Add(pendingApprovalItem);
            }

            foreach (var product in await pendingProducts)
            {
                var pendingApprovalItem = new PendingApprovalItemViewModel
                {
                    EntityDisplayName = product.Name,
                    EntityDescription = $"Product: {product.Name} - {product.Description}",
                    CanApprove = userCanApprove,
                    CanReject = userCanApprove
                };
                Update(product, pendingApprovalItem, "Product", product.Id);
                pendingItems.Add(pendingApprovalItem);
            }

            foreach (var license in await pendingLicenses)
            {
                var pendingApprovalItem = new PendingApprovalItemViewModel
                {
                    EntityDisplayName = $"License {license.LicenseCode}",
                    EntityDescription = $"License for {license.LicenseConsumer.Consumer.CompanyName}",
                    CanApprove = userCanApprove,
                    CanReject = userCanApprove                    
                };
                Update(license, pendingApprovalItem, "License", license.Id);
                pendingItems.Add(pendingApprovalItem);
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
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.ApproveAsync(id, comments);
                    TempData["SuccessMessage"] = "Consumer account approved successfully.";
                    break;
                case "product":
                    await _productWorkflowService.ApproveAsync(id, comments);
                    TempData["SuccessMessage"] = "Product approved successfully.";
                    break;
                case "license":
                    await _licenseWorkflowService.ApproveAsync(id, comments);
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
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.RejectAsync(id, reason);
                    TempData["SuccessMessage"] = "Consumer account rejected.";
                    break;
                case "product":
                    await _productWorkflowService.RejectAsync(id, reason);
                    TempData["SuccessMessage"] = "Product rejected.";
                    break;
                case "license":
                    await _licenseWorkflowService.RejectAsync(id, reason);
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
                    EntityStatus = consumer.Workflow.Status,
                    SubmittedOn = consumer.Workflow.SubmittedOn ?? consumer.Audit.CreatedOn,
                    ReviewedBy = consumer.Workflow.ReviewedBy,
                    ReviewedOn = consumer.Workflow.ReviewedOn,
                    ReviewComments = consumer.Workflow.ReviewComments,
                    CanWithdraw = consumer.Workflow.Status == EntityStatus.PendingApproval,
                    CanEdit = consumer.Workflow.Status == EntityStatus.Draft || consumer.Workflow.Status == EntityStatus.Rejected,
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
            
            switch (type.ToLower())
            {
                case "consumer":
                    await _consumerWorkflowService.WithdrawAsync(id, reason);
                    TempData["SuccessMessage"] = "Consumer account withdrawn from approval.";
                    break;
                case "product":
                    await _productWorkflowService.WithdrawAsync(id, reason);
                    TempData["SuccessMessage"] = "Product withdrawn from approval.";
                    break;
                case "license":
                    await _licenseWorkflowService.WithdrawAsync(id, reason);
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
                    await _consumerWorkflowService.SubmitForApprovalAsync(id );
                    TempData["SuccessMessage"] = "Consumer account submitted for approval.";
                    break;
                case "product":
                    await _productWorkflowService.SubmitForApprovalAsync(id);
                    TempData["SuccessMessage"] = "Product submitted for approval.";
                    break;
                case "license":
                    await _licenseWorkflowService.SubmitForApprovalAsync(id);
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
    private PendingApprovalItemViewModel Update(IWorkflowCapable entity, PendingApprovalItemViewModel viewModel, string entityType, Guid entityId)
    {
        viewModel.EntityId = entityId;
        viewModel.EntityType = entityType;
        viewModel.EntityStatus = entity.Workflow.Status;
        viewModel.SubmittedBy = entity.Workflow.SubmittedBy ?? "Unknown";
        viewModel.SubmittedOn = entity.Workflow.SubmittedOn ?? DateTime.MinValue;
        viewModel.DaysInQueue = entity.Workflow.SubmittedOn.HasValue ? (DateTime.UtcNow - entity.Workflow.SubmittedOn.Value).Days : 0;
        viewModel.ApprovalUrl = Url.Action("Approve", new { id = entityId, type = entityType }) ?? "";
        viewModel.RejectUrl = Url.Action("Reject", new { id = entityId, type = entityType }) ?? "";
        viewModel.DetailsUrl = Url.Action("Details", entityType, new { id = entityId }) ?? "";
        
        return viewModel;
    }     
}
