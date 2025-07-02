using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    /// <summary>
    /// Controller for managing student classes (turmas) in the employee dashboard.
    /// </summary>
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

        /// <summary>
        /// Displays all student classes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _studentClassHelper.GetAllAsync();
            return View("Views/EmployeeDashboard/StudentClasses/Index.cshtml", model);
        }

        /// <summary>
        /// Returns the create form for a new student class.
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new StudentClassViewModel
            {
                Courses = await _studentClassHelper.GetCoursesSelectListAsync()
            };
            return View("Views/EmployeeDashboard/StudentClasses/Create.cshtml", vm);
        }

        /// <summary>
        /// Handles the submission of a new student class.
        /// </summary>
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

        /// <summary>
        /// Displays the edit form for a student class.
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null) return NotFound();

            model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
            return View("Views/EmployeeDashboard/StudentClasses/Edit.cshtml", model);
        }

        /// <summary>
        /// Updates a student class after form submission.
        /// </summary>
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

        /// <summary>
        /// Displays the confirmation page for deleting a student class.
        /// </summary>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null) return NotFound();

            return View("Views/EmployeeDashboard/StudentClasses/Delete.cshtml", model);
        }

        /// <summary>
        /// Confirms and performs deletion of a student class.
        /// </summary>
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

        /// <summary>
        /// Allows management of student assignments to a class.
        /// </summary>
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
                .Where(s => s.StudentClassId == null)
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

        /// <summary>
        /// Assigns or unassigns a student to a class (AJAX).
        /// </summary>
        [HttpPost("AssignStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudent([FromBody] AssignStudentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentId))
                return BadRequest("Invalid request");

            var student = await _studentClassRepository.GetStudentByIdAsync(request.StudentId);
            if (student == null)
                return NotFound("Student not found");

            student.StudentClassId = request.StudentClassId;

            await _studentClassRepository.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Displays the details of a student class, including subjects.
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var studentClass = await _studentClassRepository.GetAll()
                .Where(sc => sc.Id == id)
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    sc.AcademicYear,
                    sc.Shift,
                    Course = sc.Course,
                    Subjects = sc.Course.CourseSubjects.Select(cs => cs.Subject.Name)
                })
                .FirstOrDefaultAsync();

            if (studentClass == null)
                return NotFound();

            var model = new StudentClassViewModel
            {
                Id = studentClass.Id,
                Name = studentClass.Name,
                AcademicYear = studentClass.AcademicYear,
                Shift = studentClass.Shift,
                CourseName = studentClass.Course?.Name
            };

            ViewBag.Subjects = studentClass.Subjects.ToList();

            return View("Views/EmployeeDashboard/StudentClasses/Details.cshtml", model);
        }
    }
}
