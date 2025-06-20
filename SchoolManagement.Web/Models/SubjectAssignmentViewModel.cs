using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SubjectAssignmentViewModel
    {
        [Required]
        public int SubjectId { get; set; }

        [Display(Name = "Subject Name")]
        public string SubjectName { get; set; } = null!;

        [Display(Name = "Assigned")]
        public bool IsAssigned { get; set; }
    }
}
