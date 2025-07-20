using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentDetailsViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Address { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public string? OfficialPhotoUrl { get; set; }

        public List<SubjectGradeViewModel> SubjectGrades { get; set; } = new();
    }
}

