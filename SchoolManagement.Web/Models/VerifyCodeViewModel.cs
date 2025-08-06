using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class VerifyCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 characters")]
        public string Code { get; set; }
    }
} 