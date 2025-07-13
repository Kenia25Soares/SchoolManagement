using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public interface IUserHelper
    {
        Task<IdentityResult> AddUserAsync(ApplicationUser user, string password);
        Task AddUserToRoleAsync(ApplicationUser user, string roleName);
        Task CheckRoleAsync(string roleName);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password);
        Task<ApplicationUser> GetUserAsync(ClaimsPrincipal principal);
        Task<SelectList> GetClassesSelectListAsync(int? selectedClassId);
        Task<int> GetUsersCountAsync();
        Task<SignInResult> ValidatePasswordAsync(ApplicationUser user, string password);
        Task<List<ApplicationUser>> GetAllUsersAsync();

        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(ApplicationUser user);
        Task<IdentityResult> RemoveUserFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    }
}
