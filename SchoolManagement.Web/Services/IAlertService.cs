using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Services
{
    public interface IAlertService
    {
        Task CreateGradePostedAlertAsync(string studentId, int subjectId, int gradeId, double? gradeValue, string gradeTypeName);
        Task CreateStatusChangedAlertAsync(string studentId, string oldStatus, string newStatus, int? subjectId = null);
        Task CreateAddedToClassAlertAsync(string studentId, int studentClassId, string className);
        Task CreateRemovedFromClassAlertAsync(string studentId, string className);
        Task CreateClassClosedAlertAsync(List<string> studentIds, int studentClassId, string className);
        Task CreateExcludedByAbsencesAlertAsync(string studentId, int subjectId, string subjectName, int absences, int allowedAbsences);
        Task CreateGeneralNotificationAsync(string studentId, string title, string message, string? metadata = null);
        Task CreateGeneralNotificationForClassAsync(List<string> studentIds, string title, string message, string? metadata = null);
    }
}

