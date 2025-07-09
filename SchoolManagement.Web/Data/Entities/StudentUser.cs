using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class StudentUser : ApplicationUser
    {
        [Required]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; }

        public bool IsExcludedDueToAbsences { get; set; }

        [Required]
        [Display(Name = "Official Photo URL")]
        public string? OfficialPhotoUrl { get; set; }

        public int? StudentClassId { get; set; }

        public StudentClass? StudentClass { get; set; }
    }
}
