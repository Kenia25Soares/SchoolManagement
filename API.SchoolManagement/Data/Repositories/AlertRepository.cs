using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace API.SchoolManagement.Data.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly DataContext _context;

        public AlertRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Alert>> GetStudentAlertsAsync(string studentId, bool includeRead = true)
        {
            var query = _context.Alerts
                .Include(a => a.Subject)
                .Include(a => a.StudentClass)
                .Include(a => a.StudentGrade)
                .Where(a => a.StudentId == studentId);

            if (!includeRead)
            {
                query = query.Where(a => !a.IsRead);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetUnreadStudentAlertsAsync(string studentId)
        {
            return await _context.Alerts
                .Include(a => a.Subject)
                .Include(a => a.StudentClass)
                .Include(a => a.StudentGrade)
                .Where(a => a.StudentId == studentId && !a.IsRead)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string studentId)
        {
            return await _context.Alerts
                .Where(a => a.StudentId == studentId && !a.IsRead)
                .CountAsync();
        }

        public async Task<Alert?> GetAlertWithDetailsAsync(int alertId)
        {
            return await _context.Alerts
                .Include(a => a.Student)
                .Include(a => a.Subject)
                .Include(a => a.StudentClass)
                .Include(a => a.StudentGrade)
                .FirstOrDefaultAsync(a => a.Id == alertId);
        }

        public async Task MarkAsReadAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert != null && !alert.IsRead)
            {
                alert.IsRead = true;
                alert.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkMultipleAsReadAsync(List<int> alertIds)
        {
            var alerts = await _context.Alerts
                .Where(a => alertIds.Contains(a.Id) && !a.IsRead)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                alert.IsRead = true;
                alert.ReadAt = DateTime.UtcNow;
            }

            if (alerts.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> GetRecentAlertsAsync(string studentId, int count = 10)
        {
            return await _context.Alerts
                .Include(a => a.Subject)
                .Include(a => a.StudentClass)
                .Include(a => a.StudentGrade)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> AlertExistsAsync(string studentId, int type, int? subjectId = null, int? gradeId = null)
        {
            var query = _context.Alerts
                .Where(a => a.StudentId == studentId && (int)a.Type == type);

            if (subjectId.HasValue)
            {
                query = query.Where(a => a.SubjectId == subjectId.Value);
            }

            if (gradeId.HasValue)
            {
                query = query.Where(a => a.StudentGradeId == gradeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
