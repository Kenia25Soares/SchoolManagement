using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;
using System.Security.Claims;

namespace SchoolManagement.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;  //faz a gestão dos utilizadores
        private readonly SignInManager<ApplicationUser> _signInManager;  //faz a gestão do login e logout dos utilizadores, que injeta o UserManager<ApplicationUser> para gerir os utilizadores
        private readonly RoleManager<IdentityRole> _roleManager;  //faz a gestão  (roles) dos utilizadores
                                                                
        private readonly IStudentClassRepository _studentClassRepository;  //faz a gestão das turmas dos alunos
        private readonly IAlertRepository _alertRepository;

        public UserHelper(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
           IStudentClassRepository studentClassRepository,
           IAlertRepository alertRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _studentClassRepository = studentClassRepository;
            _alertRepository = alertRepository;
        }


        public async Task<SelectList> GetClassesSelectListAsync(int? selectedClassId)
        {
            var classes = await _studentClassRepository.GetClassesSelectListAsync(selectedClassId);
            return new SelectList(classes, "Value", "Text", selectedClassId);
        }

        public async Task<IdentityResult> AddUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        //public async Task<SelectList> GetClassesSelectListAsync(int? selectedClassId)
        //{
        //    var classes = await _context.StudentClasses
        //        .OrderBy(c => c.Name)
        //        .ToListAsync();

        //    return new SelectList(classes, "Id", "Name", selectedClassId);
        //}

        public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<int> GetUsersCountByRolesAsync()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var employeeUsers = await _userManager.GetUsersInRoleAsync("Employee");

            return adminUsers.Concat(employeeUsers).Select(u => u.Id).Distinct().Count();
            
        }

        public async Task<SignInResult> ValidatePasswordAsync(ApplicationUser user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(ApplicationUser user)
        {
            var alerts = await _alertRepository.GetAll()
                .Where(a => a.CreatedById == user.Id)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                alert.CreatedById = null;
                await _alertRepository.UpdateAsync(alert);
            }

            // Remove o user de todas as roles
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> RemoveUserFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }
    }
}
