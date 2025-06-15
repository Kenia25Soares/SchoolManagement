using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } 

        [Required]
        [StringLength(20)]
        public string Shift { get; set; }  // Turno

       
       /* public ICollection<Subject> Subjects { get; set; } */  // Relacionamento com as Disciplinas
    }
}
