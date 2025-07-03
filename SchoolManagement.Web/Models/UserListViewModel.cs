using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Profile Picture")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; } = null!;

        [Display(Name = "User Role")]
        public string Role { get; set; } = null!;

        public double? AverageGrade { get; set; }
        public int TotalAbsences { get; set; }
        public bool IsFailedByAbsences { get; set; }
        public bool FailedDueToAbsences { get; internal set; }

        public string? ClassName { get; set; }
    }
}
