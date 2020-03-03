using System.ComponentModel.DataAnnotations;
using HATEOASWebService.Data.Models;

namespace HATEOASWebService.ValidationAttributes
{
    public sealed class AuthorFirstNameMustBeDifferentFromLastName : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var author = (AuthorForCreationDto)value;

            if (author.FirstName == author.LastName)
            {
                return new ValidationResult(
                    string.IsNullOrWhiteSpace(ErrorMessage) ? "Authors FirstName and LastName cannot be same" : ErrorMessage,
                    new[] { nameof(AuthorForCreationDto) });
            }

            return ValidationResult.Success;
        }
    }
}
