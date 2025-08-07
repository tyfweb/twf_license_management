using System.Text.Json;

namespace TechWayFit.Licensing.Management.Infrastructure.Helpers;

/// <summary>
/// Helper class for JSON operations
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(object? obj)
    {
        if (obj == null)
            return "{}";
        
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    public static T? FromJson<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return default(T);

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException)
        {
            return default(T);
        }
    }

    public static Dictionary<string, string> FromDictJson(string? json)
    {
        json ??= "{}";
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            var dictOfObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json, DefaultOptions);
            if (dictOfObj == null)
                return new Dictionary<string, string>();

            return dictOfObj.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }
}
