using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Repositories;

namespace API.SchoolManagement.Controllers
{
    [ApiController]
    [Route("api/public")] // anonymous catalog 
    public class PublicCatalogController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IStudentClassRepository _studentClassRepository;

        public PublicCatalogController(
            ICourseRepository courseRepository,
            ISubjectRepository subjectRepository,
            IStudentClassRepository studentClassRepository)
        {
            _courseRepository = courseRepository;
            _subjectRepository = subjectRepository;
            _studentClassRepository = studentClassRepository;
        }

        // GET: /api/public/courses
        [HttpGet("courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourses()
        {
            var list = await _courseRepository.GetAll().ToListAsync();
            return Ok(new { success = true, results = list.Select(c => new { c.Id, c.Name }) });
        }

        // GET: /api/public/courses/{id}
        [HttpGet("courses/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseRepository.GetAll()
                .Include(c => c.StudentClasses)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound(new { success = false, message = "Course not found" });

            return Ok(new
            {
                success = true,
                course = new
                {
                    course.Id,
                    course.Name,
                    classes = course.StudentClasses.Select(sc => new { sc.Id, sc.Name, sc.AcademicYear, sc.Shift, sc.IsClosed })
                }
            });
        }

        // GET: /api/public/courses/{id}/subjects
        [HttpGet("courses/{id:int}/subjects")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubjectsByCourse(int id)
        {
            var subjects = await _subjectRepository.GetAll()
                .Where(s => s.CourseSubjects.Any(cs => cs.CourseId == id))
                .ToListAsync();
            return Ok(new { success = true, results = subjects.Select(s => new { s.Id, s.Name, s.Workload, s.AllowedAbsences }) });
        }

        // GET: /api/public/classes?courseId=&year=&shift=
        [HttpGet("classes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClasses([FromQuery] int? courseId, [FromQuery] string? year, [FromQuery] string? shift)
        {
            var query = _studentClassRepository.GetAll();
            if (courseId.HasValue) query = query.Where(c => c.CourseId == courseId.Value);
            if (!string.IsNullOrWhiteSpace(year)) query = query.Where(c => c.AcademicYear == year);
            if (!string.IsNullOrWhiteSpace(shift)) query = query.Where(c => c.Shift == shift);

            var list = await query.ToListAsync();
            return Ok(new { success = true, results = list.Select(c => new { c.Id, c.Name, c.AcademicYear, c.Shift, c.CourseId, c.IsClosed }) });
        }

        // GET: /api/public/classes/{id}
        [HttpGet("classes/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassById(int id)
        {
            var cls = await _studentClassRepository.GetAll()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cls == null) return NotFound(new { success = false, message = "Class not found" });

            return Ok(new
            {
                success = true,
                @class = new { cls.Id, cls.Name, cls.AcademicYear, cls.Shift, course = new { cls.CourseId, cls.Course.Name }, cls.IsClosed }
            });
        }

        // GET: /api/public/subjects
        [HttpGet("subjects")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubjects()
        {
            var list = await _subjectRepository.GetAll().ToListAsync();
            return Ok(new { success = true, results = list.Select(s => new { s.Id, s.Name, s.Workload, s.AllowedAbsences }) });
        }

        // GET: /api/public/subjects/{id}
        [HttpGet("subjects/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var subject = await _subjectRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound(new { success = false, message = "Subject not found" });
            return Ok(new { success = true, subject = new { subject.Id, subject.Name, subject.Workload, subject.AllowedAbsences } });
        }
    }
}
