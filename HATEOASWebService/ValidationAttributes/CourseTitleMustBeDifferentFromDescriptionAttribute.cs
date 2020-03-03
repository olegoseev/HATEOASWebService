using System.ComponentModel.DataAnnotations;
using HATEOASWebService.Data.Models;

namespace HATEOASWebService.ValidationAttributes
{
    public sealed class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (CourseForChangingDto)value;

            if (course.Title == course.Description)
            {
                return new ValidationResult(
                    string.IsNullOrWhiteSpace(ErrorMessage) ? "The provided description should be different from the title" : ErrorMessage,
                    new[] { nameof(CourseForChangingDto) });
            }

            return ValidationResult.Success;
        }
    }
}
