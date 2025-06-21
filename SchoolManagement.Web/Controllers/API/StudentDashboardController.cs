using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
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

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

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

        public async Task<IActionResult> Grades()
        {
            var user = await _userHelper.GetUserAsync(User);

            var gradesModel = await _studentGradeHelper.GetGradesDetailsAsync(user.Id);

            return View(gradesModel);
        }

        public async Task<IActionResult> Absences()
        {
            var user = await _userHelper.GetUserAsync(User);

            var absencesModel = await _studentAbsenceHelper.GetAbsencesAsync(user.Id);

            return View(absencesModel);
        }
    }
}
