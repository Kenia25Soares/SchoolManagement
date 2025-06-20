using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentClassViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Class Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic Year is required.")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shift is required.")]
        [Display(Name = "Shift")]
        public string Shift { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course is required.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
    }
}
