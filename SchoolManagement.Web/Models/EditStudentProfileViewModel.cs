using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class EditStudentProfileViewModel : EditProfileViewModel
    {
        [Required]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? OfficialPhotoUrl { get; set; }

        [Display(Name = "Official Photo")]
        [DataType(DataType.Upload)]
        public IFormFile? OfficialPhoto { get; set; }

        [Display(Name = "Class")]
        public int? StudentClassId { get; set; }
    }
}
