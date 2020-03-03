using System.ComponentModel.DataAnnotations;

namespace HATEOASWebService.Data.Models
{
    public class CourseForUpdateDto : CourseForChangingDto
    {
        [Required(ErrorMessage = "Description is required")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
