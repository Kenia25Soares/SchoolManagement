using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Course Name")]
        public string Name { get; set; }


        public int SubjectsCount { get; set; }


        [ValidateNever]
        public List<string> Subjects { get; set; } = new();

        
        [ValidateNever]
        public IEnumerable<SelectListItem> AvailableSubjects { get; set; } = new List<SelectListItem>();
    }
}
