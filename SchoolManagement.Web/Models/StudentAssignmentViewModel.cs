using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentAssignmentViewModel
    {
        [Required]
        public string StudentId { get; set; } = null!;

        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = null!;

        [Display(Name = "Assigned")]
        public bool IsAssigned { get; set; }
    }
}
