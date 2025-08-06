using System.ComponentModel.DataAnnotations;

namespace API.SchoolManagement.Models
{
    public class UpdatePasswordViewModel
    {
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
} 