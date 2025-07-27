using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    /// <summary>
    /// Dashboard do Funcionário (Employee)
    /// </summary>
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {

        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IUserHelper _userHelper;

        public EmployeeDashboardController(
            IStudentProfileRepository studentProfileRepository,
            IStudentClassRepository studentClassRepository,
            IUserHelper userHelper)
        {
            _studentProfileRepository = studentProfileRepository;
            _studentClassRepository = studentClassRepository;
            _userHelper = userHelper;
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
        /// Displays statistics on the employee dashboard, including total students and class status.
        /// </summary>
        /// <returns>The dashboard view with statistics.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var students = await _studentProfileRepository.GetAll().ToListAsync();
            var classes = await _studentClassRepository.GetAll().ToListAsync();

            var model = new EmployeeDashboardViewModel
            {
                TotalStudents = students.Count,
                OpenClasses = classes.Count(c => !c.IsClosed),
                ClosedClasses = classes.Count(c => c.IsClosed)
            };

            //Views/EmployeeDashboard/Index
            return View(model);
        }
    }
}
