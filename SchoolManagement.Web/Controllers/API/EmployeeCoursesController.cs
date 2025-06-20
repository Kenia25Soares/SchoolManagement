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

    [HttpPost("AssignStudent")]
    public async Task<IActionResult> AssignStudent(int courseId, string studentId)
    {
        var studentClass = await _context.StudentClasses.FirstOrDefaultAsync(sc => sc.Id == courseId);
        if (studentClass == null) return NotFound();

        var student = await _context.Users.OfType<StudentUser>().FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return NotFound();

        student.StudentClassId = studentClass.Id;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("RemoveStudent")]
    public async Task<IActionResult> RemoveStudent(int courseId, string studentId)
    {
        var student = await _context.Users.OfType<StudentUser>()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.StudentClassId == courseId);

        if (student == null) return NotFound();

        student.StudentClassId = null;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("AssignSubject")]
    public async Task<IActionResult> AssignSubject(int courseId, int subjectId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return NotFound();

        if (!course.CourseSubjects.Any(cs => cs.SubjectId == subjectId))
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
