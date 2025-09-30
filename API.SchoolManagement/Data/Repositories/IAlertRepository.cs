using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace API.SchoolManagement.Data.Repositories
{
    public interface IAlertRepository
    {
        Task<List<Alert>> GetStudentAlertsAsync(string studentId, bool includeRead = true);
        Task<List<Alert>> GetUnreadStudentAlertsAsync(string studentId);
        Task<int> GetUnreadCountAsync(string studentId);
        Task<Alert?> GetAlertWithDetailsAsync(int alertId);
        Task MarkAsReadAsync(int alertId);
        Task MarkMultipleAsReadAsync(List<int> alertIds);
        Task<List<Alert>> GetRecentAlertsAsync(string studentId, int count = 10);
        Task<bool> AlertExistsAsync(string studentId, int type, int? subjectId = null, int? gradeId = null);
    }
}
