using System.ComponentModel.DataAnnotations;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Entities
{
    public class Alert
    {
        public int Id { get; set; }


        [Required]
        public string Title { get; set; } = null!;


        [Required]
        public string Description { get; set; } = null!;


        [Required]
        public AlertPriority Priority { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public bool IsResolved { get; set; } = false;


        // FK para Employee que criou
        public string CreatedById { get; set; } = null!;


        public ApplicationUser CreatedBy { get; set; } = null!;
    }
}
