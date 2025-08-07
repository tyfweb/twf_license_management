using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow; 
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Services.Implementations.Workflow;

/// <summary>
/// Helper class for JSON operations
/// </summary>
internal static class JsonHelper
{
    internal static string ToJson(object obj)
    {
        if (obj == null)
            return "{}";
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(obj, options);
    }

    internal static Dictionary<string, string> FromDictJson(string? json)
    {
        json ??= "{}";
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var dictOfObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
            if (dictOfObj == null)
                return new Dictionary<string, string>();

            return dictOfObj.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }

    internal static T ToEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        return default;
    }
}

/// <summary>
/// Workflow service implementation for ConsumerAccount
/// </summary>
public class ConsumerAccountWorkflowService : WorkflowService<ConsumerAccount, ConsumerAccountEntity>, IConsumerAccountWorkflowService
{
    public ConsumerAccountWorkflowService(
        IApprovalRepository<ConsumerAccountEntity> repository,
        IWorkflowHistoryRepository historyRepository,
        ILogger<WorkflowService<ConsumerAccount, ConsumerAccountEntity>> logger)
        : base(repository, historyRepository, logger, ToModel, ToEntity)
    {
    }


}

/// <summary>
/// Workflow service implementation for EnterpriseProduct
/// </summary>
public class EnterpriseProductWorkflowService : WorkflowService<EnterpriseProduct, ProductEntity>, IEnterpriseProductWorkflowService
{
    public EnterpriseProductWorkflowService(
        IApprovalRepository<ProductEntity> repository,
        IWorkflowHistoryRepository historyRepository,
        ILogger<WorkflowService<EnterpriseProduct, ProductEntity>> logger)
        : base(repository, historyRepository, logger, ToModel, ToEntity)
    {
    }

    private static EnterpriseProduct ToModel(ProductEntity entity)
    {
        return new EnterpriseProduct
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ReleaseDate = entity.ReleaseDate,
            SupportEmail = entity.SupportEmail,
            SupportPhone = entity.SupportPhone,
            DecommissionDate = entity.DecommissionDate,
            Status = JsonHelper.ToEnum<ProductStatus>(entity.Status),
            Metadata = JsonHelper.FromDictJson(entity.MetadataJson),
            
            // Use composition objects for audit properties
            Audit = new Core.Models.Common.AuditInfo
            {
                IsActive = entity.IsActive,
                IsDeleted = entity.IsDeleted,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedBy = entity.UpdatedBy,
                UpdatedOn = entity.UpdatedOn,
                DeletedBy = entity.DeletedBy,
                DeletedOn = entity.DeletedOn,
                RowVersion = entity.RowVersion
            },
            
            // Use composition object for workflow properties (ProductEntity has workflow fields)
            Workflow = new Core.Models.Common.WorkflowInfo
            {
                Status = (Core.Models.Common.EntityStatus)entity.EntityStatus,
                SubmittedBy = entity.SubmittedBy,
                SubmittedOn = entity.SubmittedOn,
                ReviewedBy = entity.ReviewedBy,
                ReviewedOn = entity.ReviewedOn,
                ReviewComments = entity.ReviewComments
            }
        };
    }

    private static ProductEntity ToEntity(EnterpriseProduct model)
    {
        return new ProductEntity
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ReleaseDate = model.ReleaseDate,
            SupportEmail = model.SupportEmail,
            SupportPhone = model.SupportPhone,
            DecommissionDate = model.DecommissionDate,
            Status = model.Status.ToString(),
            MetadataJson = JsonHelper.ToJson(model.Metadata),
            
            // Map audit properties from composition object
            IsActive = model.Audit.IsActive,
            IsDeleted = model.Audit.IsDeleted,
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn,
            DeletedBy = model.Audit.DeletedBy,
            DeletedOn = model.Audit.DeletedOn,
            RowVersion = model.Audit.RowVersion,
            
            // Map workflow properties from composition object (ProductEntity has workflow fields)
            EntityStatus = (int)model.Workflow.Status,
            SubmittedBy = model.Workflow.SubmittedBy,
            SubmittedOn = model.Workflow.SubmittedOn,
            ReviewedBy = model.Workflow.ReviewedBy,
            ReviewedOn = model.Workflow.ReviewedOn,
            ReviewComments = model.Workflow.ReviewComments
        };
    }
}

/// <summary>
/// Workflow service implementation for ProductLicense
/// </summary>
public class ProductLicenseWorkflowService : WorkflowService<ProductLicense, ProductLicenseEntity>, IProductLicenseWorkflowService
{
    public ProductLicenseWorkflowService(
        IApprovalRepository<ProductLicenseEntity> repository,
        IWorkflowHistoryRepository historyRepository,
        ILogger<WorkflowService<ProductLicense, ProductLicenseEntity>> logger)
        : base(repository, historyRepository, logger, ToModel, ToEntity)
    {
    }

    private static ProductLicense ToModel(ProductLicenseEntity entity)
    {
        return new ProductLicense
        {
            Id = entity.Id,
            LicenseCode = entity.LicenseCode,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            ValidProductVersionFrom = entity.ValidProductVersionFrom,
            ValidProductVersionTo = entity.ValidProductVersionTo,
            Encryption = entity.Encryption,
            Signature = entity.Signature,
            LicenseKey = entity.LicenseKey,
            PublicKey = entity.PublicKey,
            LicenseSignature = entity.LicenseSignature,
            KeyGeneratedAt = entity.KeyGeneratedAt,
            // For now, use the Core model Status directly since enum mapping is complex
            IssuedBy = entity.IssuedBy,
            
            // Base audit properties
            EntityStatus = (Core.Models.Common.EntityStatus)entity.EntityStatus,
            SubmittedBy = entity.SubmittedBy,
            SubmittedOn = entity.SubmittedOn,
            ReviewedBy = entity.ReviewedBy,
            ReviewedOn = entity.ReviewedOn,
            ReviewComments = entity.ReviewComments,
            
            CreatedBy = entity.CreatedBy,
            CreatedOn = entity.CreatedOn,
            UpdatedBy = entity.UpdatedBy,
            UpdatedOn = entity.UpdatedOn,
            IsActive = entity.IsActive,
            RowVersion = entity.RowVersion
        };
    }

    private static ProductLicenseEntity ToEntity(ProductLicense model)
    {
        return new ProductLicenseEntity
        {
            Id = model.Id,
            LicenseCode = model.LicenseCode,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            ValidProductVersionFrom = model.ValidProductVersionFrom,
            ValidProductVersionTo = model.ValidProductVersionTo,
            Encryption = model.Encryption,
            Signature = model.Signature,
            LicenseKey = model.LicenseKey,
            PublicKey = model.PublicKey,
            LicenseSignature = model.LicenseSignature,
            KeyGeneratedAt = model.KeyGeneratedAt,
            Status = model.Status.ToString(),
            IssuedBy = model.IssuedBy,
            
            // Base audit properties
            EntityStatus = (int)model.EntityStatus,
            SubmittedBy = model.SubmittedBy,
            SubmittedOn = model.SubmittedOn,
            ReviewedBy = model.ReviewedBy,
            ReviewedOn = model.ReviewedOn,
            ReviewComments = model.ReviewComments,
            
            CreatedBy = model.CreatedBy,
            CreatedOn = model.CreatedOn,
            UpdatedBy = model.UpdatedBy,
            UpdatedOn = model.UpdatedOn,
            IsActive = model.IsActive,
            RowVersion = model.RowVersion
        };
    }
}
