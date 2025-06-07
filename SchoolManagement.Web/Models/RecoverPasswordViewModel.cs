using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
