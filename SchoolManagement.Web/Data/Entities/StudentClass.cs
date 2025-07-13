using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class StudentClass : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string AcademicYear { get; set; }
        [Required]
        public string Shift { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public bool IsClosed { get; set; }
        public ICollection<StudentProfile> Students { get; set; }
    }
}
