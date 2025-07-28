using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// API controller for managing student classes and retrieving related students.
    /// </summary>
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentClassesAPIController : ControllerBase
    {
        private readonly IStudentClassRepository _studentClassRepository;

        /// <summary>
        /// Constructor to inject repository.
        /// </summary>
        public StudentClassesAPIController(IStudentClassRepository studentClassRepository)
        {
            _studentClassRepository = studentClassRepository;
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
            var classWithStudents = await _studentClassRepository.GetClassWithStudentsAsync(id);

            if (classWithStudents == null)
            {
                return NotFound(new Response
                {
                    IsSuccess = false,
                    Message = "Class not found."
                });
            }

            return Ok(new Response
            {
                IsSuccess = true,
                Message = "Class found successfully.",
                Results = classWithStudents
            });

            return Ok(new Response
            {
                IsSuccess = true,
                Message = "Class found successfully.",
                Results = classWithStudents
            });
        }
    }
}
