using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Subjects")]
    public class SubjectsController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;

        public SubjectsController(ISubjectRepository subjectRepository,
                                  IConverterHelper converterHelper,
                                  IUserHelper userHelper)
        {
            _subjectRepository = subjectRepository;
            _converterHelper = converterHelper;
            _userHelper=userHelper;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        /// <summary>
        /// Displays the list of all subjects.
        /// </summary>
        /// <returns>The subjects index view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var subjects = await _subjectRepository.GetAll().ToListAsync();
            var viewModel = subjects.Select(s => _converterHelper.ToSubjectViewModel(s)).ToList();
            return View("/Views/AdminDashboard/Subjects/Index.cshtml", viewModel);
        }


        /// <summary>
        /// Displays the form to create a new subject.
        /// </summary>
        /// <returns>The subject creation form view.</returns>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();
            return View("/Views/AdminDashboard/Subjects/Create.cshtml");
        }


        /// <summary>
        /// Handles the creation of a new subject.
        /// </summary>
        /// <param name="model">The subject view model containing form data.</param>
        /// <returns>Redirects to index or reloads the form on error.</returns>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = _converterHelper.ToSubjectEntity(model, true);
                await _subjectRepository.CreateAsync(subject);
                TempData["SuccessMessage"] = "Subject created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/AdminDashboard/Subjects/Create.cshtml", model);
        }


        /// <summary>
        /// Displays the form to edit an existing subject.
        /// </summary>
        /// <param name="id">The ID of the subject to edit.</param>
        /// <returns>The subject edit form view.</returns>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            await SetUserProfilePictureAsync();

            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = _converterHelper.ToSubjectViewModel(subject);
            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", model);
        }


        /// <summary>
        /// Handles the update of an existing subject.
        /// </summary>
        /// <param name="model">The subject view model with updated data.</param>
        /// <returns>Redirects to index or reloads the form on error.</returns>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = _converterHelper.ToSubjectEntity(model, false);
                await _subjectRepository.UpdateAsync(subject);
                TempData["SuccessMessage"] = "Subject updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", model);
        }


        /// <summary>
        /// Displays the confirmation view to delete a subject.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>The delete confirmation view.</returns>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await SetUserProfilePictureAsync();

            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = _converterHelper.ToSubjectViewModel(subject);
            return View("/Views/AdminDashboard/Subjects/Delete.cshtml", model);
        }


        /// <summary>
        /// Confirms and executes the deletion of a subject if not in use.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>Redirects to index with result message.</returns>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _subjectRepository.DeleteAsync(subject);
                TempData["SuccessMessage"] = "Subject deleted successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            //// Verifica se o subject está em uso em algum CourseSubject, para evitar exclusão de um subject que está associado a cursos.
            //var isInUse = await _subjectRepository.IsSubjectInUseAsync(id);
            //if (isInUse)
            //{
            //    TempData["ErrorMessage"] = "Cannot delete this subject because it is associated with courses.";
            //    return RedirectToAction(nameof(Index));
            //}

            //await _subjectRepository.DeleteAsync(subject);
            //TempData["SuccessMessage"] = "Subject deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays detailed information about a specific subject.
        /// </summary>
        /// <param name="id">The ID of the subject to view.</param>
        /// <returns>The subject details view.</returns>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            await SetUserProfilePictureAsync();
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = _converterHelper.ToSubjectViewModel(subject);
            return View("Views/AdminDashboard/Subjects/Details.cshtml", model);
        }
    }
}