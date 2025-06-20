using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class StudentGrade
    {
        public int Id { get; set; }

        public string StudentId { get; set; }
        public StudentUser Student { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public double? Grade { get; set; }
        public int Absences { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
