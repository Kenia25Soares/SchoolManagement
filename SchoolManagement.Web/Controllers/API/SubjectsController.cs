using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Models;

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

        public SubjectsController(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        /// <summary>
        /// Lista todos os Subjects existentes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectRepository.GetAll().ToListAsync();
            var viewModel = subjects.Select(s => new SubjectViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Workload = s.Workload
            }).ToList();

            return View("/Views/AdminDashboard/Subjects/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Abre o formulário para criar um novo Subject.
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("/Views/AdminDashboard/Subjects/Create.cshtml");
        }

        /// <summary>
        /// Recebe o POST do formulário de criação e cria o Subject.
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = new Subject
                {
                    Name = model.Name,
                    Workload = model.Workload
                };

                await _subjectRepository.CreateAsync(subject);
                return RedirectToAction(nameof(Index));
            }

            return View("/Views/AdminDashboard/Subjects/Create.cshtml", model);
        }

        /// <summary>
        /// Abre o formulário de edição de um Subject.
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null) return NotFound();

            var viewModel = new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Workload = subject.Workload
            };

            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", viewModel);
        }

        /// <summary>
        /// Recebe o POST do formulário de edição e atualiza o Subject.
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = new Subject
                {
                    Id = model.Id,
                    Name = model.Name,
                    Workload = model.Workload
                };

                await _subjectRepository.UpdateAsync(subject);
                return RedirectToAction(nameof(Index));
            }

            return View("/Views/AdminDashboard/Subjects/Edit.cshtml", model);
        }

        /// <summary>
        /// Mostra a página de confirmação de remoção de um Subject.
        /// </summary>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null) return NotFound();

            var viewModel = new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Workload = subject.Workload
            };

            return View("/Views/AdminDashboard/Subjects/Delete.cshtml", viewModel);
        }

        /// <summary>
        /// Remove o Subject após confirmação.
        /// </summary>
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
