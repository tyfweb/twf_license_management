using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Generator.Models;

namespace TechWayFit.Licensing.Management.Core.Extensions;

/// <summary>
/// Extension methods for managing license key parameters in SimplifiedLicenseGenerationRequest
/// </summary>
public static class LicenseParameterExtensions
{
    /// <summary>
    /// Adds a license parameter to the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to add</param>
    /// <param name="value">The value for the parameter</param>
    /// <returns>The same request instance for method chaining</returns>
    public static SimplifiedLicenseGenerationRequest AddLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        object value)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(value);

        // Initialize CustomData if it's null
        request.CustomData ??= new Dictionary<string, object>();

        // Add or update the parameter
        request.CustomData[parameter.ToString()] = value;

        return request;
    }

    /// <summary>
    /// Adds a license parameter with a string value to the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to add</param>
    /// <param name="value">The string value for the parameter</param>
    /// <returns>The same request instance for method chaining</returns>
    public static SimplifiedLicenseGenerationRequest AddLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        string value)
    {
        return request.AddLicenseParameter(parameter, (object)value);
    }

    /// <summary>
    /// Adds a license parameter with a boolean value to the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to add</param>
    /// <param name="value">The boolean value for the parameter</param>
    /// <returns>The same request instance for method chaining</returns>
    public static SimplifiedLicenseGenerationRequest AddLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        bool value)
    {
        return request.AddLicenseParameter(parameter, value.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Adds a license parameter with an integer value to the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to add</param>
    /// <param name="value">The integer value for the parameter</param>
    /// <returns>The same request instance for method chaining</returns>
    public static SimplifiedLicenseGenerationRequest AddLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        int value)
    {
        return request.AddLicenseParameter(parameter, value.ToString());
    }

    /// <summary>
    /// Gets a license parameter value from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to retrieve</param>
    /// <returns>The parameter value as an object, or null if not found</returns>
    public static object? GetLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomData == null)
            return null;

        return request.CustomData.TryGetValue(parameter.ToString(), out var value) ? value : null;
    }

    /// <summary>
    /// Gets a license parameter value as a string from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to retrieve</param>
    /// <param name="defaultValue">Default value to return if parameter is not found</param>
    /// <returns>The parameter value as a string, or the default value if not found</returns>
    public static string GetLicenseParameterAsString(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        string defaultValue = "")
    {
        var value = request.GetLicenseParameter(parameter);
        return value?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Gets a license parameter value as a boolean from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to retrieve</param>
    /// <param name="defaultValue">Default value to return if parameter is not found</param>
    /// <returns>The parameter value as a boolean, or the default value if not found</returns>
    public static bool GetLicenseParameterAsBool(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        bool defaultValue = false)
    {
        var value = request.GetLicenseParameterAsString(parameter);
        
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a license parameter value as an integer from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to retrieve</param>
    /// <param name="defaultValue">Default value to return if parameter is not found</param>
    /// <returns>The parameter value as an integer, or the default value if not found</returns>
    public static int GetLicenseParameterAsInt(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter, 
        int defaultValue = 0)
    {
        var value = request.GetLicenseParameterAsString(parameter);
        
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Checks if a license parameter exists in the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to check</param>
    /// <returns>True if the parameter exists, false otherwise</returns>
    public static bool HasLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.CustomData?.ContainsKey(parameter.ToString()) ?? false;
    }

    /// <summary>
    /// Removes a license parameter from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <param name="parameter">The parameter to remove</param>
    /// <returns>The same request instance for method chaining</returns>
    public static SimplifiedLicenseGenerationRequest RemoveLicenseParameter(
        this SimplifiedLicenseGenerationRequest request, 
        LicenseKeyParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomData != null)
        {
            request.CustomData.Remove(parameter.ToString());
        }

        return request;
    }

    /// <summary>
    /// Gets all license parameters from the CustomData dictionary
    /// </summary>
    /// <param name="request">The license generation request</param>
    /// <returns>A dictionary of all license parameters and their values</returns>
    public static Dictionary<LicenseKeyParameter, object> GetAllLicenseParameters(
        this SimplifiedLicenseGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new Dictionary<LicenseKeyParameter, object>();

        if (request.CustomData == null)
            return result;

        foreach (var kvp in request.CustomData)
        {
            if (Enum.TryParse<LicenseKeyParameter>(kvp.Key, out var parameter))
            {
                result[parameter] = kvp.Value;
            }
        }

        return result;
    }
}
