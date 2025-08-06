using API.SchoolManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using System.Security.Claims;

namespace API.SchoolManagement.Controllers
{
    /// <summary>
    /// Controller for managing student grades and subjects
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradesRepository _gradesRepository;
        private readonly ILogger<GradesController> _logger;

        public GradesController(IGradesRepository gradesRepository, ILogger<GradesController> logger)
        {
            _gradesRepository = gradesRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all grades for a student
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <returns>Student grades grouped by subject with weighted averages</returns>
        [HttpGet("{studentId}")]
        public async Task<ActionResult<object>> GetGrades(string studentId)
        {
            try
            {
                // Validate that the authenticated user can only access their own data
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != studentId)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "You can only access your own grades"
                    });
                }

                // Get student with class info
                var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
                if (student == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Get grades and absences
                var grades = await _gradesRepository.GetGradesWithSubjectsAndTypesAsync(studentId);
                var absences = await _gradesRepository.GetAbsencesByStudentAsync(studentId);

                // Group grades by subject (same logic as your web controller)
                var groupedGrades = grades
                    .GroupBy(g => g.Subject)
                    .Select(g => new
                    {
                        SubjectName = g.Key?.Name ?? "Unknown",
                        SubjectCode = g.Key?.Name?.Replace(" ", "").ToUpper() ?? "UNKNOWN", // Use name as code
                        GradesByType = g.GroupBy(x => x.GradeType)
                            .Where(gt => gt.Key != null)
                            .Select(gt => new
                            {
                                GradeTypeName = gt.Key!.Name,
                                Weight = gt.Key.Weight,
                                Grades = gt.Select(x => x.Grade ?? 0).ToList()
                            }).ToList(),
                        AllowedAbsences = g.Key?.AllowedAbsences ?? 0,
                        TotalAbsences = absences.Where(a => a.SubjectId == g.Key!.Id).Sum(a => a.Absences),
                        WeightedAverage = 0.0, // Will be calculated below
                        FailedDueToAbsences = false // Will be calculated below
                    }).ToList();

                // Calculate weighted averages for each subject
                foreach (var subject in groupedGrades)
                {
                    double subjectWeightedSum = 0;
                    double subjectWeightTotal = 0;

                    foreach (var gt in subject.GradesByType)
                    {
                        if (gt.Weight > 0 && gt.Grades.Any())
                        {
                            var avg = gt.Grades.Average();
                            subjectWeightedSum += avg * gt.Weight;
                            subjectWeightTotal += gt.Weight;
                        }
                    }

                    // Update the anonymous type properties using reflection or create a new list
                    var updatedSubject = new
                    {
                        subject.SubjectName,
                        subject.SubjectCode,
                        subject.GradesByType,
                        subject.AllowedAbsences,
                        subject.TotalAbsences,
                        WeightedAverage = subjectWeightTotal > 0 ? subjectWeightedSum / subjectWeightTotal : 0,
                        FailedDueToAbsences = subject.TotalAbsences > subject.AllowedAbsences
                    };

                    // Replace the item in the list
                    var index = groupedGrades.IndexOf(subject);
                    groupedGrades[index] = updatedSubject;
                }

                // Calculate total average
                double totalWeightedSum = 0;
                double totalOverallWeight = 0;

                foreach (var subject in groupedGrades)
                {
                    foreach (var gt in subject.GradesByType)
                    {
                        if (gt.Weight > 0 && gt.Grades.Any())
                        {
                            foreach (var grade in gt.Grades)
                            {
                                totalWeightedSum += grade * gt.Weight;
                                totalOverallWeight += gt.Weight;
                            }
                        }
                    }
                }

                double totalAverage = totalOverallWeight > 0 ? totalWeightedSum / totalOverallWeight : 0;

                return Ok(new
                {
                    Success = true,
                    Message = "Grades retrieved successfully",
                    Results = new
                    {
                        StudentId = student.User.Id,
                        StudentName = student.User.FullName,
                        SubjectGrades = groupedGrades,
                        TotalAverage = totalAverage,
                        IsClassClosed = student.StudentClass?.IsClosed ?? false
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grades for student {StudentId}", studentId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all subjects for a student's course
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <returns>List of subjects available for the student</returns>
        [HttpGet("{studentId}/subjects")]
        public async Task<ActionResult<object>> GetStudentSubjects(string studentId)
        {
            try
            {
                // Validate that the authenticated user can only access their own data
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != studentId)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "You can only access your own subjects"
                    });
                }

                // Get student with class info
                var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
                if (student == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Get subjects for the student's course
                var subjects = await _gradesRepository.GetSubjectsByCourseAsync(student.StudentClass.CourseId);

                var subjectsList = subjects.Select(s => new
                {
                    SubjectCode = s.Name?.Replace(" ", "").ToUpper() ?? "UNKNOWN", // Use name as code
                    SubjectName = s.Name,
                    TeacherName = "Unknown" // You might need to add teacher info to your repository
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "Subjects retrieved successfully",
                    Results = subjectsList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subjects for student {StudentId}", studentId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get detailed grades and absences for a specific subject
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="subjectCode">The subject code or name</param>
        /// <returns>Detailed subject information including grades, absences, and status</returns>
        [HttpGet("{studentId}/subject/{subjectCode}")]
        public async Task<ActionResult<object>> GetSubjectGrade(string studentId, string subjectCode)
        {
            try
            {
                // Validate that the authenticated user can only access their own data
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != studentId)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "You can only access your own grades"
                    });
                }

                // Get student with class info
                var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
                if (student == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Get all subjects for the student's course
                var allSubjects = await _gradesRepository.GetSubjectsByCourseAsync(student.StudentClass.CourseId);

                // Find the subject by name (since we don't have Code field)
                var subject = allSubjects.FirstOrDefault(s =>
                    s.Name.Equals(subjectCode, StringComparison.OrdinalIgnoreCase) ||
                    s.Name.Replace(" ", "").ToUpper().Equals(subjectCode, StringComparison.OrdinalIgnoreCase));

                if (subject == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Subject not found"
                    });
                }

                // Get grades for this specific subject
                var grades = await _gradesRepository.GetGradesWithSubjectsAndTypesAsync(studentId);
                var subjectGrades = grades.Where(g => g.SubjectId == subject.Id).ToList();

                // Get absences for this subject
                var absences = await _gradesRepository.GetAbsencesByStudentAsync(studentId);
                var subjectAbsences = absences.Where(a => a.SubjectId == subject.Id).ToList();

                // Calculate weighted average
                double weightedAverage = 0;
                double totalWeight = 0;
                var gradeDetails = new List<object>();

                foreach (var grade in subjectGrades.Where(g => g.Grade.HasValue))
                {
                    var weight = grade.GradeType?.Weight ?? 0;
                    if (weight > 0)
                    {
                        weightedAverage += grade.Grade.Value * weight;
                        totalWeight += weight;

                        gradeDetails.Add(new
                        {
                            Description = grade.GradeType?.Name ?? "Unknown",
                            Grade = grade.Grade.Value,
                            Weight = weight,
                            Date = grade.CreatedAt
                        });
                    }
                }

                if (totalWeight > 0)
                {
                    weightedAverage /= totalWeight;
                }

                // Process absences
                var absenceDetails = new List<object>();
                int totalAbsences = subjectAbsences.Sum(a => a.Absences);
                int allowedAbsences = subject.AllowedAbsences;

                // Since absences are stored as counts, we'll create individual absence records
                foreach (var absence in subjectAbsences)
                {
                    // Create individual absence records (you might need to adjust this based on your data structure)
                    for (int i = 0; i < absence.Absences; i++)
                    {
                        absenceDetails.Add(new
                        {
                            Date = absence.CreatedAt, // You might need to store actual absence dates
                            Justification = "", // Add justification field if needed
                            IsJustified = false // Add justification logic if needed
                        });
                    }
                }

                // Determine status - using default passing grade of 10.0 since Subject doesn't have PassingGrade
                double passingGrade = 10.0; // Default passing grade
                string status = DetermineStatus(weightedAverage, totalAbsences, allowedAbsences, passingGrade);
                bool failedDueToAbsences = totalAbsences > allowedAbsences;

                return Ok(new
                {
                    Success = true,
                    Message = "Subject grade retrieved successfully",
                    Result = new
                    {
                        SubjectName = subject.Name,
                        SubjectCode = subject.Name?.Replace(" ", "").ToUpper() ?? "UNKNOWN",
                        WeightedAverage = Math.Round(weightedAverage, 2),
                        TotalAbsences = totalAbsences,
                        AllowedAbsences = allowedAbsences,
                        FailedDueToAbsences = failedDueToAbsences,
                        Status = status,
                        GradeDetails = gradeDetails,
                        AbsenceDetails = absenceDetails
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject grade for student {StudentId}, subject {SubjectCode}", studentId, subjectCode);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Determine student status based on grades and absences
        /// </summary>
        /// <param name="average">Weighted average grade</param>
        /// <param name="totalAbsences">Total number of absences</param>
        /// <param name="allowedAbsences">Maximum allowed absences</param>
        /// <param name="passingGrade">Minimum grade to pass</param>
        /// <returns>Status string: "Approved", "Failed", or "Excluded due to absences"</returns>
        private string DetermineStatus(double average, int totalAbsences, int allowedAbsences, double passingGrade)
        {
            if (totalAbsences > allowedAbsences)
            {
                return "Excluded due to absences";
            }

            if (average >= passingGrade)
            {
                return "Approved";
            }

            return "Failed";
        }
    }
}