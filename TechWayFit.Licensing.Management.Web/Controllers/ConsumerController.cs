using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.WebUI.ViewModels.Consumer;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// Controller for consumer management
    /// </summary>
    [Authorize]
    public class ConsumerController : Controller
    {
        private readonly IConsumerService _consumerService;
        private readonly ILicenseLifecycleService _licenseService;
        private readonly ILogger<ConsumerController> _logger;

        public ConsumerController(
            IConsumerService consumerService,
            ILicenseLifecycleService licenseService,
            ILogger<ConsumerController> logger)
        {
            _consumerService = consumerService;
            _licenseService = licenseService;
            _logger = logger;
        }

        /// <summary>
        /// List all consumers with search and filtering
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            try
            {
                // Get consumers based on search term
                var allConsumers = string.IsNullOrEmpty(searchTerm) 
                    ? await GetAllConsumersAsync() 
                    : await _consumerService.SearchConsumersAsync(searchTerm);

                var consumerViewModels = new List<ConsumerViewModel>();

                foreach (var consumer in allConsumers)
                {
                    // Get license statistics for each consumer
                    var licenses = await _consumerService.GetConsumerLicensesAsync(consumer.ConsumerId);
                    var activeLicenses = licenses.Count(l => l.Status == LicenseStatus.Active);

                    consumerViewModels.Add(new ConsumerViewModel
                    {
                        ConsumerId = consumer.ConsumerId,
                        OrganizationName = consumer.OrganizationName,
                        ContactPerson = consumer.ContactPerson,
                        ContactEmail = consumer.ContactEmail,
                        IsActive = consumer.IsActive,
                        CreatedAt = consumer.CreatedAt,
                        TotalLicenses = licenses.Count(),
                        ActiveLicenses = activeLicenses
                    });
                }

                // Apply pagination
                var totalCount = consumerViewModels.Count;
                var paginatedConsumers = consumerViewModels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new ConsumerListViewModel
                {
                    SearchTerm = searchTerm,
                    Consumers = paginatedConsumers,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consumers with search term: {SearchTerm}", searchTerm);
                TempData["Error"] = "Failed to load consumers";
                return View(new ConsumerListViewModel { SearchTerm = searchTerm });
            }
        }

        /// <summary>
        /// Helper method to get all consumers across all products
        /// </summary>
        private async Task<IEnumerable<Consumer>> GetAllConsumersAsync()
        {
            try
            {
                // Since we don't have a GetAllConsumersAsync method, we'll use search with empty term
                // or implement a way to aggregate consumers from all products
                return await _consumerService.SearchConsumersAsync("");
            }
            catch
            {
                return new List<Consumer>();
            }
        }

        /// <summary>
        /// Show consumer details with license history
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var consumer = await _consumerService.GetConsumerAsync(id);
                if (consumer == null)
                {
                    TempData["Error"] = "Consumer not found";
                    return NotFound();
                }

                // Get consumer licenses
                var licenses = await _consumerService.GetConsumerLicensesAsync(id);

                // Build license summaries
                var licenseSummaries = licenses.Select(l => new LicenseSummaryViewModel
                {
                    LicenseId = l.LicenseId,
                    ProductName = l.ProductId, // You might want to resolve product name
                    Tier = l.Tier,
                    Status = l.Status,
                    ValidFrom = l.ValidFrom,
                    ValidTo = l.ValidTo,
                    CreatedAt = l.CreatedAt
                }).ToList();

                // Calculate statistics
                var statistics = new ConsumerStatisticsViewModel
                {
                    TotalLicenses = licenses.Count(),
                    ActiveLicenses = licenses.Count(l => l.Status == LicenseStatus.Active),
                    ExpiredLicenses = licenses.Count(l => l.Status == LicenseStatus.Expired),
                    FirstLicense = licenses.Any() ? licenses.Min(l => l.CreatedAt) : null,
                    LastActivity = licenses.Any() ? licenses.Max(l => l.UpdatedAt) : null
                };

                var viewModel = new ConsumerDetailViewModel
                {
                    Consumer = consumer,
                    Licenses = licenseSummaries,
                    Statistics = statistics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consumer details for {ConsumerId}", id);
                TempData["Error"] = "Failed to load consumer details";
                return NotFound();
            }
        }

        /// <summary>
        /// Show consumer creation form
        /// </summary>
        public IActionResult Create()
        {
            var viewModel = new ConsumerCreateViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// Handle consumer creation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsumerCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var consumer = new Consumer
                {
                    ConsumerId = Guid.NewGuid().ToString(),
                    OrganizationName = model.OrganizationName,
                    ContactPerson = model.ContactPerson,
                    ContactEmail = model.ContactEmail,
                    PhoneNumber = model.ContactPhone,
                    Address = !string.IsNullOrEmpty(model.Address) 
                        ? $"{model.Address}, {model.City}, {model.State} {model.PostalCode}, {model.Country}".Trim(' ', ',')
                        : string.Empty,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add metadata for notes if provided
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    consumer.Metadata.Add("Notes", model.Notes);
                }

                var createdConsumer = await _consumerService.CreateConsumerAsync(consumer);
                
                TempData["Success"] = $"Consumer '{createdConsumer.OrganizationName}' created successfully";
                return RedirectToAction(nameof(Details), new { id = createdConsumer.ConsumerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consumer for organization {OrganizationName}", model.OrganizationName);
                TempData["Error"] = "Failed to create consumer";
                return View(model);
            }
        }

        /// <summary>
        /// Show consumer edit form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var consumer = await _consumerService.GetConsumerAsync(id);
                if (consumer == null)
                {
                    TempData["Error"] = "Consumer not found";
                    return NotFound();
                }

                // Parse address components if available
                var addressParts = consumer.Address?.Split(',').Select(p => p.Trim()).ToArray() ?? new string[0];

                var viewModel = new ConsumerEditViewModel
                {
                    ConsumerId = consumer.ConsumerId,
                    OrganizationName = consumer.OrganizationName,
                    ContactPerson = consumer.ContactPerson,
                    ContactEmail = consumer.ContactEmail,
                    ContactPhone = consumer.PhoneNumber ?? string.Empty,
                    Address = addressParts.Length > 0 ? addressParts[0] : string.Empty,
                    City = addressParts.Length > 1 ? addressParts[1] : string.Empty,
                    State = addressParts.Length > 2 ? addressParts[2].Split(' ')[0] : string.Empty,
                    PostalCode = addressParts.Length > 2 && addressParts[2].Contains(' ') 
                        ? addressParts[2].Split(' ')[1] : string.Empty,
                    Country = addressParts.Length > 3 ? addressParts[3] : string.Empty,
                    IsActive = consumer.IsActive,
                    Notes = consumer.Metadata.ContainsKey("Notes") ? consumer.Metadata["Notes"] : string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consumer {ConsumerId} for edit", id);
                TempData["Error"] = "Failed to load consumer for editing";
                return NotFound();
            }
        }

        /// <summary>
        /// Handle consumer update
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ConsumerEditViewModel model)
        {
            if (string.IsNullOrEmpty(id) || !ModelState.IsValid)
                return View(model);

            try
            {
                var existingConsumer = await _consumerService.GetConsumerAsync(id);
                if (existingConsumer == null)
                {
                    TempData["Error"] = "Consumer not found";
                    return NotFound();
                }

                // Update consumer properties
                existingConsumer.OrganizationName = model.OrganizationName;
                existingConsumer.ContactPerson = model.ContactPerson;
                existingConsumer.ContactEmail = model.ContactEmail;
                existingConsumer.PhoneNumber = model.ContactPhone;
                existingConsumer.Address = !string.IsNullOrEmpty(model.Address) 
                    ? $"{model.Address}, {model.City}, {model.State} {model.PostalCode}, {model.Country}".Trim(' ', ',')
                    : string.Empty;
                existingConsumer.IsActive = model.IsActive;
                existingConsumer.UpdatedAt = DateTime.UtcNow;

                // Update metadata for notes
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    existingConsumer.Metadata["Notes"] = model.Notes;
                }
                else if (existingConsumer.Metadata.ContainsKey("Notes"))
                {
                    existingConsumer.Metadata.Remove("Notes");
                }

                var updatedConsumer = await _consumerService.UpdateConsumerAsync(existingConsumer);
                
                TempData["Success"] = $"Consumer '{updatedConsumer.OrganizationName}' updated successfully";
                return RedirectToAction(nameof(Details), new { id = updatedConsumer.ConsumerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating consumer {ConsumerId}", id);
                TempData["Error"] = "Failed to update consumer";
                return View(model);
            }
        }

        /// <summary>
        /// Handle consumer deletion (soft delete by setting IsActive = false)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var consumer = await _consumerService.GetConsumerAsync(id);
                if (consumer == null)
                {
                    TempData["Error"] = "Consumer not found";
                    return RedirectToAction(nameof(Index));
                }

                // Check if consumer has active licenses
                var licenses = await _consumerService.GetConsumerLicensesAsync(id);
                var activeLicenses = licenses.Where(l => l.Status == LicenseStatus.Active).ToList();

                if (activeLicenses.Any())
                {
                    TempData["Error"] = $"Cannot delete consumer '{consumer.OrganizationName}' because they have {activeLicenses.Count} active license(s). Please revoke active licenses first.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Perform soft delete by setting IsActive = false
                consumer.IsActive = false;
                consumer.UpdatedAt = DateTime.UtcNow;

                // Add deletion metadata
                consumer.Metadata["DeletedAt"] = DateTime.UtcNow.ToString("O");
                consumer.Metadata["DeletedBy"] = User.Identity?.Name ?? "System";

                await _consumerService.UpdateConsumerAsync(consumer);
                
                TempData["Success"] = $"Consumer '{consumer.OrganizationName}' has been deactivated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consumer {ConsumerId}", id);
                TempData["Error"] = "Failed to delete consumer";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
