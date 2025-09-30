using API.SchoolManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Enums;
using API.SchoolManagement.Data.Repositories;

namespace API.SchoolManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(IAlertRepository alertRepository, ILogger<AlertsController> logger)
        {
            _alertRepository = alertRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all alerts for a student (read and unread)
        /// </summary>
        /// <param name="studentId">The student's user ID</param>
        /// <returns>List of alerts for the student</returns>
        [HttpGet("{studentId}/all")]
        public async Task<IActionResult> GetStudentAlerts(string studentId)
        {
            try
            {
                var alerts = await _alertRepository.GetStudentAlertsAsync(studentId, includeRead: true);
                var unreadCount = await _alertRepository.GetUnreadCountAsync(studentId);

                var alertInfos = alerts.Select(a => new AlertInfo
                {
                    Id = a.Id,
                    Type = ConvertAlertTypeToString((int)a.Type),
                    Title = a.Title,
                    Message = a.Message,
                    IsRead = a.IsRead,
                    CreatedAt = a.CreatedAt,
                    ReadAt = a.ReadAt,
                    SubjectName = a.Subject?.Name,
                    ClassName = a.StudentClass?.Name,
                    Metadata = a.Metadata
                }).ToList();

                var response = new AlertsResponse
                {
                    Success = true,
                    Message = "Alerts retrieved successfully",
                    Alerts = alertInfos,
                    UnreadCount = unreadCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts for student {StudentId}", studentId);
                return StatusCode(500, new AlertsResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving alerts"
                });
            }
        }

        /// <summary>
        /// Gets only unread alerts for a student
        /// </summary>
        /// <param name="studentId">The student's user ID</param>
        /// <returns>List of unread alerts for the student</returns>
        [HttpGet("{studentId}/unread")]
        public async Task<IActionResult> GetUnreadAlerts(string studentId)
        {
            try
            {
                var alerts = await _alertRepository.GetUnreadStudentAlertsAsync(studentId);
                var unreadCount = alerts.Count;

                var alertInfos = alerts.Select(a => new AlertInfo
                {
                    Id = a.Id,
                    Type = ConvertAlertTypeToString((int)a.Type),
                    Title = a.Title,
                    Message = a.Message,
                    IsRead = a.IsRead,
                    CreatedAt = a.CreatedAt,
                    ReadAt = a.ReadAt,
                    SubjectName = a.Subject?.Name,
                    ClassName = a.StudentClass?.Name,
                    Metadata = a.Metadata
                }).ToList();

                var response = new AlertsResponse
                {
                    Success = true,
                    Message = "Unread alerts retrieved successfully",
                    Alerts = alertInfos,
                    UnreadCount = unreadCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread alerts for student {StudentId}", studentId);
                return StatusCode(500, new AlertsResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving unread alerts"
                });
            }
        }

        /// <summary>
        /// Gets recent alerts for a student (last 10 by default)
        /// </summary>
        /// <param name="studentId">The student's user ID</param>
        /// <param name="count">Number of recent alerts to retrieve default: 10, max: 50</param>
        /// <returns>List of recent alerts for the student</returns>
        [HttpGet("{studentId}/recent")]
        public async Task<IActionResult> GetRecentAlerts(string studentId, [FromQuery] int count = 10)
        {
            try
            {
                // Limit count to prevent abuse
                count = Math.Min(Math.Max(count, 1), 50);

                var alerts = await _alertRepository.GetRecentAlertsAsync(studentId, count);
                var unreadCount = await _alertRepository.GetUnreadCountAsync(studentId);

                var alertInfos = alerts.Select(a => new AlertInfo
                {
                    Id = a.Id,
                    Type = ConvertAlertTypeToString((int)a.Type),
                    Title = a.Title,
                    Message = a.Message,
                    IsRead = a.IsRead,
                    CreatedAt = a.CreatedAt,
                    ReadAt = a.ReadAt,
                    SubjectName = a.Subject?.Name,
                    ClassName = a.StudentClass?.Name,
                    Metadata = a.Metadata
                }).ToList();

                var response = new AlertsResponse
                {
                    Success = true,
                    Message = $"Recent alerts retrieved successfully (last {count})",
                    Alerts = alertInfos,
                    UnreadCount = unreadCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent alerts for student {StudentId}", studentId);
                return StatusCode(500, new AlertsResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving recent alerts"
                });
            }
        }

        /// <summary>
        /// Gets the count of unread alerts for a student
        /// </summary>
        /// <param name="studentId">The student's user ID</param>
        /// <returns>Count of unread alerts</returns>
        [HttpGet("{studentId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(string studentId)
        {
            try
            {
                var count = await _alertRepository.GetUnreadCountAsync(studentId);

                return Ok(new
                {
                    success = true,
                    message = "Unread count retrieved successfully",
                    unreadCount = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count for student {StudentId}", studentId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving unread count"
                });
            }
        }

        /// <summary>
        /// Marks a single alert as read
        /// </summary>
        /// <param name="alertId">The alert ID to mark as read</param>
        /// <returns>Success/failure response</returns>
        [HttpPost("{alertId}/mark-read")]
        public async Task<IActionResult> MarkAlertAsRead(int alertId)
        {
            try
            {
                await _alertRepository.MarkAsReadAsync(alertId);

                return Ok(new AlertOperationResponse
                {
                    Success = true,
                    Message = "Alert marked as read successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking alert {AlertId} as read", alertId);
                return StatusCode(500, new AlertOperationResponse
                {
                    Success = false,
                    Message = "An error occurred while marking alert as read"
                });
            }
        }

        /// <summary>
        /// Marks multiple alerts as read
        /// </summary>
        /// <param name="request">List of alert IDs to mark as read</param>
        /// <returns>Success/failure response</returns>
        [HttpPost("mark-multiple-read")]
        public async Task<IActionResult> MarkMultipleAlertsAsRead([FromBody] MarkAlertsAsReadRequest request)
        {
            try
            {
                if (request == null || !request.AlertIds.Any())
                {
                    return BadRequest(new AlertOperationResponse
                    {
                        Success = false,
                        Message = "No alert IDs provided"
                    });
                }

                await _alertRepository.MarkMultipleAsReadAsync(request.AlertIds);

                return Ok(new AlertOperationResponse
                {
                    Success = true,
                    Message = $"{request.AlertIds.Count} alerts marked as read successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking multiple alerts as read");
                return StatusCode(500, new AlertOperationResponse
                {
                    Success = false,
                    Message = "An error occurred while marking alerts as read"
                });
            }
        }

        /// <summary>
        /// Converts AlertType enum integer value to string representation
        /// </summary>
        private string ConvertAlertTypeToString(int alertTypeValue)
        {
            return alertTypeValue switch
            {
                1 => "GradePosted",
                2 => "StatusChanged",
                3 => "AddedToClass",
                4 => "RemovedFromClass",
                5 => "ClassClosed",
                6 => "ExcludedByAbsences",
                7 => "GeneralNotification",
                _ => "Unknown"
            };
        }
    }
}
