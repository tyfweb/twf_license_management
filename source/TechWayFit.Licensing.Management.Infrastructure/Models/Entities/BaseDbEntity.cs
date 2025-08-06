using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

/// <summary>
/// Base entity class that provides audit fields for all database entities
/// </summary>
public abstract class BaseDbEntity
{
    /// <summary>
    /// Unique identifier for the entity (Primary Key)
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Is the entity active? This is used for soft deletes
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is the entity deleted? This is used for soft deletes
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// User who deleted the entity (for soft deletes)
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was deleted (for soft deletes)
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Workflow status of the entity
    /// </summary>
    public int EntityStatus { get; set; } = 0; // Default to Draft

    /// <summary>
    /// User who submitted the entity for approval
    /// </summary>
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was submitted for approval
    /// </summary>
    public DateTime? SubmittedOn { get; set; }

    /// <summary>
    /// User who approved/rejected the entity
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was reviewed
    /// </summary>
    public DateTime? ReviewedOn { get; set; }

    /// <summary>
    /// Approval comments or rejection reason
    /// </summary>
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    protected static string ToJson(object obj)
    {
        if (obj == null)
            return "{}"; // Return empty JSON if obj is null
        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        return System.Text.Json.JsonSerializer.Serialize(obj, options);
    }
    protected static T FromJson<T>(string? json)
    {
        json ??= "{}"; // Default to empty JSON if null
        if (string.IsNullOrWhiteSpace(json))
            return default!; // Return default value for T if json is empty or whitespace
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, options) ?? throw new InvalidOperationException("Deserialization failed");
    }
    protected static Dictionary<string, string> FromDictJson(string? json)
    {
        json ??= "{}"; // Default to empty JSON if null
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>(); // Return empty dictionary for empty/null json

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var dictOfObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
            if (dictOfObj == null)
                return new Dictionary<string, string>(); // Return empty dictionary if deserialization fails

            return dictOfObj.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
        }
        catch (System.Text.Json.JsonException)
        {
            // If JSON deserialization fails, return empty dictionary
            return new Dictionary<string, string>();
        }
    }

    public static T ToEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return default; // Return default value if null or empty

        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        throw new ArgumentException($"Invalid value '{value}' for enum type '{typeof(T).Name}'", nameof(value));
    }
    public static string ToStringEnum<T>(T value) where T : struct, Enum
    {
        return value.ToString();
    }
}
