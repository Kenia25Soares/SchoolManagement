using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Courses")]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        /// <summary>
        /// Lista todos os cursos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ⚠ ALTERAÇÃO IMPORTANTE: GetAll() + ToListAsync()
            var courses = await _courseRepository.GetAll().ToListAsync();

            var viewModel = courses.Select(c => new CourseViewModel
            {
                Id = c.Id,
                Name = c.Name,
                AcademicYear = c.AcademicYear,
                Shift = c.Shift
            }).ToList();

            return View("Views/AdminDashboard/Courses/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Abre o formulário de criação.
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("Views/AdminDashboard/Courses/Create.cshtml");
        }

        /// <summary>
        /// Cria um novo curso.
        /// </summary>
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

                await _courseRepository.CreateAsync(course);
                return RedirectToAction(nameof(Index));
            }

            return View("Views/AdminDashboard/Courses/Create.cshtml", model);
        }

        /// <summary>
        /// Abre o formulário de edição.
        /// </summary>
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

        /// <summary>
        /// Edita o curso.
        /// </summary>
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

        /// <summary>
        /// Mostra página de confirmação de delete.
        /// </summary>
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

        /// <summary>
        /// Remove o curso.
        /// </summary>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _courseRepository.GetByIdAsync(id);
            if (entity != null)
            {
                await _courseRepository.DeleteAsync(entity);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
