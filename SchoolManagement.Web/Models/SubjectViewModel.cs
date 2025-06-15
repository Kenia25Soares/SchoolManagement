using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SubjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Workload (hours)")]
        [Range(1, 500, ErrorMessage = "Please enter a valid workload in hours.")]
        public int Workload { get; set; }
    }
}
