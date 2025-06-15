using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{ 
    
    /// <summary>
   /// Controller responsável pela gestão de cursos.
   /// Apenas acessível ao Admin.
   /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Courses")]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IConverterHelper _converterHelper;

        public CoursesController(ICourseRepository courseRepository, IConverterHelper converterHelper)
        {
            _courseRepository = courseRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista todos os cursos.
        /// </summary>
        // GET: AdminDashboard/Courses
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAll().ToListAsync();
            var viewModel = courses.Select(c => _converterHelper.ToCourseViewModel(c)).ToList();
            return View("Views/AdminDashboard/Courses/Index.cshtml", viewModel);
        }


        /// <summary>
        /// Abre o formulário para criar um novo curso.
        /// </summary>
        // GET: AdminDashboard/Courses/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("Views/AdminDashboard/Courses/Create.cshtml");
        }


        /// <summary>
        /// Cria um novo curso.
        /// </summary>
        // POST: AdminDashboard/Courses/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = _converterHelper.ToCourseEntity(model, true);
                await _courseRepository.CreateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View("Views/AdminDashboard/Courses/Create.cshtml", model);
        }


        /// <summary>
        /// Abre o formulário para editar um curso existente.
        /// </summary>
        // GET: AdminDashboard/Courses/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return NotFound();

            var viewModel = _converterHelper.ToCourseViewModel(course);
            return View("Views/AdminDashboard/Courses/Edit.cshtml", viewModel);
        }


        /// <summary>
        /// Atualiza um curso.
        /// </summary>
        // POST: AdminDashboard/Courses/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = _converterHelper.ToCourseEntity(model, false);
                await _courseRepository.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View("Views/AdminDashboard/Courses/Edit.cshtml", model);
        }


        /// <summary>
        /// Abre a confirmação de exclusão do curso.
        /// </summary>
        // GET: AdminDashboard/Courses/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return NotFound();

            var viewModel = _converterHelper.ToCourseViewModel(course);
            return View("Views/AdminDashboard/Courses/Delete.cshtml", viewModel);
        }


        /// <summary>
        /// Exclui o curso.
        /// </summary>
        // POST: AdminDashboard/Courses/Delete/{id}
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
