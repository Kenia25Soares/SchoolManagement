using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            // Verificar se as roles existem, caso contrário, criá-las
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Employee");
            await _userHelper.CheckRoleAsync("Student");

            // Criar o utilizador Admin se não existir
            var email = "admin@school.com";
            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    FullName = "System Admin",
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await _userHelper.AddUserAsync(user, "Admin123*");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default admin user.");
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }

            // Garantir que continua no Role Admin 
            if (!await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
        }
    }
}
