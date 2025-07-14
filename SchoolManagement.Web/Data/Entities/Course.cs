using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();

        public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();

    }
}
