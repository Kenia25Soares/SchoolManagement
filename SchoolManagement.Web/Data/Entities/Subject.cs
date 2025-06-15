using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class Subject : IEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Workload (hours)")]
        public int Workload { get; set; }
    }
}
