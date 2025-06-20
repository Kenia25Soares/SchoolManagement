using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SubjectViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject name is required.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Workload is required.")]
        [Display(Name = "Workload (hours)")]
        public int Workload { get; set; }
    }
}
