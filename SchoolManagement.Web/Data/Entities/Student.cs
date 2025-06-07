using System;

namespace SchoolManagement.Web.Data.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public string? Contact { get; set; }
        public string? OfficialPhotoUrl { get; set; }
        public int Absences { get; set; }
        public bool IsExcludedDueToAbsences { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }

        // FK
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public int? CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
