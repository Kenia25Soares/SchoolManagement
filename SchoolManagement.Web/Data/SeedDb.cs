using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data
{
    public class SeedDb
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await EnsureRoleAsync("Admin");
            await EnsureRoleAsync("Employee");
            await EnsureRoleAsync("Student");

            await EnsureAdminAsync();
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private async Task EnsureAdminAsync()
        {
            var email = "admin@school.com";
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user, "Admin123*");
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
