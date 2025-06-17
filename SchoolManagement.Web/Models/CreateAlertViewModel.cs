using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class CreateAlertViewModel
    {
        [Required]
        public string Title { get; set; } = null!;


        [Required]
        public string Description { get; set; } = null!;


        [Required]
        public AlertPriority Priority { get; set; }
    }
}
