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
        public string? OfficialPhotoUrl { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public int? StudentClassId { get; set; }
    }
}

