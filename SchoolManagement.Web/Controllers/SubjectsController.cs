using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Linq;
using System.Threading.Tasks;

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
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var subjects = await _subjectRepository.GetAll().ToListAsync();
            var viewModel = subjects.Select(s => _converterHelper.ToSubjectViewModel(s)).ToList();
            return View("/Views/AdminDashboard/Subjects/Index.cshtml", viewModel);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();
            return View("/Views/AdminDashboard/Subjects/Create.cshtml");
        }

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

            // Verifica se o subject está em uso em algum CourseSubject
            var isInUse = await _subjectRepository.IsSubjectInUseAsync(id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Cannot delete this subject because it is associated with courses.";
                return RedirectToAction(nameof(Index));
            }

            await _subjectRepository.DeleteAsync(subject);
            TempData["SuccessMessage"] = "Subject deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

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