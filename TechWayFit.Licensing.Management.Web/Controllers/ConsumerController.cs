using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.WebUI.ViewModels.Consumer;

namespace TechWayFit.Licensing.WebUI.Controllers;

/// <summary>
/// Controller for managing consumer accounts
/// </summary>
[Authorize]
public class ConsumerController : Controller
{
    private readonly IConsumerAccountService _consumerAccountService;
    private readonly IProductLicenseService _productLicenseService; 
    private readonly ILogger<ConsumerController> _logger;

    public ConsumerController(
        IConsumerAccountService consumerAccountService,
        IProductLicenseService productLicenseService,
        ILogger<ConsumerController> logger)
    {
        _productLicenseService = productLicenseService ?? throw new ArgumentNullException(nameof(productLicenseService));
        _consumerAccountService = consumerAccountService ?? throw new ArgumentNullException(nameof(consumerAccountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display list of consumer accounts
    /// </summary>
    public async Task<IActionResult> Index(ConsumerFilterViewModel filter)
    {
        try
        {
            _logger.LogInformation("Getting consumer list with filter: SearchTerm={SearchTerm}, Status={Status}",
                filter.SearchTerm, filter.Status);

            var consumers = await _consumerAccountService.GetConsumerAccountsAsync(
                status: filter.Status,
                isActive: filter.IsActive,
                searchTerm: filter.SearchTerm,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            var totalCount = await _consumerAccountService.GetConsumerAccountCountAsync(
                status: filter.Status,
                isActive: filter.IsActive,
                searchTerm: filter.SearchTerm);

            // Map to view models
            var consumerViewModels = consumers.Select(MapToViewModel).ToList();

            // Create list view model
            var listViewModel = new ConsumerListViewModel
            {
                SearchTerm = filter.SearchTerm ?? string.Empty,
                Consumers = consumerViewModels,
                Filter = filter,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalCount
                }
            };

            return View(listViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consumer list");
            TempData["ErrorMessage"] = "Unable to load consumers at this time. Please try again later.";
            return RedirectToAction("Index", "Home");
        }
    }    /// <summary>
         /// Display consumer account details
         /// </summary>
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Consumer ID is required");
        }

        try
        {
            var consumer = await _consumerAccountService.GetConsumerAccountByIdAsync(id);
            var licenses = await _productLicenseService.GetLicensesByConsumerAsync(id);
            var statistics= await _productLicenseService.GetLicenseUsageStatisticsAsync(consumerId: id);
            if (consumer == null)
            {
                return NotFound($"Consumer account with ID '{id}' not found");
            }

            var viewModel = MapToConsumerDetailsViewModel(consumer,licenses, statistics);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading consumer account details for ID: {ConsumerId}", id);
            TempData["ErrorMessage"] = "Failed to load consumer account details. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display create consumer account form
    /// </summary>
    public IActionResult Create()
    {
        var viewModel = new ConsumerCreateViewModel
        {
            // Set default values can be added here if needed
        };

        return View(viewModel);
    }

    /// <summary>
    /// Handle create consumer account form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsumerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Map from view model to domain model
            var consumer = new ConsumerAccount
            {
                ConsumerId = Guid.NewGuid().ToString(),
                CompanyName = model.OrganizationName,
                PrimaryContact = new ContactPerson
                {
                    Name = model.ContactPerson,
                    Email = model.ContactEmail,
                    Phone = model.ContactPhone ?? string.Empty,
                    Position = string.Empty
                },
                Address = new Address
                {
                    Street = model.Address ?? string.Empty,
                    City = model.City ?? string.Empty,
                    State = model.State ?? string.Empty,
                    Country = model.Country ?? string.Empty,
                    PostalCode = model.PostalCode ?? string.Empty
                },
                Status = ConsumerStatus.Active,
                IsActive = true,
                Notes = model.Notes ?? string.Empty
            };

            // Get current user from context
            var currentUser = User.Identity?.Name ?? "system";

            // Create consumer account
            var createdConsumer = await _consumerAccountService.CreateConsumerAccountAsync(consumer, currentUser);

            TempData["SuccessMessage"] = $"Consumer account '{createdConsumer.CompanyName}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = createdConsumer.ConsumerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumer account for company: {CompanyName}", model.OrganizationName);
            ModelState.AddModelError("", "Failed to create consumer account. Please check the information and try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Display edit consumer account form
    /// </summary>
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Consumer ID is required");
        }

        try
        {
            var consumer = await _consumerAccountService.GetConsumerAccountByIdAsync(id);
            if (consumer == null)
            {
                return NotFound($"Consumer account with ID '{id}' not found");
            }

            // Map to edit view model
            var viewModel = new ConsumerEditViewModel
            {
                ConsumerId = consumer.ConsumerId,
                OrganizationName = consumer.CompanyName,
                ContactPerson = consumer.PrimaryContact?.Name ?? string.Empty,
                ContactEmail = consumer.PrimaryContact?.Email ?? string.Empty,
                ContactPhone = consumer.PrimaryContact?.Phone ?? string.Empty,
                Address = consumer.Address?.Street ?? string.Empty,
                City = consumer.Address?.City ?? string.Empty,
                State = consumer.Address?.State ?? string.Empty,
                Country = consumer.Address?.Country ?? string.Empty,
                PostalCode = consumer.Address?.PostalCode ?? string.Empty,
                IsActive = consumer.IsActive,
                Notes = consumer.Notes,
                CreatedAt = consumer.CreatedAt,
                CreatedBy = string.Empty, // Not available in domain model
                LastModifiedAt = null, // Not available in domain model
                LastModifiedBy = string.Empty // Not available in domain model
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading consumer account for edit: {ConsumerId}", id);
            TempData["ErrorMessage"] = "Failed to load consumer account for editing. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handle edit consumer account form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ConsumerEditViewModel model)
    {
        if (id != model.ConsumerId)
        {
            return BadRequest("Consumer ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Get existing consumer
            var existingConsumer = await _consumerAccountService.GetConsumerAccountByIdAsync(id);
            if (existingConsumer == null)
            {
                return NotFound($"Consumer account with ID '{id}' not found");
            }

            // Update the existing consumer with new values
            existingConsumer.CompanyName = model.OrganizationName;
            existingConsumer.IsActive = model.IsActive;
            existingConsumer.Notes = model.Notes;

            // Update primary contact
            if (existingConsumer.PrimaryContact == null)
                existingConsumer.PrimaryContact = new ContactPerson();
            
            existingConsumer.PrimaryContact.Name = model.ContactPerson;
            existingConsumer.PrimaryContact.Email = model.ContactEmail;
            existingConsumer.PrimaryContact.Phone = model.ContactPhone;

            // Update address
            if (existingConsumer.Address == null)
                existingConsumer.Address = new Address();
            
            existingConsumer.Address.Street = model.Address;
            existingConsumer.Address.City = model.City;
            existingConsumer.Address.State = model.State;
            existingConsumer.Address.Country = model.Country;
            existingConsumer.Address.PostalCode = model.PostalCode;

            // Get current user
            var currentUser = User.Identity?.Name ?? "system";

            // Update consumer account
            var updatedConsumer = await _consumerAccountService.UpdateConsumerAccountAsync(existingConsumer, currentUser);

            TempData["SuccessMessage"] = $"Consumer account '{updatedConsumer.CompanyName}' updated successfully.";
            return RedirectToAction(nameof(Details), new { id = updatedConsumer.ConsumerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer account: {ConsumerId}", id);
            ModelState.AddModelError("", "Failed to update consumer account. Please check the information and try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Map domain model to view model
    /// </summary>
    private static ConsumerViewModel MapToViewModel(ConsumerAccount consumer)
    {
        return new ConsumerViewModel
        {
            ConsumerId = consumer.ConsumerId,
            CompanyName = consumer.CompanyName,
            AccountCode = string.Empty, // This field is not in the domain model yet
            PrimaryContactName = consumer.PrimaryContact?.Name ?? string.Empty,
            PrimaryContactEmail = consumer.PrimaryContact?.Email ?? string.Empty,
            PrimaryContactPhone = consumer.PrimaryContact?.Phone ?? string.Empty,
            Address = $"{consumer.Address?.Street}, {consumer.Address?.City}, {consumer.Address?.State} {consumer.Address?.PostalCode}".Trim(' ', ','),
            Status = consumer.Status,
            IsActive = consumer.IsActive,
            Notes = consumer.Notes,
            CreatedBy = string.Empty, // Not available in domain model
            CreatedOn = consumer.CreatedAt,
            UpdatedBy = string.Empty, // Not available in domain model 
            UpdatedOn = null // Not available in domain model
        };
    }
    private static ConsumerDetailViewModel MapToConsumerDetailsViewModel(ConsumerAccount consumer,
    IEnumerable<ProductLicense> licenses, LicenseUsageStatistics statistics)
    {
        var licenseSummary = licenses.Select(l => new LicenseSummaryViewModel
        {
            LicenseId = l.LicenseId,
            LicenseCode = l.LicenseCode,
            ProductName = l.LicenseConsumer.Product?.Name ?? "Unknown Product",
            Tier = Core.Models.LicenseTier.Custom, // Assuming tier is not available in the license model
            Status = l.Status,
            ValidFrom = l.ValidFrom,
            ValidTo = l.ValidTo,
            CreatedAt = l.CreatedAt            
        }).ToList();        return new ConsumerDetailViewModel
        {
            Consumer = consumer,
            Licenses = licenseSummary,
            Statistics = new ConsumerStatisticsViewModel
            {
                TotalLicenses = statistics.TotalLicenses,
                ActiveLicenses = statistics.ActiveLicenses,
                ExpiredLicenses = statistics.ExpiredLicenses
                
            }
        
        };
    }
}
