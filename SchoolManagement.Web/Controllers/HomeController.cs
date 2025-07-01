using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Repository;
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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Courses()
        {
            var courses = await _courseRepository.GetAll()
                                .Include(c => c.CourseSubjects)
                                .ToListAsync();

            var model = courses.Select(c => _converterHelper.ToCourseViewModel(c)).ToList();

            return View(model);
        }


        public async Task<IActionResult> StudentClasses()
        {
            var classes = await _classRepository.GetAll()
                            .Include(sc => sc.Course)
                                .ThenInclude(c => c.CourseSubjects)
                                    .ThenInclude(cs => cs.Subject)
                            .ToListAsync();

            return View(classes); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Public()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
