using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IAlertRepository _alertRepository;
        private readonly IAdminDashboardRepository _dashboardRepository;

        public AdminDashboardController(
            IUserHelper userHelper,
            IBlobHelper blobHelper,
            IAlertRepository alertRepository,
            IAdminDashboardRepository dashboardRepository)
        {
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _alertRepository = alertRepository;
            _dashboardRepository = dashboardRepository;
        }

        /// <summary>
        /// Sets the current user's profile picture in the view data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        /// <summary>
        /// Displays the main dashboard view for the admin, including alerts and statistics.
        /// </summary>
        /// <returns>The admin dashboard view with alert and statistics data.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var alertsData = await _alertRepository.GetAllAsync();

            var alerts = alertsData.Select(a => new AlertViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Message,
                Priority = AlertPriority.Medium, // Default priority
                IsResolved = a.IsRead,
                CreatedBy = a.CreatedBy?.FullName ?? "System" 
            }).ToList();

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _userHelper.GetUsersCountByRolesAsync(),
                TotalCourses = await _dashboardRepository.GetCoursesCountAsync(),
                TotalSubjects = await _dashboardRepository.GetSubjectsCountAsync()
            };

            var model = new AdminDashboardCombinedViewModel
            {
                Alerts = alerts,
                Stats = stats
            };

            return View(model);
            //Views/AdminDashboard/Index
        }
    }
}
