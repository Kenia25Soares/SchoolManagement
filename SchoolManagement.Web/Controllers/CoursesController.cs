using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    /// <summary>
    /// Controller responsible for course management.
    /// Accessible only by Admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Courses")]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICourseHelper _courseHelper;
        private readonly IUserHelper _userHelper;

        public CoursesController(
            ICourseRepository courseRepository,
            IConverterHelper converterHelper,
            ICourseHelper courseHelper,
            IUserHelper userHelper)
        {
            _courseRepository = courseRepository;
            _converterHelper = converterHelper;
            _courseHelper = courseHelper;
            _userHelper = userHelper;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        /// <summary>
        /// Displays the list of all courses.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var courses = await _courseRepository.GetAll()
                .Include(c => c.CourseSubjects)
                .ToListAsync();

            var viewModel = courses.Select(c => _converterHelper.ToCourseViewModel(c)).ToList();
            return View("Views/AdminDashboard/Courses/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Shows the form to create a new course.
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();
            return View("Views/AdminDashboard/Courses/Create.cshtml");
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = _converterHelper.ToCourseEntity(model, true);
                await _courseRepository.CreateAsync(course);
                TempData["SuccessMessage"] = "Course successfully created.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to create course. Please check the form.";
            return View("Views/AdminDashboard/Courses/Create.cshtml", model);
        }

        /// <summary>
        /// Shows the form to edit an existing course.
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            await SetUserProfilePictureAsync();
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _converterHelper.ToCourseViewModel(course);
            return View("Views/AdminDashboard/Courses/Edit.cshtml", viewModel);
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = _converterHelper.ToCourseEntity(model, false);
                await _courseRepository.UpdateAsync(course);
                TempData["SuccessMessage"] = "Course successfully updated.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update course. Please check the form.";
            return View("Views/AdminDashboard/Courses/Edit.cshtml", model);
        }

        /// <summary>
        /// Shows confirmation page for deleting a course.
        /// </summary>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await SetUserProfilePictureAsync();

            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _converterHelper.ToCourseViewModel(course);
            return View("Views/AdminDashboard/Courses/Delete.cshtml", viewModel);
        }

        /// <summary>
        /// Deletes the course.
        /// </summary>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdWithAllRelationsAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            // Se houver notas associadas, não permitir a exclusão
            if (course.StudentGrades != null && course.StudentGrades.Any())
            {
                TempData["ErrorMessage"] = "This course has student grades assigned and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Remove outras entidades relacionadas (subjects, classes)
            if (course.CourseSubjects != null && course.CourseSubjects.Any())
            {
                await _courseRepository.RemoveCourseSubjectsAsync(course.CourseSubjects);
            }

            if (course.StudentClasses != null && course.StudentClasses.Any())
            {
                TempData["ErrorMessage"] = "This course has student classes assigned and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Agora pode excluir o curso
            await _courseRepository.DeleteAsync(course);
            TempData["SuccessMessage"] = "Course successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Manages the subjects assigned to a course.
        /// </summary>
        [HttpGet("Manage/{id}")]
        public async Task<IActionResult> Manage(int id)
        {
            await SetUserProfilePictureAsync();

            var model = await _courseHelper.GetCourseManagementAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            return View("Views/AdminDashboard/Courses/Manage.cshtml", model);
        }

        /// <summary>
        /// Assigns a subject to a course.
        /// </summary>
        [HttpPost("AssignSubject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSubject([FromBody] AssignSubjectRequest request)
        {
            if (request == null || request.CourseId <= 0 || request.SubjectId <= 0)
                return BadRequest("Invalid request.");

            await _courseHelper.AssignSubjectToCourseAsync(request.CourseId, request.SubjectId);

            return Ok(new { message = "Subject successfully assigned to course." });
        }

        /// <summary>
        /// Removes a subject from a course.
        /// </summary>
        [HttpPost("RemoveSubject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSubject([FromBody] AssignSubjectRequest request)
        {
            if (request == null || request.CourseId <= 0 || request.SubjectId <= 0)
                return BadRequest("Invalid request.");

            await _courseHelper.RemoveSubjectFromCourseAsync(request.CourseId, request.SubjectId);

            return Ok(new { message = "Subject successfully removed from course." });
        }

        /// <summary>
        /// Displays course details.
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            await SetUserProfilePictureAsync();

            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = _converterHelper.ToCourseViewModel(course);

            return View("Views/AdminDashboard/Courses/Details.cshtml", model);
        }
    }
}
