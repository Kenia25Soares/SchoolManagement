using API.SchoolManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Data.Entities;

using System.Security.Claims;

namespace API.SchoolManagement.Controllers
{
    /// <summary>
    /// Controller for managing subject enrollment requests
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly ISubjectEnrollmentRequestRepository _enrollmentRepository;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            ISubjectEnrollmentRequestRepository enrollmentRepository,
            ILogger<EnrollmentController> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get available subjects for enrollment
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <returns>List of available subjects for enrollment</returns>
        [HttpGet("available-subjects/{studentId}")]
        public async Task<ActionResult<AvailableSubjectsResponse>> GetAvailableSubjects(string studentId)
        {
            try
            {
                // Validate that the authenticated user can only access their own data
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != studentId)
                {
                    return Unauthorized(new AvailableSubjectsResponse
                    {
                        Success = false,
                        Message = "You can only access your own data"
                    });
                }

                var availableSubjects = await _enrollmentRepository.GetAvailableSubjectsForStudentAsync(studentId);

                var results = new List<AvailableSubject>();

                foreach (var subject in availableSubjects)
                {
                    var hasPendingRequest = await _enrollmentRepository.HasPendingRequestForSubjectAsync(studentId, subject.Id);

                    results.Add(new AvailableSubject
                    {
                        SubjectId = subject.Id,
                        SubjectName = subject.Name,
                        SubjectCode = subject.Name?.Replace(" ", "").ToUpper() ?? "UNKNOWN",
                        Workload = subject.Workload,
                        AllowedAbsences = subject.AllowedAbsences,
                        HasPendingRequest = hasPendingRequest
                    });
                }

                return Ok(new AvailableSubjectsResponse
                {
                    Success = true,
                    Message = "Available subjects retrieved successfully",
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available subjects for student {StudentId}", studentId);
                return StatusCode(500, new AvailableSubjectsResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Create a new enrollment request
        /// </summary>
        /// <param name="model">Enrollment request data</param>
        /// <returns>Result of the enrollment request creation</returns>
        [HttpPost("request")]
        public async Task<ActionResult<EnrollmentOperationResponse>> CreateEnrollmentRequest([FromBody] CreateEnrollmentRequestModel model)
        {
            try
            {
                // Validate that the authenticated user can only create requests for themselves
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != model.StudentId)
                {
                    return Unauthorized(new EnrollmentOperationResponse
                    {
                        Success = false,
                        Message = "You can only create requests for yourself"
                    });
                }

                // Check if student already has a pending request for this subject
                var hasPendingRequest = await _enrollmentRepository.HasPendingRequestForSubjectAsync(model.StudentId, model.SubjectId);
                if (hasPendingRequest)
                {
                    return BadRequest(new EnrollmentOperationResponse
                    {
                        Success = false,
                        Message = "You already have a pending request for this subject"
                    });
                }

                // Check if the subject is available for the student
                var availableSubjects = await _enrollmentRepository.GetAvailableSubjectsForStudentAsync(model.StudentId);
                if (!availableSubjects.Any(s => s.Id == model.SubjectId))
                {
                    return BadRequest(new EnrollmentOperationResponse
                    {
                        Success = false,
                        Message = "This subject is not available for enrollment"
                    });
                }

                // Create the enrollment request
                var enrollmentRequest = new SubjectEnrollmentRequest
                {
                    StudentId = model.StudentId,
                    SubjectId = model.SubjectId,
                    Description = model.Description,
                    Status = 0, 
                    RequestDate = DateTime.UtcNow
                };

                await _enrollmentRepository.CreateAsync(enrollmentRequest);

                return Ok(new EnrollmentOperationResponse
                {
                    Success = true,
                    Message = "Enrollment request created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enrollment request for student {StudentId}, subject {SubjectId}", 
                    model.StudentId, model.SubjectId);
                return StatusCode(500, new EnrollmentOperationResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get student's enrollment requests
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <returns>List of student's enrollment requests</returns>
        [HttpGet("my-requests/{studentId}")]
        public async Task<ActionResult<EnrollmentRequestsResponse>> GetMyRequests(string studentId)
        {
            try
            {
                // Validate that the authenticated user can only access their own data
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != studentId)
                {
                    return Unauthorized(new EnrollmentRequestsResponse
                    {
                        Success = false,
                        Message = "You can only access your own requests"
                    });
                }

                var requests = await _enrollmentRepository.GetRequestsByStudentAsync(studentId);

                var results = requests.Select(r => new EnrollmentRequestInfo
                {
                    RequestId = r.Id,
                    SubjectName = r.Subject.Name,
                    SubjectCode = r.Subject.Name?.Replace(" ", "").ToUpper() ?? "UNKNOWN",
                    Description = r.Description,
                    Status = ConvertStatusToString((int)r.Status),
                    RequestDate = r.RequestDate,
                    ResponseMessage = r.ResponseMessage,
                    ProcessedByName = r.ProcessedBy?.FullName,
                    ProcessedDate = r.ProcessedDate
                }).ToList();

                return Ok(new EnrollmentRequestsResponse
                {
                    Success = true,
                    Message = "Enrollment requests retrieved successfully",
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollment requests for student {StudentId}", studentId);
                return StatusCode(500, new EnrollmentRequestsResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Convert status integer to string
        /// </summary>
        /// <param name="statusValue">Status as integer (0=Pending, 1=Approved, 2=Rejected)</param>
        /// <returns>Status as string</returns>
        private string ConvertStatusToString(int statusValue)
        {
            return statusValue switch
            {
                0 => "Pending",
                1 => "Approved", 
                2 => "Rejected",
                _ => "Unknown"
            };
        }
    }
}
