using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
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

        /// <summary>
        /// Sets the currently logged-in user's profile picture in the view data.
        /// </summary>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl; ;
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
            return View(viewModel);
            //Views/AdminDashboard/Courses/Index
        }

        /// <summary>
        /// Shows the form to create a new course.
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();

            //Views/AdminDashboard/Courses/Create
            return View();
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="model">The course view model containing input data.</param>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to create course. Please check the form.";
                return View(model);
                //Views/AdminDashboard/Courses/Create
            }

            var entity = _converterHelper.ToCourseEntity(model, true);
            await _courseRepository.CreateAsync(entity);

            TempData["SuccessMessage"] = "Course successfully created.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Shows the form to edit an existing course.
        /// </summary>
        /// <param name="id">The ID of the course to edit.</param>
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
            //Views/AdminDashboard/Courses/Edit
            return View(viewModel);
        }


        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="model">The updated course view model.</param>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to update course. Please check the form.";
                return View(model);
                //Views/AdminDashboard/Courses/Edit
            }

            var entity = _converterHelper.ToCourseEntity(model, false);
            await _courseRepository.UpdateAsync(entity);

            TempData["SuccessMessage"] = "Course successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Shows confirmation page for deleting a course.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
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
            return View(viewModel);
            //Views/AdminDashboard/Courses/Delete
        }

        /// <summary>
        /// Deletes the course after confirmation.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
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

            try
            {
                await _courseRepository.DeleteAsync(course);
                TempData["SuccessMessage"] = "Course successfully deleted.";
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("student classes"))
                    TempData["ErrorMessage"] = "Cannot delete a course that has student classes assigned. Remove or reassign the classes first.";
                else
                    TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
            
        }


        /// <summary>
        /// Manages the subjects assigned to a course.
        /// </summary>
        /// <param name="id">The ID of the course to manage.</param>
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

            return View(model);
           //Views/AdminDashboard/Courses/Manage   
        }


        /// <summary>
        /// Assigns a subject to a course.
        /// </summary>
        /// <param name="request">Request containing course and subject IDs.</param>
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
        /// <param name="request">Request containing course and subject IDs.</param>
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
        /// Displays detailed information about a course.
        /// </summary>
        /// <param name="id">The ID of the course.</param>
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

            //Views/AdminDashboard/Courses/Details
            return View(model);
        }
    }
}
