using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Grades")]
    public class GradesController : Controller
    {
        private readonly DataContext _context;

        public GradesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? classId)
        {
            var classes = await _context.StudentClasses.Include(sc => sc.Course).ToListAsync();
            var selectedClass = classId ?? classes.FirstOrDefault()?.Id;

            var students = await _context.Users
                .OfType<StudentUser>()
                .Where(s => s.StudentClassId == selectedClass)
                .ToListAsync();

            ViewBag.Classes = new SelectList(classes, "Id", "Name", selectedClass);
            return View("/Views/EmployeeDashboard/Grades/Index.cshtml", students);
        }

        [HttpGet("Assign/{studentId}")]
        public async Task<IActionResult> Assign(string studentId)
        {
            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student?.StudentClassId == null || student.StudentClass?.Course == null)
                return NotFound();

            var courseId = student.StudentClass.CourseId;

            var subjects = await _context.CourseSubjects
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Subject)
                .Select(cs => new SubjectGradeInput
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject.Name
                }).ToListAsync();

            var model = new GradeAssignmentViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseId = courseId,
                CourseName = student.StudentClass.Course.Name,
                Subjects = subjects
            };

            return View("/Views/EmployeeDashboard/Grades/Assign.cshtml", model);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(GradeAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var subjects = await _context.CourseSubjects
                    .Where(cs => cs.CourseId == model.CourseId)
                    .Include(cs => cs.Subject)
                    .Select(cs => new SubjectGradeInput
                    {
                        SubjectId = cs.SubjectId,
                        SubjectName = cs.Subject.Name
                    }).ToListAsync();

                model.Subjects = subjects;

                var student = await _context.Users.OfType<StudentUser>().FirstOrDefaultAsync(s => s.Id == model.StudentId);
                model.StudentName = student?.FullName;
                model.CourseName = (await _context.Courses.FindAsync(model.CourseId))?.Name;

                return View("/Views/EmployeeDashboard/Grades/Assign.cshtml", model);
            }

            foreach (var subj in model.Subjects)
            {
                var existing = await _context.StudentGrades.FirstOrDefaultAsync(s =>
                    s.StudentId == model.StudentId &&
                    s.SubjectId == subj.SubjectId &&
                    s.CourseId == model.CourseId);

                if (existing != null)
                {
                    existing.Grade = subj.Grade;
                    existing.Absences = subj.Absences;
                }
                else
                {
                    _context.StudentGrades.Add(new StudentGrade
                    {
                        StudentId = model.StudentId,
                        SubjectId = subj.SubjectId,
                        CourseId = model.CourseId,
                        Grade = subj.Grade,
                        Absences = subj.Absences
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Assign", new { studentId = model.StudentId });
            }

            return RedirectToAction("Index", new { classId = model.CourseId });
        }
    }
}
