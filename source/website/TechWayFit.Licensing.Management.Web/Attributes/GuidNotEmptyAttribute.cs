using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Attributes;

/// <summary>
/// Validation attribute to ensure a string can be parsed as a GUID and is not empty
/// </summary>
public class GuidNotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return false;
                
            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid != Guid.Empty;
            }
            return false;
        }
        
        if (value is Guid guidValue)
        {
            return guidValue != Guid.Empty;
        }
        
        // If value is null, let Required attribute handle it
        return value == null;
    }

    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"The {name} field must be a valid, non-empty identifier.";
    }
}
