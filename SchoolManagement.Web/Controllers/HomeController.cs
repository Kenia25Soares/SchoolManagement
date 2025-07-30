using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Diagnostics;

namespace SchoolManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IStudentClassRepository _classRepository;

        public HomeController(ICourseRepository courseRepository, IConverterHelper converterHelper, IStudentClassRepository classRepository)
        {
            _courseRepository = courseRepository;
            _converterHelper = converterHelper;
            _classRepository = classRepository;
        }


        /// <summary>
        /// Displays the main landing page.
        /// </summary>
        /// <returns>The home view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the About page with project details, including author, course,
        /// version, and delivery date.
        /// </summary>
        /// <returns>The About view.</returns>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Displays a list of all available courses.
        /// </summary>
        /// <returns>Courses view with list of course view models.</returns>
        public async Task<IActionResult> Courses()
        {
            var courses = await _courseRepository.GetAll()
                                .Include(c => c.CourseSubjects)
                                .ToListAsync();

            var model = courses.Select(c => _converterHelper.ToCourseViewModel(c)).ToList();

            return View(model);
        }


        /// <summary>
        /// Displays a list of all student classes, including course and subject data.
        /// </summary>
        /// <returns>Student classes view.</returns>
        public async Task<IActionResult> StudentClasses()
        {
            var classes = await _classRepository.GetAll()
                            .Include(sc => sc.Course)
                                .ThenInclude(c => c.CourseSubjects)
                                    .ThenInclude(cs => cs.Subject)
                            .ToListAsync();

            return View(classes); 
        }


        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>The privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the public page (accessible without authentication).
        /// </summary>
        /// <returns>The public view.</returns>
        public IActionResult Public()
        {
            return View();
        }
    }
}
