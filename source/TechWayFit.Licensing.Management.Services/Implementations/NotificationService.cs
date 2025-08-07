using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Services.Implementations;

/// <summary>
/// Implementation of notification service for license notifications and alerts
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger;
    }

    public async Task<bool> SendLicenseExpirationNotificationAsync(ProductLicense license, int daysUntilExpiry)
    {
        try
        {
            // Get expiration notification template
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.LicenseExpiration);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active expiration notification template found");
                return false;
            }

            // Create notification history entry
            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = license.LicenseId,
                EntityType = "License",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.LicenseExpiration,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("License expiration notification sent for license {LicenseId}", license.LicenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license expiration notification for license {LicenseId}", license.LicenseId);
            return false;
        }
    }

    public async Task<bool> SendLicenseActivationNotificationAsync(ProductLicense license, Dictionary<string, object> activationInfo)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.LicenseActivation);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active activation notification template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = license.LicenseId,
                EntityType = "License",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.LicenseActivation,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("License activation notification sent for license {LicenseId}", license.LicenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license activation notification for license {LicenseId}", license.LicenseId);
            return false;
        }
    }

    public async Task<bool> SendLicenseRevocationNotificationAsync(ProductLicense license, string reason)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.LicenseRevocation);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active revocation notification template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = license.LicenseId,
                EntityType = "License",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.LicenseRevocation,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("License revocation notification sent for license {LicenseId}", license.LicenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license revocation notification for license {LicenseId}", license.LicenseId);
            return false;
        }
    }

    public async Task<bool> SendLicenseRenewalNotificationAsync(ProductLicense license, DateTime previousExpiryDate)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.LicenseRenewal);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active renewal notification template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = license.LicenseId,
                EntityType = "License",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.LicenseRenewal,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("License renewal notification sent for license {LicenseId}", license.LicenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license renewal notification for license {LicenseId}", license.LicenseId);
            return false;
        }
    }

    public async Task<bool> SendLicenseSuspensionNotificationAsync(ProductLicense license, string reason, DateTime? suspensionEndDate)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.LicenseSuspension);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active suspension notification template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = license.LicenseId,
                EntityType = "License",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.LicenseSuspension,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("License suspension notification sent for license {LicenseId}", license.LicenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license suspension notification for license {LicenseId}", license.LicenseId);
            return false;
        }
    }

    public async Task<bool> SendBulkExpirationAlertAsync(IEnumerable<ProductLicense> expiringLicenses, int daysUntilExpiry)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.SystemAlert);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active bulk expiration alert template found");
                return false;
            }

            foreach (var license in expiringLicenses)
            {
                var notification = new NotificationHistory
                {
                    NotificationId = Guid.NewGuid(),
                    EntityId = license.LicenseId,
                    EntityType = "BulkAlert",
                    NotificationMode = NotificationMode.Email,
                    NotificationTemplateId = template.Id,
                    NotificationType = NotificationType.SystemAlert,
                    Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                    SentDate = DateTime.UtcNow,
                    DeliveryStatus = DeliveryStatus.Sent
                };

                var entity = NotificationHistoryEntity.FromModel(notification);
                await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            }
            
            _logger.LogInformation("Bulk expiration alert sent for {Count} licenses", expiringLicenses.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk expiration alert");
            return false;
        }
    }

    public async Task<bool> SendValidationFailureAlertAsync(Guid licenseId, string validationError, Dictionary<string, object> attemptInfo)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.ValidationFailure);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active validation failure alert template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = licenseId,
                EntityType = "ValidationFailure",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.ValidationFailure,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Validation failure alert sent for license key {LicenseId}", licenseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send validation failure alert for license key {LicenseId}", licenseId);
            return false;
        }
    }

    public async Task<bool> SendUsageThresholdAlertAsync(Guid productId, int currentUsage, int threshold)
    {
        try
        {
            var templates = await _unitOfWork.NotificationTemplates.GetByTypeAsync(NotificationType.UsageThreshold);
            var template = templates.FirstOrDefault(t => t.IsActive);
            
            if (template == null)
            {
                _logger.LogWarning("No active usage threshold alert template found");
                return false;
            }

            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = productId,
                EntityType = "Product",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = template.Id,
                NotificationType = NotificationType.UsageThreshold,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Usage threshold alert sent for product {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send usage threshold alert for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> SendCustomNotificationAsync(IEnumerable<string> recipients, string subject, string message, NotificationType notificationType, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var notification = new NotificationHistory
            {
                NotificationId = Guid.NewGuid(),
                EntityId = IdConstants.CustomNotificationEntityId,
                EntityType = "Custom",
                NotificationMode = NotificationMode.Email,
                NotificationTemplateId = Guid.NewGuid(),
                NotificationType = notificationType,
                Recipients = new NotificationPreferences { Mode = NotificationMode.Email },
                SentDate = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            };

            var entity = NotificationHistoryEntity.FromModel(notification);
            await _unitOfWork.NotificationHistory.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Custom notification sent to {Count} recipients", recipients.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send custom notification");
            return false;
        }
    }

    public async Task<IEnumerable<NotificationHistory>> GetLicenseNotificationHistoryAsync(Guid licenseId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var entities = await _unitOfWork.NotificationHistory.GetAllAsync(CancellationToken.None);
            var filtered = entities.Where(e => e.EntityId == licenseId);

            if (fromDate.HasValue)
                filtered = filtered.Where(e => e.SentDate >= fromDate.Value);

            if (toDate.HasValue)
                filtered = filtered.Where(e => e.SentDate <= toDate.Value);

            var paged = filtered
                .OrderByDescending(e => e.SentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return paged.Select(e => e.ToModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification history for license {LicenseId}", licenseId);
            return Enumerable.Empty<NotificationHistory>();
        }
    }

    public async Task<IEnumerable<NotificationHistory>> GetConsumerNotificationHistoryAsync(Guid consumerId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var entities = await _unitOfWork.NotificationHistory.GetAllAsync(CancellationToken.None);
            var filtered = entities.Where(e => e.EntityId == consumerId);

            if (fromDate.HasValue) 
                filtered = filtered.Where(e => e.SentDate >= fromDate.Value);

            if (toDate.HasValue)
                filtered = filtered.Where(e => e.SentDate <= toDate.Value);

            var paged = filtered
                .OrderByDescending(e => e.SentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return paged.Select(e => e.ToModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification history for consumer {ConsumerId}", consumerId);
            return Enumerable.Empty<NotificationHistory>();
        }
    }

    public async Task<IEnumerable<NotificationTemplate>> GetNotificationTemplatesAsync(NotificationType? notificationType = null)
    {
        try
        {
            IEnumerable<NotificationTemplateEntity> entities;
            
            if (notificationType.HasValue)
            {
                entities = await _unitOfWork.NotificationTemplates.GetByTypeAsync(notificationType.Value);
            }
            else
            {
                entities = await _unitOfWork.NotificationTemplates.GetAllAsync(CancellationToken.None);
            }

            return entities.Select(e => e.ToModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification templates");
            return Enumerable.Empty<NotificationTemplate>();
        }
    }

    public async Task<NotificationTemplate> SaveNotificationTemplateAsync(NotificationTemplate template, string createdBy)
    {
        try
        {
            template.CreatedBy = createdBy;
            template.CreatedDate = DateTime.UtcNow;

            var entity = NotificationTemplateEntity.FromModel(template);
            
            // Check if template exists
            var existingTemplate = await _unitOfWork.NotificationTemplates.GetByIdAsync(template.TemplateId);
            if (existingTemplate != null)
            {
                await _unitOfWork.NotificationTemplates.UpdateAsync(entity);
            }
            else
            {
                template.TemplateId = Guid.NewGuid();
                entity.Id = template.TemplateId;
                await _unitOfWork.NotificationTemplates.AddAsync(entity);
            }

            _logger.LogInformation("Notification template saved: {TemplateId}", template.TemplateId);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save notification template");
            throw;
        }
    }    public Task<NotificationPreferences> GetNotificationPreferencesAsync(Guid consumerId)
    {
        try
        {
            // For now, return default preferences
            // In a real implementation, this would fetch from a preferences repository
            return Task.FromResult(new NotificationPreferences
            {
                Mode = NotificationMode.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification preferences for consumer {ConsumerId}", consumerId);
            return Task.FromResult(new NotificationPreferences());
        }
    }

    public Task<NotificationPreferences> UpdateNotificationPreferencesAsync(Guid consumerId, NotificationPreferences preferences)
    {
        try
        {
            // For now, just return the provided preferences
            // In a real implementation, this would save to a preferences repository
            _logger.LogInformation("Notification preferences updated for consumer {ConsumerId}", consumerId);
            return Task.FromResult(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification preferences for consumer {ConsumerId}", consumerId);
            throw;
        }
    }

    public async Task<NotificationStatistics> GetNotificationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var statistics = await _unitOfWork.NotificationHistory.GetStatisticsAsync(fromDate, toDate);
            
            return new NotificationStatistics
            {
                TotalNotifications = (int)statistics.GetValueOrDefault("TotalCount", 0),
                SuccessfulDeliveries = (int)statistics.GetValueOrDefault("SentCount", 0),
                FailedDeliveries = (int)statistics.GetValueOrDefault("FailedCount", 0),
                DeliverySuccessRate = CalculateSuccessRate(statistics),
                NotificationsByType = new Dictionary<NotificationType, int>(),
                NotificationsByDate = new Dictionary<DateTime, int>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification statistics");
            return new NotificationStatistics();
        }
    }

    private static double CalculateSuccessRate(Dictionary<string, object> statistics)
    {
        var totalCount = (int)statistics.GetValueOrDefault("TotalCount", 0);
        var sentCount = (int)statistics.GetValueOrDefault("SentCount", 0);
        
        return totalCount > 0 ? Math.Round((double)sentCount / totalCount * 100, 2) : 0.0;
    }

     
 
}
