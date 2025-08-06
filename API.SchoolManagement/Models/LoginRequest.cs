using System.ComponentModel.DataAnnotations;

namespace API.SchoolManagement.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

        public bool rememberMe { get; set; }
    }
} 