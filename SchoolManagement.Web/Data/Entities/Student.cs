using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class StudentUser : ApplicationUser
    {
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool IsExcludedDueToAbsences { get; set; }
        public string? OfficialPhotoUrl { get; set; }
        public int? CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
