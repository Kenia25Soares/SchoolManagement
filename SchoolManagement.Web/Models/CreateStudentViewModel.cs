using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class CreateStudentViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;

        public string ClassName { get; set; }


        [Display(Name = "Official Photo")]
        public IFormFile? OfficialPhoto { get; set; }


        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }
        public int? StudentClassId { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? OfficialPhotoUrl { get; set; }
    }
}

