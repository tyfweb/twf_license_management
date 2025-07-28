namespace TechWayFit.Licensing.Infrastructure.Models.Entities;

/// <summary>
/// Base entity class that provides audit fields for all database entities
/// </summary>
public abstract class BaseAuditEntity
{
    /// <summary>
    /// Is the entity active? This is used for soft deletes
    /// </summary>
    public bool IsActive { get; set; } = true;
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
            return default!; // Return default value for T if json is empty or whitespace
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        var dictOfObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
        if (dictOfObj == null)
            return new Dictionary<string, string>(); // Return empty dictionary if deserialization fails
        return dictOfObj.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);        
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
