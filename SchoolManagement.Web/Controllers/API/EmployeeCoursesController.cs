using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

[Authorize(Roles = "Employee")]
[Route("EmployeeDashboard/Courses")]
public class EmployeeCoursesController : Controller
{
    private readonly DataContext _context;
    private readonly ICourseHelper _courseHelper;

    public EmployeeCoursesController(DataContext context, ICourseHelper courseHelper)
    {
        _context = context;
        _courseHelper = courseHelper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.ToListAsync();
        return View("/Views/EmployeeDashboard/Courses/Index.cshtml", courses);
    }

    [HttpGet("Manage/{id}")]
    public async Task<IActionResult> Manage(int id)
    {
        var model = await _courseHelper.GetCourseManagementAsync(id);
        if (model == null) return NotFound();

        return View("/Views/EmployeeDashboard/Courses/Manage.cshtml", model);
    }

    // AJAX para atribuir aluno
    [HttpPost("AssignStudent")]
    public async Task<IActionResult> AssignStudent(int courseId, string studentId)
    {
        var student = await _context.Users.OfType<StudentUser>().FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return NotFound();

        student.CourseId = courseId;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // AJAX para remover aluno
    [HttpPost("RemoveStudent")]
    public async Task<IActionResult> RemoveStudent(int courseId, string studentId)
    {
        var student = await _context.Users.OfType<StudentUser>().FirstOrDefaultAsync(s => s.Id == studentId && s.CourseId == courseId);
        if (student == null) return NotFound();

        student.CourseId = null;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // AJAX para atribuir disciplina (usando CourseSubject)
    [HttpPost("AssignSubject")]
    public async Task<IActionResult> AssignSubject(int courseId, int subjectId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return NotFound();

        bool alreadyAssigned = course.CourseSubjects.Any(cs => cs.SubjectId == subjectId);
        if (!alreadyAssigned)
        {
            course.CourseSubjects.Add(new CourseSubject
            {
                CourseId = courseId,
                SubjectId = subjectId
            });

            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("RemoveSubject")]
    public async Task<IActionResult> RemoveSubject(int courseId, int subjectId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return NotFound();

        var courseSubject = course.CourseSubjects.FirstOrDefault(cs => cs.SubjectId == subjectId);
        if (courseSubject != null)
        {
            course.CourseSubjects.Remove(courseSubject);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}
