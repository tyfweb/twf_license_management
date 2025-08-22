using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TechWayFit.Licensing.Management.Web.Attributes;

/// <summary>
/// Validation attribute to ensure a date is after another date property
/// </summary>
public class DateAfterAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateAfterAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime currentValue)
        {
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);
            if (comparisonValue is DateTime comparisonDate)
            {
                if (currentValue <= comparisonDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be after {_comparisonProperty}.");
                }
            }
        }

        return ValidationResult.Success;
    }
}
