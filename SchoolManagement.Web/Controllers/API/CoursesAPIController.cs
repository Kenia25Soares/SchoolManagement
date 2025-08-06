using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesAPIController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IConverterHelper _converterHelper;

        public CoursesAPIController(ICourseRepository courseRepository, IConverterHelper converterHelper)
        {
            _courseRepository = courseRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Returns all courses with their subjects.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseRepository.GetAll().ToListAsync();

            var result = courses.Select(c => _converterHelper.ToCourseViewModel(c));
            return Ok(new
            {
                IsSuccess = true,
                Message = "Courses successfully recovered.",
                Results = result
            });
        }
    }
}
