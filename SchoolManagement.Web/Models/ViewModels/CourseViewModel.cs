using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Academic Year is required.")]
        [StringLength(50)]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "Shift is required.")]
        [StringLength(20)]
        [Display(Name = "Shift")]
        public string Shift { get; set; }
    }
}
