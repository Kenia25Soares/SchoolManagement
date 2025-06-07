using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "User Role")]
        public string Role { get; set; } = null!;

        public List<string> Roles { get; set; } = new();

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureUrl { get; set; }

        // Student-only fields:
        [Display(Name = "Contact Number")]
        public string? Contact { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Home Address")]
        public string? Address { get; set; }

        [Display(Name = "Official Student Photo URL")]
        public string? OfficialPhotoUrl { get; set; }
    }
}
