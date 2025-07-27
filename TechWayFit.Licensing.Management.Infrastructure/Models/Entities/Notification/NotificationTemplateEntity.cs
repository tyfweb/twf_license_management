using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.Notification;

/// <summary>
/// Database entity for Notification Templates
/// </summary>
public class NotificationTemplateEntity : BaseAuditEntity
{
    public string NotificationTemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string NotificationMode { get; set; } = string.Empty; 
    public string Subject { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string TemplateVariableJson { get; set; } = "{}";


    /// <summary>
    /// Navigation property to Notification History
    /// </summary>
    public virtual ICollection<NotificationHistoryEntity> NotificationHistory { get; set; } = new List<NotificationHistoryEntity>();

    public static NotificationTemplateEntity FromModel(NotificationTemplate model)
    {
        return new NotificationTemplateEntity
        {
            NotificationTemplateId = model.TemplateId,
            TemplateName = model.TemplateName,
            NotificationType = model.NotificationType.ToString(),
            NotificationMode = model.Preferences.Mode.ToString(), 
            Subject = model.Subject,
            MessageTemplate = model.MessageTemplate,
            IsActive = model.IsActive,
            TemplateVariableJson = ToJson(model.TemplateVariables),
            CreatedBy = model.CreatedBy,
            CreatedOn = model.CreatedDate,
            UpdatedBy = "system", // Assuming system user for creation
            UpdatedOn = DateTime.UtcNow, // Assuming UpdatedOn is set to current time on creation
        };
    }
    public NotificationTemplate ToModel()
    {
        return new NotificationTemplate
        {
            TemplateId = NotificationTemplateId,
            TemplateName = TemplateName,
            NotificationType = NotificationType.ToEnum<NotificationType>(),
            Preferences = new NotificationPreferences
            {
                Mode = NotificationMode.ToEnum<NotificationMode>()
            },
            Subject = Subject,
            MessageTemplate = MessageTemplate,
            IsActive = IsActive,
            CreatedBy = CreatedBy,
            CreatedDate = CreatedOn,
            TemplateVariables = FromJson<Dictionary<string, object>>(TemplateVariableJson)
        };
    }   

}
