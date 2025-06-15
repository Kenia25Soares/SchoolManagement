using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
       
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;

        public AdminDashboardController(IUserHelper userHelper, IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _blobHelper = blobHelper;
        }

        private async Task SetUserProfilePictureAsync()
        {
            
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();
            return View();
        }
    }
}
