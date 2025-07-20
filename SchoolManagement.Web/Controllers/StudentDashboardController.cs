using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IStudentGradeHelper _studentGradeHelper;
        private readonly IStudentAbsenceHelper _studentAbsenceHelper;

        public StudentDashboardController(
            IUserHelper userHelper,
            IStudentGradeHelper studentGradeHelper,
            IStudentAbsenceHelper studentAbsenceHelper)
        {
            _userHelper = userHelper;
            _studentGradeHelper = studentGradeHelper;
            _studentAbsenceHelper = studentAbsenceHelper;
        }


        /// <summary>
        /// Sets the profile picture URL of the currently logged-in user in the ViewData.
        /// </summary>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        /// <summary>
        /// Displays the main student dashboard with basic student information.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var user = await _userHelper.GetUserAsync(User);
            var model = new StudentDashboardViewModel
            {
                StudentId = user.Id,
                StudentName = user.FullName
            };
            return View(model);
        }


        /// <summary>
        /// Displays the student's grades including calculated averages.
        /// </summary>
        /// <returns>The grades view with grade details model.</returns>
        public async Task<IActionResult> Grades()
        {
            await SetUserProfilePictureAsync();

            var user = await _userHelper.GetUserAsync(User);

            var gradesModel = await _studentGradeHelper.GetGradesDetailsAsync(user.Id);

            return View(gradesModel);
        }


        /// <summary>
        /// Displays the student's absence records.
        /// </summary>
        /// <returns>The absences view with absence summary model.</returns>
        public async Task<IActionResult> Absences()
        {
            await SetUserProfilePictureAsync();

            var user = await _userHelper.GetUserAsync(User);

            var absencesModel = await _studentAbsenceHelper.GetAbsencesAsync(user.Id);

            return View(absencesModel);
        }
    }
}
