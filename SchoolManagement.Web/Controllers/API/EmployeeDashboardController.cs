using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeDashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();
            return View();
        }
    }
}
