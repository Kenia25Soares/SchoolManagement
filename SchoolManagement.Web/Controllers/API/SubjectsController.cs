using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// Controller responsável pela gestão de Subjects (Disciplinas).
    /// Apenas acessível ao Admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Subjects")]
    public class SubjectsController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IConverterHelper _converterHelper;

        public SubjectsController(ISubjectRepository subjectRepository, IConverterHelper converterHelper)
        {
            _subjectRepository = subjectRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista todos os Subjects existentes.
        /// </summary>
        // GET: AdminDashboard/Subjects
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectRepository.GetAll().ToListAsync();
            var viewModel = subjects.Select(s => _converterHelper.ToSubjectViewModel(s)).ToList();
            return View("/Views/AdminDashboard/Subjects/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Abre o formulário para criar um novo Subject.
        /// </summary>
        // GET: AdminDashboard/Subjects/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("/Views/AdminDashboard/Subjects/Create.cshtml");
        }

        /// <summary>
        /// Recebe o POST do formulário de criação e cria o Subject.
        /// </summary>
        // POST: AdminDashboard/Subjects/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = _converterHelper.ToSubjectEntity(model, true);
                await _subjectRepository.CreateAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/AdminDashboard/Subjects/Create.cshtml", model);
        }

        /// <summary>
        /// Abre o formulário de edição de um Subject.
        /// </summary>
        // GET: AdminDashboard/Subjects/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null) return NotFound();

            var model = _converterHelper.ToSubjectViewModel(subject);
            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", model);
        }

        /// <summary>
        /// Recebe o POST do formulário de edição e atualiza o Subject.
        /// </summary>
        // POST: AdminDashboard/Subjects/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = _converterHelper.ToSubjectEntity(model, false);
                await _subjectRepository.UpdateAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", model);
        }

        /// <summary>
        /// Mostra a página de confirmação de remoção de um Subject.
        /// </summary>
        // GET: AdminDashboard/Subjects/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null) return NotFound();

            var model = _converterHelper.ToSubjectViewModel(subject);
            return View("/Views/AdminDashboard/Subjects/Delete.cshtml", model);
        }

        /// <summary>
        /// Remove o Subject após confirmação.
        /// </summary>
        // POST: AdminDashboard/Subjects/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject != null)
            {
                await _subjectRepository.DeleteAsync(subject);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
