using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models.ViewModels;

namespace SchoolManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Courses")]
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public CourseController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // GET: Index
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllAsync();
            var viewModel = courses.Select(c => new CourseViewModel
            {
                Id = c.Id,
                Name = c.Name,
                AcademicYear = c.AcademicYear,
                Shift = c.Shift
            }).ToList();

            return View("Views/AdminDashboard/Courses/Index.cshtml", viewModel);
        }

        // GET: Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("Views/AdminDashboard/Courses/Create.cshtml");
        }

        // POST: Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    Name = model.Name,
                    AcademicYear = model.AcademicYear,
                    Shift = model.Shift
                };

                await _courseRepository.AddAsync(course);
                return RedirectToAction(nameof(Index));
            }

            return View("Views/AdminDashboard/Courses/Create.cshtml", model);
        }

        // GET: Edit
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return NotFound();

            var viewModel = new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                AcademicYear = course.AcademicYear,
                Shift = course.Shift
            };

            return View("Views/AdminDashboard/Courses/Edit.cshtml", viewModel);
        }

        // POST: Edit
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    Id = model.Id,
                    Name = model.Name,
                    AcademicYear = model.AcademicYear,
                    Shift = model.Shift
                };

                await _courseRepository.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }

            return View("Views/AdminDashboard/Courses/Edit.cshtml", model);
        }

        // GET: Delete
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return NotFound();

            var viewModel = new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                AcademicYear = course.AcademicYear,
                Shift = course.Shift
            };

            return View("Views/AdminDashboard/Courses/Delete.cshtml", viewModel);
        }

        // POST: Delete
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
