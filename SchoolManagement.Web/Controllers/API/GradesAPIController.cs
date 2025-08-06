using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // exige o login via JWT
    public class GradesAPIController : ControllerBase
    {
        private readonly IStudentGradeHelper _gradeHelper;
        private readonly IStudentAbsenceHelper _absenceHelper;

        public GradesAPIController(IStudentGradeHelper gradeHelper, IStudentAbsenceHelper absenceHelper)
        {
            _gradeHelper = gradeHelper;
            _absenceHelper = absenceHelper;
        }

        /// <summary>
        /// Returns student grades, absences and averages.
        /// </summary>
        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudentGrades(string studentId)
        {
            var gradesDetails = await _gradeHelper.GetGradesDetailsAsync(studentId);
            var absences = await _absenceHelper.GetAbsencesAsync(studentId);

            if (gradesDetails == null)
            {
                return NotFound(new
                {
                    IsSuccess = false,
                    Message = "No information found for this student."
                });
            }

            return Ok(new
            {
                IsSuccess = true,
                Message = "Grades and absences successfully recovered.",
                Results = new
                {
                    gradesDetails.StudentName,
                    gradesDetails.SubjectGrades,
                    gradesDetails.TotalAverage,
                    gradesDetails.IsClassClosed,
                    absences.Absences
                }
            });
        }
    }
}
