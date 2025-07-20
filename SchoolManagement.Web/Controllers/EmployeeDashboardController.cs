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
        private readonly IGenericRepository<StudentProfile> _studentProfileRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IUserHelper _userHelper;

        public EmployeeDashboardController(
            IGenericRepository<StudentProfile> studentProfileRepo,
            IStudentClassRepository studentClassRepo,
            IUserHelper userHelper)
        {
            _studentProfileRepo = studentProfileRepo;
            _studentClassRepo = studentClassRepo;
            _userHelper = userHelper;
        }


        /// <summary>
        /// Displays statistics on the employee dashboard, including total students and class status.
        /// </summary>
        /// <returns>The dashboard view with statistics.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students = await _studentProfileRepo.GetAll().ToListAsync();
            var classes = await _studentClassRepo.GetAll().ToListAsync();

            var model = new EmployeeDashboardViewModel
            {
                TotalStudents = students.Count,
                OpenClasses = classes.Count(c => !c.IsClosed),
                ClosedClasses = classes.Count(c => c.IsClosed)
            };

            return View("Views/EmployeeDashboard/Index.cshtml", model);
        }
    }
}
