public class JsonResponse
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Contains any error messages if the request failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Contains the data returned by the API
    /// </summary>
    public object? Data { get; set; }

    public static JsonResponse OK(object? data = null, string? message = null)
    {
        return new JsonResponse
        {
            Success = true,
            Data = data,
            Message = message
        };
    }
    public static JsonResponse Error(string message, List<string>? errors = null)
    {
        return new JsonResponse
        {
            Success = false,
            Message = message,
            Data = errors
        };
    }
}