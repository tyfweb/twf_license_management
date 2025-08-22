using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Attributes;

/// <summary>
/// Validation attribute to ensure a date is not in the past
/// </summary>
public class DateNotInPastAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime dateTime)
        {
            // Allow dates from today onwards
            return dateTime.Date >= DateTime.Now.Date;
        }
        
        // If value is null, let Required attribute handle it
        return value == null;
    }

    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"The {name} field cannot be in the past.";
    }
}
