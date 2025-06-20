using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Um curso pode ter várias turmas (StudentClasses)
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();


        // Um curso pode ter várias disciplinas (Subjects) através de CourseSubjects
        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    }
}
