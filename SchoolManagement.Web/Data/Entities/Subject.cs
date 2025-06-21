using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class Subject : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Workload (hours)")]
        public int Workload { get; set; }

        [Display(Name = "Allowed Absences")]
        public int AllowedAbsences { get; set; } = 0;

        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();

        public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
    }
}
