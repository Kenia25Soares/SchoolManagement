using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly DataContext _context;

        public AdminDashboardController(IUserHelper userHelper, IBlobHelper blobHelper, DataContext context)
        {
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _context = context;
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

            var alerts = await _context.Alerts
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Priority = a.Priority,
                    IsResolved = a.IsResolved,
                    CreatedBy = a.CreatedBy.FullName
                })
                .ToListAsync();

            return View(alerts);
        }
    }
}
