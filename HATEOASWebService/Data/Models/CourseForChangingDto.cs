using System.ComponentModel.DataAnnotations;
using HATEOASWebService.ValidationAttributes;

namespace HATEOASWebService.Data.Models
{
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "Course Title must be different from the description.")]
    public abstract class CourseForChangingDto
    {
        [Required(ErrorMessage = "The Course Title is required.")]
        [MaxLength(100, ErrorMessage = "The Course Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The Course Description cannot exceed 1500 characters.")]
        public virtual string Description { get; set; }
    }
}
