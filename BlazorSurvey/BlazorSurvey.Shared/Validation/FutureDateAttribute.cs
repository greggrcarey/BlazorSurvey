using System.ComponentModel.DataAnnotations;

namespace BlazorSurvey.Shared.Validation;

internal class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset <= DateTimeOffset.UtcNow ? new ValidationResult("The date must be in the future") : ValidationResult.Success;
        }
        return new ValidationResult("Invalid date format");
    }

}
