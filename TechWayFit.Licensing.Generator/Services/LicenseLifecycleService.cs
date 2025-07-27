using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Service for managing license lifecycle operations
    /// </summary>
    public class LicenseLifecycleService : ILicenseLifecycleService
    {
        private readonly ILicenseRepository _licenseRepository;
        private readonly IConsumerRepository _consumerRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly LicenseGenerator _licenseGenerator;
        private readonly ILogger<LicenseLifecycleService> _logger;

        public LicenseLifecycleService(
            ILicenseRepository licenseRepository,
            IConsumerRepository consumerRepository,
            IAuditRepository auditRepository,
            LicenseGenerator licenseGenerator,
            ILogger<LicenseLifecycleService> logger)
        {
            _licenseRepository = licenseRepository;
            _consumerRepository = consumerRepository;
            _auditRepository = auditRepository;
            _licenseGenerator = licenseGenerator;
            _logger = logger;
        }

        public async Task<SignedLicense> GenerateLicenseAsync(LicenseGenerationRequest request)
        {
            try
            {
                _logger.LogInformation("Generating license for {LicensedTo} on product {ProductId}", request.LicensedTo, request.ProductId);

                // Generate signed license using the license generator
                var signedLicense = await _licenseGenerator.GenerateLicenseAsync(request);

                // Decode the license data to get the license information
                var licenseDataJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(signedLicense.LicenseData));
                var generatedLicense = System.Text.Json.JsonSerializer.Deserialize<License>(licenseDataJson, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (generatedLicense == null)
                {
                    throw new InvalidOperationException("Failed to decode generated license data");
                }

                // Create the license for storage
                var license = new License
                {
                    ProductId = request.ProductId,
                    ProductType = request.ProductType,
                    ConsumerId = request.ConsumerId,
                    LicenseId = generatedLicense.LicenseId,
                    LicensedTo = request.LicensedTo,
                    ContactPerson = request.ContactPerson,
                    ContactEmail = request.ContactEmail,
                    SecondaryContactPerson = request.SecondaryContactPerson,
                    SecondaryContactEmail = request.SecondaryContactEmail,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    Status = LicenseStatus.Active,
                    Tier = request.Tier,
                    MaxApiCallsPerMonth = request.MaxApiCallsPerMonth,
                    MaxConcurrentConnections = request.MaxConcurrentConnections,
                    FeaturesIncluded = generatedLicense.FeaturesIncluded,
                    IssuedAt = generatedLicense.IssuedAt,
                    CreatedBy = request.CreatedBy ?? "System",
                    Metadata = request.Metadata ?? new Dictionary<string, string>()
                };

                // Save the license
                await _licenseRepository.SaveLicenseAsync(license, request.ProductId, request.ConsumerId);

                // Add audit entry
                await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                {
                    LicenseId = license.LicenseId,
                    ProductId = request.ProductId,
                    ConsumerId = request.ConsumerId,
                    Operation = LicenseOperation.Created,
                    Description = $"License created for {request.LicensedTo}",
                    PerformedBy = request.CreatedBy ?? "System",
                    NewStatus = LicenseStatus.Active
                });

                _logger.LogInformation("Successfully generated license {LicenseId}", license.LicenseId);
                return signedLicense;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate license for {LicensedTo}", request.LicensedTo);
                throw;
            }
        }

        public async Task<SignedLicense> RenewLicenseAsync(LicenseRenewalRequest request)
        {
            try
            {
                _logger.LogInformation("Renewing license {LicenseId}", request.CurrentLicenseId);

                var currentLicense = await _licenseRepository.GetLicenseByIdAsync(request.CurrentLicenseId);
                if (currentLicense == null)
                    throw new InvalidOperationException($"License {request.CurrentLicenseId} not found");

                // Create renewal request
                var renewalRequest = new LicenseGenerationRequest
                {
                    ProductId = request.ProductId,
                    ProductType = currentLicense.ProductType,
                    ConsumerId = request.ConsumerId,
                    LicensedTo = currentLicense.LicensedTo,
                    ContactPerson = currentLicense.ContactPerson,
                    ContactEmail = currentLicense.ContactEmail,
                    SecondaryContactPerson = currentLicense.SecondaryContactPerson,
                    SecondaryContactEmail = currentLicense.SecondaryContactEmail,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = request.NewExpiryDate,
                    Tier = request.NewTier,
                    MaxApiCallsPerMonth = currentLicense.MaxApiCallsPerMonth,
                    MaxConcurrentConnections = currentLicense.MaxConcurrentConnections,
                    CustomFeatures = request.FeatureChanges?.Select(f => new LicenseFeature { Name = f, IsEnabled = true }).ToList(),
                    CreatedBy = request.RequestedBy,
                    Metadata = request.Metadata
                };

                // Generate new license
                var newSignedLicense = await GenerateLicenseAsync(renewalRequest);

                // Update old license status
                await _licenseRepository.UpdateLicenseStatusAsync(request.CurrentLicenseId, LicenseStatus.Archived);

                // Add audit entries
                await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                {
                    LicenseId = request.CurrentLicenseId,
                    ProductId = request.ProductId,
                    ConsumerId = request.ConsumerId,
                    Operation = LicenseOperation.Archived,
                    Description = $"License archived during renewal",
                    PerformedBy = request.RequestedBy,
                    OldStatus = LicenseStatus.Active,
                    NewStatus = LicenseStatus.Archived
                });

                _logger.LogInformation("Successfully renewed license {LicenseId}", request.CurrentLicenseId);
                return newSignedLicense;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew license {LicenseId}", request.CurrentLicenseId);
                throw;
            }
        }

        public async Task<bool> RevokeLicenseAsync(LicenseRevocationRequest request)
        {
            try
            {
                _logger.LogInformation("Revoking license {LicenseId}", request.LicenseId);

                var license = await _licenseRepository.GetLicenseByIdAsync(request.LicenseId);
                if (license == null)
                    return false;

                var effectiveDate = request.EffectiveDate ?? DateTime.UtcNow;
                var newStatus = effectiveDate <= DateTime.UtcNow ? LicenseStatus.Revoked : LicenseStatus.RenewalPending;

                var success = await _licenseRepository.UpdateLicenseStatusAsync(request.LicenseId, newStatus);

                if (success)
                {
                    await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                    {
                        LicenseId = request.LicenseId,
                        ProductId = license.ProductId,
                        ConsumerId = license.ConsumerId,
                        Operation = LicenseOperation.Revoked,
                        Description = $"License revoked: {request.ReasonDescription}",
                        PerformedBy = request.RevokedBy,
                        NewStatus = newStatus,
                        AdditionalDetails = new Dictionary<string, string>
                        {
                            ["RevocationReason"] = request.Reason.ToString(),
                            ["EffectiveDate"] = effectiveDate.ToString("O")
                        }
                    });

                    _logger.LogInformation("Successfully revoked license {LicenseId}", request.LicenseId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke license {LicenseId}", request.LicenseId);
                return false;
            }
        }

        public async Task<bool> SuspendLicenseAsync(string licenseId, string reason, string suspendedBy)
        {
            try
            {
                var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
                if (license == null)
                    return false;

                var success = await _licenseRepository.UpdateLicenseStatusAsync(licenseId, LicenseStatus.Suspended);

                if (success)
                {
                    await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                    {
                        LicenseId = licenseId,
                        ProductId = license.ProductId,
                        ConsumerId = license.ConsumerId,
                        Operation = LicenseOperation.Suspended,
                        Description = $"License suspended: {reason}",
                        PerformedBy = suspendedBy,
                        NewStatus = LicenseStatus.Suspended
                    });
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to suspend license {LicenseId}", licenseId);
                return false;
            }
        }

        public async Task<bool> ReactivateLicenseAsync(string licenseId, string reactivatedBy)
        {
            try
            {
                var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
                if (license == null)
                    return false;

                var success = await _licenseRepository.UpdateLicenseStatusAsync(licenseId, LicenseStatus.Active);

                if (success)
                {
                    await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                    {
                        LicenseId = licenseId,
                        ProductId = license.ProductId,
                        ConsumerId = license.ConsumerId,
                        Operation = LicenseOperation.Activated,
                        Description = "License reactivated from suspension",
                        PerformedBy = reactivatedBy,
                        OldStatus = LicenseStatus.Suspended,
                        NewStatus = LicenseStatus.Active
                    });
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reactivate license {LicenseId}", licenseId);
                return false;
            }
        }

        public async Task<IEnumerable<ConsumerLicenseInfo>> GetLicensesExpiringWithinAsync(int days)
        {
            return await _licenseRepository.GetLicensesExpiringWithinAsync(days);
        }

        public async Task<int> ProcessExpiredLicensesAsync()
        {
            try
            {
                var expiredLicenses = await _licenseRepository.GetExpiredLicensesAsync();
                int processedCount = 0;

                foreach (var licenseInfo in expiredLicenses)
                {
                    var success = await _licenseRepository.UpdateLicenseStatusAsync(licenseInfo.LicenseId, LicenseStatus.Expired);
                    if (success)
                    {
                        await _auditRepository.AddAuditEntryAsync(new LicenseAuditEntry
                        {
                            LicenseId = licenseInfo.LicenseId,
                            ProductId = licenseInfo.ProductId,
                            ConsumerId = "", // This would need to be looked up
                            Operation = LicenseOperation.Modified,
                            Description = "License automatically expired",
                            PerformedBy = "System",
                            OldStatus = licenseInfo.Status,
                            NewStatus = LicenseStatus.Expired
                        });

                        processedCount++;
                    }
                }

                _logger.LogInformation("Processed {Count} expired licenses", processedCount);
                return processedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired licenses");
                return 0;
            }
        }

        public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseId)
        {
            // This would integrate with the validation service
            // For now, return a basic validation based on license data
            var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
            
            if (license == null)
            {
                return LicenseValidationResult.Failure(LicenseStatus.NotFound, "License not found");
            }

            if (license.Status == LicenseStatus.Revoked)
            {
                return LicenseValidationResult.Failure(LicenseStatus.Revoked, "License has been revoked");
            }

            if (license.ValidTo < DateTime.UtcNow)
            {
                return LicenseValidationResult.Failure(LicenseStatus.Expired, "License has expired");
            }

            return LicenseValidationResult.Success(license);
        }

        public async Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(string licenseId)
        {
            return await _auditRepository.GetAuditEntriesForLicenseAsync(licenseId);
        }

        public async Task<IEnumerable<ConsumerLicenseInfo>> GetLicensesByProductAsync(string productId)
        {
            try
            {
                // Use the license repository to get all licenses for the product
                return await _licenseRepository.GetLicensesByProductAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting licenses for product {ProductId}", productId);
                return Enumerable.Empty<ConsumerLicenseInfo>();
            }
        }

        private List<LicenseFeature> CreateFeaturesForTier(LicenseTier tier, List<string>? customFeatures = null)
        {
            // This would be replaced with product-specific feature configuration
            var features = new List<LicenseFeature>();

            switch (tier)
            {
                case LicenseTier.Community:
                    features.Add(new LicenseFeature { Name = "BasicRouting", IsEnabled = true });
                    break;
                case LicenseTier.Professional:
                    features.Add(new LicenseFeature { Name = "BasicRouting", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "LoadBalancing", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "BasicAuth", IsEnabled = true });
                    break;
                case LicenseTier.Enterprise:
                    features.Add(new LicenseFeature { Name = "BasicRouting", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "LoadBalancing", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "BasicAuth", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "AdvancedAuth", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "Monitoring", IsEnabled = true });
                    features.Add(new LicenseFeature { Name = "Analytics", IsEnabled = true });
                    break;
            }

            if (customFeatures != null)
            {
                foreach (var customFeature in customFeatures)
                {
                    if (!features.Any(f => f.Name == customFeature))
                    {
                        features.Add(new LicenseFeature { Name = customFeature, IsEnabled = true });
                    }
                }
            }

            return features;
        }
    }
}
