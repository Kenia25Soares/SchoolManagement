using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Web.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string ProfilePicturePath { get; set; }
    }
}
