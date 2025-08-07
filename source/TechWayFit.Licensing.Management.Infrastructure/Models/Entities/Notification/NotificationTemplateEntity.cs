using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.Helpers;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Notification;

/// <summary>
/// Database entity for Notification Templates
/// </summary>
[Table("notification_templates")]
public class NotificationTemplateEntity : AuditEntity,IEntityMapper<NotificationTemplate, NotificationTemplateEntity>
{
    public string TemplateName { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string NotificationMode { get; set; } = string.Empty; 
    public string Subject { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty; 
    public string TemplateVariableJson { get; set; } = "{}";


    /// <summary>
    /// Navigation property to Notification History
    /// </summary>
    public virtual ICollection<NotificationHistoryEntity> NotificationHistory { get; set; } = new List<NotificationHistoryEntity>();

#region IEntityMapper Implementation
    public NotificationTemplateEntity Map(NotificationTemplate model)
    {
        return new NotificationTemplateEntity
        {
            Id = model.TemplateId,
            TemplateName = model.TemplateName,
            NotificationType = model.NotificationType.ToString(),
            NotificationMode = model.Preferences.Mode.ToString(),
            Subject = model.Subject,
            MessageTemplate = model.MessageTemplate,
            IsActive = model.IsActive,
            TemplateVariableJson = JsonHelper.ToJson(model.TemplateVariables),
            CreatedBy = model.CreatedBy,
            CreatedOn = model.CreatedDate,
            UpdatedBy = "system", // Assuming system user for creation
            UpdatedOn = DateTime.UtcNow, // Assuming UpdatedOn is set to current time on creation
        };
    }
    public NotificationTemplate Map()
    {
        return new NotificationTemplate
        {
            TemplateId = Id,
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
            TemplateVariables = JsonHelper.FromJson<Dictionary<string, object>>(TemplateVariableJson)?? new Dictionary<string, object>()
        };
    }   
#endregion
}
