using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace API.SchoolManagement.Models
{
    public class UpdateFullProfileRequest
    {
        // User fields
        [Required]
        public string FullName { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? Email { get; set; }

        // Student-specific fields
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        public IFormFile? OfficialPhoto { get; set; }
    }
}
