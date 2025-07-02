using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// API controller for managing student classes and retrieving related students.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentClassesController : ControllerBase
    {
        private readonly DataContext _context;

        /// <summary>
        /// Constructor to inject database context.
        /// </summary>
        /// <param name="context">The EF Core data context</param>
        public StudentClassesController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all students associated with a specific class.
        /// </summary>
        /// <param name="id">The ID of the class</param>
        /// <returns>A list of students for the given class</returns>
        /// <response code="200">Returns the list of students</response>
        /// <response code="404">If the class is not found</response>
        [HttpGet("{id}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStudentsByClass(int id)
        {
            var classWithStudents = await _context.StudentClasses
                .Include(sc => sc.Students)
                .Where(sc => sc.Id == id)
                .Select(sc => new
                {
                    ClassId = sc.Id,
                    ClassName = sc.Name,
                    AcademicYear = sc.AcademicYear,
                    Students = sc.Students.Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        s.Email,
                        s.PhoneNumber,
                        s.DateOfBirth
                    })
                })
                .FirstOrDefaultAsync();

            if (classWithStudents == null)
            {
                return NotFound(new Response
                {
                    IsSuccess = false,
                    Message = "Turma não encontrada."
                });
            }

            return Ok(new Response
            {
                IsSuccess = true,
                Message = "Turma encontrada com sucesso.",
                Results = classWithStudents
            });
        }
    }
}
