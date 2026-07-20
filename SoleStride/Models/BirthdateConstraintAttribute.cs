using System.ComponentModel.DataAnnotations;

namespace SoleStride.Models
{
    internal class BirthdateConstraintAttribute : ValidationAttribute
    {
        //Minimum age 14
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime birthdate)
            {
                var age = DateTime.Now.Year - birthdate.Year;
                if (age < 14)
                {
                    return new ValidationResult("You must be at least 14 years old.");
                }
            }
            return ValidationResult.Success;
        }
    }
}