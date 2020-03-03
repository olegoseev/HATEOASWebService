using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HATEOASWebService.ValidationAttributes;

namespace HATEOASWebService.Data.Models
{
    [AuthorFirstNameMustBeDifferentFromLastName]
    public class AuthorForCreationDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        [MaxLength(50)]
        public string MainCategory { get; set; }

        public ICollection<CourseForCreationDto> Courses { get; set; } = new List<CourseForCreationDto>();
    }
}
