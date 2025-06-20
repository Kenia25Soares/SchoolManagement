using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentClassViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Academic Year is required")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "Shift is required")]
        public string Shift { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [ValidateNever]
        public string CourseName { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> Courses { get; set; }
    }
}
