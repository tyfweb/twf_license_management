using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Web.ViewModels.Approval;

namespace TechWayFit.Licensing.Management.Web.ViewComponents;

/// <summary>
/// View component for displaying pending approval items on the dashboard
/// </summary>
public class PendingApprovalsViewComponent : ViewComponent
{
    private readonly IWorkflowService<ConsumerAccount> _consumerWorkflowService;
    private readonly IWorkflowService<EnterpriseProduct> _productWorkflowService;
    private readonly IWorkflowService<ProductLicense> _licenseWorkflowService;
    private readonly ILogger<PendingApprovalsViewComponent> _logger;

    public PendingApprovalsViewComponent(
        IWorkflowService<ConsumerAccount> consumerWorkflowService,
        IWorkflowService<EnterpriseProduct> productWorkflowService,
        IWorkflowService<ProductLicense> licenseWorkflowService,
        ILogger<PendingApprovalsViewComponent> logger)
    {
        _consumerWorkflowService = consumerWorkflowService ?? throw new ArgumentNullException(nameof(consumerWorkflowService));
        _productWorkflowService = productWorkflowService ?? throw new ArgumentNullException(nameof(productWorkflowService));
        _licenseWorkflowService = licenseWorkflowService ?? throw new ArgumentNullException(nameof(licenseWorkflowService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders the pending approvals widget for the dashboard
    /// </summary>
    /// <param name="maxItems">Maximum number of items to display (default: 5)</param>
    /// <param name="showActions">Whether to show action buttons (default: true)</param>
    /// <returns>View with pending approval items</returns>
    public async Task<IViewComponentResult> InvokeAsync(int maxItems = 5, bool showActions = true)
    {
        try
        {
            var pendingItems = new List<PendingApprovalItemViewModel>();

            // Get pending items from all workflow services
            var pendingConsumers = await _consumerWorkflowService.GetPendingApprovalAsync(1, maxItems);
            var pendingProducts = await _productWorkflowService.GetPendingApprovalAsync(1, maxItems);
            var pendingLicenses = await _licenseWorkflowService.GetPendingApprovalAsync(1, maxItems);

            // Convert consumers to view models
            foreach (var consumer in pendingConsumers.Take(maxItems))
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = consumer.ConsumerId,
                    EntityType = "Consumer",
                    EntityDisplayName = consumer.CompanyName,
                    EntityDescription = $"Consumer account for {consumer.CompanyName}",
                    EntityStatus = consumer.Workflow.Status,
                    SubmittedBy = consumer.Workflow.SubmittedBy ?? "Unknown",
                    SubmittedOn = consumer.Workflow.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = consumer.Workflow.SubmittedOn.HasValue ? 
                        (DateTime.UtcNow - consumer.Workflow.SubmittedOn.Value).Days : 0,
                    CanApprove = showActions,
                    CanReject = showActions,
                    CanWithdraw = false, // Users can't withdraw others' submissions
                    ApprovalUrl = showActions ? Url.Action("Approve", "Approval", new { id = consumer.ConsumerId, type = "Consumer" }) ?? "" : "",
                    RejectUrl = showActions ? Url.Action("Reject", "Approval", new { id = consumer.ConsumerId, type = "Consumer" }) ?? "" : "",
                    DetailsUrl = Url.Action("Details", "Consumer", new { id = consumer.ConsumerId }) ?? ""
                });
            }

            // Convert products to view models
            foreach (var product in pendingProducts.Take(maxItems))
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = product.Id,
                    EntityType = "Product",
                    EntityDisplayName = product.Name,
                    EntityDescription = $"Product: {product.Name} - {product.Description}",
                    EntityStatus = product.Workflow.Status,
                    SubmittedBy = product.Workflow.SubmittedBy ?? "Unknown",
                    SubmittedOn = product.Workflow.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = product.Workflow.SubmittedOn.HasValue ? 
                        (DateTime.UtcNow - product.Workflow.SubmittedOn.Value).Days : 0,
                    CanApprove = showActions,
                    CanReject = showActions,
                    CanWithdraw = false,
                    ApprovalUrl = showActions ? Url.Action("Approve", "Approval", new { id = product.Id, type = "Product" }) ?? "" : "",
                    RejectUrl = showActions ? Url.Action("Reject", "Approval", new { id = product.Id, type = "Product" }) ?? "" : "",
                    DetailsUrl = Url.Action("Details", "Product", new { id = product.Id }) ?? ""
                });
            }

            // Convert licenses to view models
            foreach (var license in pendingLicenses.Take(maxItems))
            {
                pendingItems.Add(new PendingApprovalItemViewModel
                {
                    EntityId = license.Id,
                    EntityType = "License",
                    EntityDisplayName = $"License {license.LicenseCode}",
                    EntityDescription = $"License for {license.LicenseConsumer.Product.Name} - Consumer: {license.LicenseConsumer.Consumer.CompanyName}",
                    EntityStatus = license.Workflow.Status,
                    SubmittedBy = license.Workflow.SubmittedBy ?? "Unknown",
                    SubmittedOn = license.Workflow.SubmittedOn ?? DateTime.MinValue,
                    DaysInQueue = license.Workflow.SubmittedOn.HasValue ? 
                        (DateTime.UtcNow - license.Workflow.SubmittedOn.Value).Days : 0,
                    CanApprove = showActions,
                    CanReject = showActions,
                    CanWithdraw = false,
                    ApprovalUrl = showActions ? Url.Action("Approve", "Approval", new { id = license.Id, type = "License" }) ?? "" : "",
                    RejectUrl = showActions ? Url.Action("Reject", "Approval", new { id = license.Id, type = "License" }) ?? "" : "",
                    DetailsUrl = Url.Action("Details", "License", new { id = license.Id }) ?? ""
                });
            }

            // Sort by submission date (oldest first) and take only the specified number
            var sortedItems = pendingItems
                .OrderBy(x => x.SubmittedOn)
                .Take(maxItems)
                .ToList();

            var viewModel = new PendingApprovalsDashboardViewModel
            {
                PendingItems = sortedItems,
                TotalPendingCount = pendingItems.Count,
                ShowActions = showActions,
                MaxItems = maxItems,
                HasMoreItems = pendingItems.Count > maxItems,
                ViewAllUrl = Url.Action("Dashboard", "Approval") ?? ""
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending approvals for dashboard");
            
            // Return empty view model on error
            var errorViewModel = new PendingApprovalsDashboardViewModel
            {
                PendingItems = new List<PendingApprovalItemViewModel>(),
                TotalPendingCount = 0,
                ShowActions = showActions,
                MaxItems = maxItems,
                HasMoreItems = false,
                ViewAllUrl = Url.Action("Dashboard", "Approval") ?? "",
                ErrorMessage = "Unable to load pending approvals at this time."
            };

            return View(errorViewModel);
        }
    }
}
