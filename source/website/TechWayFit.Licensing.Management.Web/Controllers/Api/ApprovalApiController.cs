using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Web.Models.Api.Approval;
using TechWayFit.Licensing.Management.Web.Controllers;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for managing approval workflows
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalApiController : BaseController
{
    private readonly IWorkflowService<ConsumerAccount> _consumerWorkflowService;
    private readonly IWorkflowService<EnterpriseProduct> _productWorkflowService;
    private readonly IWorkflowService<ProductLicense> _licenseWorkflowService;
    private readonly ILogger<ApprovalApiController> _logger;

    public ApprovalApiController(
        IWorkflowService<ConsumerAccount> consumerWorkflowService,
        IWorkflowService<EnterpriseProduct> productWorkflowService,
        IWorkflowService<ProductLicense> licenseWorkflowService,
        ILogger<ApprovalApiController> logger)
    {
        _consumerWorkflowService = consumerWorkflowService ?? throw new ArgumentNullException(nameof(consumerWorkflowService));
        _productWorkflowService = productWorkflowService ?? throw new ArgumentNullException(nameof(productWorkflowService));
        _licenseWorkflowService = licenseWorkflowService ?? throw new ArgumentNullException(nameof(licenseWorkflowService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get pending approvals with filtering and pagination
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(GetPendingApprovalsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetPendingApprovalsResponse>> GetPendingApprovals([FromQuery] GetPendingApprovalsRequest request)
    {
        try
        {
            var allPendingItems = new List<PendingApprovalItemResponse>();

            // Get pending items from all workflow services
            var pendingConsumers = await _consumerWorkflowService.GetPendingApprovalAsync(1, 1000); // Get all for filtering
            var pendingProducts = await _productWorkflowService.GetPendingApprovalAsync(1, 1000);
            var pendingLicenses = await _licenseWorkflowService.GetPendingApprovalAsync(1, 1000);

            // Convert to API response models
            foreach (var consumer in pendingConsumers)
            {
                if (string.IsNullOrEmpty(request.EntityTypeFilter) || request.EntityTypeFilter.Equals("Consumer", StringComparison.OrdinalIgnoreCase))
                {
                    allPendingItems.Add(new PendingApprovalItemResponse
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
                        CanApprove = await _consumerWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanReject = await _consumerWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanWithdraw = false
                    });
                }
            }

            foreach (var product in pendingProducts)
            {
                if (string.IsNullOrEmpty(request.EntityTypeFilter) || request.EntityTypeFilter.Equals("Product", StringComparison.OrdinalIgnoreCase))
                {
                    allPendingItems.Add(new PendingApprovalItemResponse
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
                        CanApprove = await _productWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanReject = await _productWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanWithdraw = false
                    });
                }
            }

            foreach (var license in pendingLicenses)
            {
                if (string.IsNullOrEmpty(request.EntityTypeFilter) || request.EntityTypeFilter.Equals("License", StringComparison.OrdinalIgnoreCase))
                {
                    allPendingItems.Add(new PendingApprovalItemResponse
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
                        CanApprove = await _licenseWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanReject = await _licenseWorkflowService.CanUserApproveAsync(GetCurrentUserId()),
                        CanWithdraw = false
                    });
                }
            }

            // Apply additional filters
            var filteredItems = allPendingItems.AsQueryable();

            if (!string.IsNullOrEmpty(request.SubmittedByFilter))
            {
                filteredItems = filteredItems.Where(x => x.SubmittedBy.Contains(request.SubmittedByFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (request.SubmittedFromFilter.HasValue)
            {
                filteredItems = filteredItems.Where(x => x.SubmittedOn >= request.SubmittedFromFilter.Value);
            }

            if (request.SubmittedToFilter.HasValue)
            {
                filteredItems = filteredItems.Where(x => x.SubmittedOn <= request.SubmittedToFilter.Value);
            }

            // Apply sorting
            switch (request.SortBy?.ToLower())
            {
                case "entitytype":
                    filteredItems = request.SortDescending ? 
                        filteredItems.OrderByDescending(x => x.EntityType) : 
                        filteredItems.OrderBy(x => x.EntityType);
                    break;
                case "submittedby":
                    filteredItems = request.SortDescending ? 
                        filteredItems.OrderByDescending(x => x.SubmittedBy) : 
                        filteredItems.OrderBy(x => x.SubmittedBy);
                    break;
                case "daysinqueue":
                    filteredItems = request.SortDescending ? 
                        filteredItems.OrderByDescending(x => x.DaysInQueue) : 
                        filteredItems.OrderBy(x => x.DaysInQueue);
                    break;
                default: // "submittedon" or default
                    filteredItems = request.SortDescending ? 
                        filteredItems.OrderByDescending(x => x.SubmittedOn) : 
                        filteredItems.OrderBy(x => x.SubmittedOn);
                    break;
            }

            var totalCount = filteredItems.Count();
            var pagedItems = filteredItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new GetPendingApprovalsResponse
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approvals");
            return StatusCode(500, new { message = "An error occurred while retrieving pending approvals" });
        }
    }

    /// <summary>
    /// Approve an entity
    /// </summary>
    [HttpPost("approve")]
    [ProducesResponseType(typeof(WorkflowActionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<WorkflowActionResponse>> ApproveEntity([FromBody] ApproveEntityRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? result = null;
            string entityDisplayName = "";

            switch (request.EntityType.ToLower())
            {
                case "consumer":
                    var consumer = await _consumerWorkflowService.ApproveAsync(request.EntityId, request.Comments);
                    result = consumer;
                    entityDisplayName = consumer.CompanyName;
                    break;
                case "product":
                    var product = await _productWorkflowService.ApproveAsync(request.EntityId, request.Comments);
                    result = product;
                    entityDisplayName = product.Name;
                    break;
                case "license":
                    var license = await _licenseWorkflowService.ApproveAsync(request.EntityId, request.Comments);
                    result = license;
                    entityDisplayName = $"License {license.LicenseCode}";
                    break;
                default:
                    return BadRequest(new { message = "Invalid entity type" });
            }

            var response = new WorkflowActionResponse
            {
                Success = true,
                Message = $"{request.EntityType} approved successfully",
                EntityId = request.EntityId,
                NewStatus = EntityStatus.Approved,
                ActionTimestamp = DateTime.UtcNow,
                EntityType = request.EntityType,
                EntityDisplayName = entityDisplayName
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Entity not found for approval: {EntityId}", request.EntityId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving entity {EntityId}", request.EntityId);
            return StatusCode(500, new { message = "An error occurred while approving the entity" });
        }
    }

    /// <summary>
    /// Reject an entity
    /// </summary>
    [HttpPost("reject")]
    [ProducesResponseType(typeof(WorkflowActionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<WorkflowActionResponse>> RejectEntity([FromBody] RejectEntityRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? result = null;
            string entityDisplayName = "";

            switch (request.EntityType.ToLower())
            {
                case "consumer":
                    var consumer = await _consumerWorkflowService.RejectAsync(request.EntityId, request.Reason);
                    result = consumer;
                    entityDisplayName = consumer.CompanyName;
                    break;
                case "product":
                    var product = await _productWorkflowService.RejectAsync(request.EntityId, request.Reason);
                    result = product;
                    entityDisplayName = product.Name;
                    break;
                case "license":
                    var license = await _licenseWorkflowService.RejectAsync(request.EntityId, request.Reason);
                    result = license;
                    entityDisplayName = $"License {license.LicenseCode}";
                    break;
                default:
                    return BadRequest(new { message = "Invalid entity type" });
            }

            var response = new WorkflowActionResponse
            {
                Success = true,
                Message = $"{request.EntityType} rejected",
                EntityId = request.EntityId,
                NewStatus = EntityStatus.Rejected,
                ActionTimestamp = DateTime.UtcNow,
                EntityType = request.EntityType,
                EntityDisplayName = entityDisplayName
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Entity not found for rejection: {EntityId}", request.EntityId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting entity {EntityId}", request.EntityId);
            return StatusCode(500, new { message = "An error occurred while rejecting the entity" });
        }
    }

    /// <summary>
    /// Get workflow statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(WorkflowStatisticsResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<WorkflowStatisticsResponse>> GetStatistics()
    {
        try
        {
            // Get pending counts
            var pendingConsumers = await _consumerWorkflowService.GetPendingApprovalAsync(1, 1000);
            var pendingProducts = await _productWorkflowService.GetPendingApprovalAsync(1, 1000);
            var pendingLicenses = await _licenseWorkflowService.GetPendingApprovalAsync(1, 1000);

            var allPending = new List<(string type, DateTime? submittedOn)>();
            allPending.AddRange(pendingConsumers.Select(x => ("Consumer", x.Workflow.SubmittedOn)));
            allPending.AddRange(pendingProducts.Select(x => ("Product", x.Workflow.SubmittedOn)));
            allPending.AddRange(pendingLicenses.Select(x => ("License", x.Workflow.SubmittedOn)));

            var response = new WorkflowStatisticsResponse
            {
                TotalPending = allPending.Count,
                PendingByType = allPending.GroupBy(x => x.type).ToDictionary(g => g.Key, g => g.Count()),
                OldestPendingDays = allPending.Where(x => x.submittedOn.HasValue)
                    .Select(x => (DateTime.UtcNow - x.submittedOn!.Value).Days)
                    .DefaultIfEmpty(0)
                    .Max()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }
}
