using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/StudentClasses")]
    public class StudentClassesController : Controller
    {
        private readonly IStudentClassHelper _studentClassHelper;
        private readonly IStudentClassRepository _studentClassRepository;

        public StudentClassesController(IStudentClassHelper studentClassHelper, IStudentClassRepository studentClassRepository)
        {
            _studentClassHelper = studentClassHelper;
            _studentClassRepository = studentClassRepository;
        }

        // List all student classes
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = await _studentClassHelper.GetAllAsync();
            return View("Views/EmployeeDashboard/StudentClasses/Index.cshtml", model);
        }

        // Create form
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new StudentClassViewModel
            {
                Courses = await _studentClassHelper.GetCoursesSelectListAsync()
            };
            return View("Views/EmployeeDashboard/StudentClasses/Create.cshtml", vm);
        }

        // Create POST
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
                return View("Views/EmployeeDashboard/StudentClasses/Create.cshtml", model);
            }

            var entity = new StudentClass
            {
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                Shift = model.Shift,
                CourseId = model.CourseId
            };

            await _studentClassRepository.CreateAsync(entity);
            return RedirectToAction(nameof(Index));
        }

        // Edit form
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null) return NotFound();

            model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
            return View("Views/EmployeeDashboard/StudentClasses/Edit.cshtml", model);
        }

        // Edit POST
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
                return View("Views/EmployeeDashboard/StudentClasses/Edit.cshtml", model);
            }

            var entity = await _studentClassRepository.GetByIdAsync(model.Id);
            if (entity == null) return NotFound();

            entity.Name = model.Name;
            entity.AcademicYear = model.AcademicYear;
            entity.Shift = model.Shift;
            entity.CourseId = model.CourseId;

            await _studentClassRepository.UpdateAsync(entity);
            return RedirectToAction(nameof(Index));
        }

        // Delete GET
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null) return NotFound();

            return View("Views/EmployeeDashboard/StudentClasses/Delete.cshtml", model);
        }

        // Delete POST
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _studentClassRepository.GetByIdAsync(id);
            if (entity != null)
            {
                await _studentClassRepository.DeleteAsync(entity);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Manage/{id}")]
        public async Task<IActionResult> Manage(int id)
        {
            var studentClass = await _studentClassRepository.GetByIdWithDetailsAsync(id);
            if (studentClass == null) return NotFound();

            var allStudents = await _studentClassHelper.GetAllStudentsAsync();

            var assignedStudents = studentClass.Students.Select(s => new StudentAssignmentViewModel
            {
                StudentId = s.Id,
                StudentName = s.FullName
            }).ToList();

            var availableStudents = allStudents
                .Where(s => s.StudentClassId == null)   // Apenas alunos sem turma atribuída
                .Select(s => new StudentAssignmentViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FullName
                })
                .ToList();

            var vm = new ManageStudentClassViewModel
            {
                StudentClassId = studentClass.Id,
                StudentClassName = studentClass.Name,
                AssignedStudents = assignedStudents,
                AvailableStudents = availableStudents
            };

            return View("Views/EmployeeDashboard/StudentClasses/Manage.cshtml", vm);
        }

        // AJAX POST to assign or unassign student immediately
        [HttpPost("AssignStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudent([FromBody] AssignStudentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentId))
                return BadRequest("Invalid request");

            var student = await _studentClassRepository.GetStudentByIdAsync(request.StudentId);
            if (student == null)
                return NotFound("Student not found");

            student.StudentClassId = request.StudentClassId; // null to remove assignment

            await _studentClassRepository.SaveChangesAsync();

            return Ok();
        }

        public class AssignStudentRequest
        {
            public string StudentId { get; set; }
            public int? StudentClassId { get; set; }
        }
    }
}
