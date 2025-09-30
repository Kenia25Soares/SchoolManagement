using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;
using SchoolManagement.Web.Data.Repositories;

namespace SchoolManagement.Web.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly DataContext _context;
        private readonly ILogger<AlertService> _logger;

        public AlertService(IAlertRepository alertRepository, DataContext context, ILogger<AlertService> logger)
        {
            _alertRepository = alertRepository;
            _context = context;
            _logger = logger;
        }

        public async Task CreateGradePostedAlertAsync(string studentId, int subjectId, int gradeId, double? gradeValue, string gradeTypeName)
        {
            try
            {
                _logger.LogInformation("Creating grade posted alert for student {StudentId}, subject {SubjectId}, grade {GradeId}", studentId, subjectId, gradeId);
                
                // Check if alert already exists for this grade
                var alertExists = await _alertRepository.AlertExistsAsync(studentId, AlertType.GradePosted, subjectId, gradeId);
                if (alertExists)
                {
                    _logger.LogInformation("Alert for grade {GradeId} already exists for student {StudentId}", gradeId, studentId);
                    return;
                }

                var subject = await _context.Subjects.FindAsync(subjectId);
                var subjectName = subject?.Name ?? "Unknown Subject";

                var gradeText = gradeValue.HasValue ? gradeValue.Value.ToString("F1") : "Present";
                var title = $"New {gradeTypeName} - {subjectName}";
                var message = gradeValue.HasValue 
                    ? $"Your {gradeTypeName.ToLower()} grade of {gradeText} has been posted for {subjectName}."
                    : $"Your attendance has been recorded for {subjectName}.";

                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.GradePosted,
                    Title = title,
                    Message = message,
                    SubjectId = subjectId,
                    StudentGradeId = gradeId
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("Grade posted alert created successfully for student {StudentId}, subject {SubjectId}, alert ID {AlertId}", studentId, subjectId, alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating grade posted alert for student {StudentId}", studentId);
            }
        }

        public async Task CreateStatusChangedAlertAsync(string studentId, string oldStatus, string newStatus, int? subjectId = null)
        {
            try
            {
                var title = "Status Update";
                var message = subjectId.HasValue 
                    ? $"Your status has changed from {oldStatus} to {newStatus} for this subject."
                    : $"Your overall status has changed from {oldStatus} to {newStatus}.";

                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.StatusChanged,
                    Title = title,
                    Message = message,
                    SubjectId = subjectId,
                    Metadata = $"{{\"oldStatus\":\"{oldStatus}\",\"newStatus\":\"{newStatus}\"}}"
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("Status changed alert created for student {StudentId}", studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating status changed alert for student {StudentId}", studentId);
            }
        }

        public async Task CreateAddedToClassAlertAsync(string studentId, int studentClassId, string className)
        {
            try
            {
                var title = "Added to Class";
                var message = $"You have been enrolled in class {className}. Welcome!";

                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.AddedToClass,
                    Title = title,
                    Message = message,
                    StudentClassId = studentClassId
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("Added to class alert created for student {StudentId}, class {StudentClassId}", studentId, studentClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating added to class alert for student {StudentId}", studentId);
            }
        }

        public async Task CreateRemovedFromClassAlertAsync(string studentId, string className)
        {
            try
            {
                var title = "Removed from Class";
                var message = $"You have been removed from class {className}. Please contact administration if you have questions.";

                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.RemovedFromClass,
                    Title = title,
                    Message = message
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("Removed from class alert created for student {StudentId}", studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating removed from class alert for student {StudentId}", studentId);
            }
        }

        public async Task CreateClassClosedAlertAsync(List<string> studentIds, int studentClassId, string className)
        {
            try
            {
                var title = "Class Closed";
                var message = $"Class {className} has been closed. No further enrollments or changes will be accepted.";

                var alerts = studentIds.Select(studentId => new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.ClassClosed,
                    Title = title,
                    Message = message,
                    StudentClassId = studentClassId
                }).ToList();

                foreach (var alert in alerts)
                {
                    await _alertRepository.CreateAsync(alert);
                }

                _logger.LogInformation("Class closed alerts created for {Count} students in class {StudentClassId}", studentIds.Count, studentClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class closed alerts for class {StudentClassId}", studentClassId);
            }
        }

        public async Task CreateExcludedByAbsencesAlertAsync(string studentId, int subjectId, string subjectName, int absences, int allowedAbsences)
        {
            try
            {
                var title = $"Excluded by Absences - {subjectName}";
                var message = $"You have been excluded from {subjectName} due to excessive absences ({absences}/{allowedAbsences}). Please contact your coordinator.";

                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.ExcludedByAbsences,
                    Title = title,
                    Message = message,
                    SubjectId = subjectId,
                    Metadata = $"{{\"absences\":{absences},\"allowedAbsences\":{allowedAbsences}}}"
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("Excluded by absences alert created for student {StudentId}, subject {SubjectId}", studentId, subjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating excluded by absences alert for student {StudentId}", studentId);
            }
        }

        public async Task CreateGeneralNotificationAsync(string studentId, string title, string message, string? metadata = null)
        {
            try
            {
                var alert = new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.GeneralNotification,
                    Title = title,
                    Message = message,
                    Metadata = metadata
                };

                await _alertRepository.CreateAsync(alert);
                _logger.LogInformation("General notification created for student {StudentId}", studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating general notification for student {StudentId}", studentId);
            }
        }

        public async Task CreateGeneralNotificationForClassAsync(List<string> studentIds, string title, string message, string? metadata = null)
        {
            try
            {
                var alerts = studentIds.Select(studentId => new Alert
                {
                    StudentId = studentId,
                    Type = AlertType.GeneralNotification,
                    Title = title,
                    Message = message,
                    Metadata = metadata
                }).ToList();

                foreach (var alert in alerts)
                {
                    await _alertRepository.CreateAsync(alert);
                }

                _logger.LogInformation("General notifications created for {Count} students", studentIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating general notifications for class");
            }
        }
    }
}
