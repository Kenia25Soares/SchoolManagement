using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SendVerificationCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 