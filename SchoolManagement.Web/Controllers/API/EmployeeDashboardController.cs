using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// Dashboard do Funcionário (Employee)
    /// </summary>
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly IUserHelper _userHelper;

        public EmployeeDashboardController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }


        /// <summary>
        /// Atribui a foto de perfil à ViewData.
        /// </summary>
        private async Task SetUserProfilePictureAsync()
        {
            
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        /// <summary>
        /// GET: Dashboard do Employee
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();
            return View();
        }
    }
}
