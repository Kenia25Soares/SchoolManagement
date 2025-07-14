using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Linq;
using System.Threading.Tasks;

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

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var alerts = _alertRepository.GetAll()
                .Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Priority = a.Priority,
                    IsResolved = a.IsResolved,
                    CreatedBy = a.CreatedBy.FullName
                })
                .ToList();

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
        }
    }
}
