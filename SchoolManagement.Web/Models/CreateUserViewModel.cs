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
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }


        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Required]
        public string Role { get; set; }

        public List<string>? Roles { get; set; }

        // Apenas para Student
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? OfficialPhotoUrl { get; set; }

        public int? CourseId { get; set; }
    }
}
