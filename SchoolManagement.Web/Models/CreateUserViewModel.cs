using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User Role")]
        public string Role { get; set; }

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureUrl { get; set; }

        // Student-specific fields
        [Display(Name = "Contact Number")]
        public string? Contact { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Home Address")]
        public string? Address { get; set; }

        [Display(Name = "Official Student Photo URL")]
        public string? OfficialPhotoUrl { get; set; }

        // Role list
        public List<string> Roles { get; set; } = new();
    }
}
