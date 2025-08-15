using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Repositories
{
    public class SubjectEnrollmentRequestRepository : GenericRepository<SubjectEnrollmentRequest>, ISubjectEnrollmentRequestRepository
    {
        private readonly DataContext _context;

        public SubjectEnrollmentRequestRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SubjectEnrollmentRequest>> GetPendingRequestsAsync()
        {
            return await _context.SubjectEnrollmentRequests
                .Where(r => r.Status == EnrollmentRequestStatus.Pending)
                .Include(r => r.Student)
                .Include(r => r.Subject)
                .OrderBy(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<List<SubjectEnrollmentRequest>> GetRequestsByStudentAsync(string studentId)
        {
            return await _context.SubjectEnrollmentRequests
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Subject)
                .Include(r => r.ProcessedBy)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<List<Subject>> GetAvailableSubjectsForStudentAsync(string studentId)
        {
            // Get student's current course subjects
            var studentProfile = await _context.StudentProfiles
                .Include(sp => sp.StudentClass)
                .FirstOrDefaultAsync(sp => sp.UserId == studentId);

            if (studentProfile?.StudentClass == null)
                return new List<Subject>();

            var courseSubjectIds = await _context.CourseSubjects
                .Where(cs => cs.CourseId == studentProfile.StudentClass.CourseId)
                .Select(cs => cs.SubjectId)
                .ToListAsync();

            // Get all subjects that are NOT in the student's course
            return await _context.Subjects
                .Where(s => !courseSubjectIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequestForSubjectAsync(string studentId, int subjectId)
        {
            return await _context.SubjectEnrollmentRequests
                .AnyAsync(r => r.StudentId == studentId && 
                              r.SubjectId == subjectId && 
                              r.Status == EnrollmentRequestStatus.Pending);
        }

        public async Task<List<SubjectEnrollmentRequest>> GetRequestsByStatusAsync(EnrollmentRequestStatus status)
        {
            return await _context.SubjectEnrollmentRequests
                .Where(r => r.Status == status)
                .Include(r => r.Student)
                .Include(r => r.Subject)
                .Include(r => r.ProcessedBy)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<SubjectEnrollmentRequest?> GetRequestWithDetailsAsync(int requestId)
        {
            return await _context.SubjectEnrollmentRequests
                .Include(r => r.Student)
                .Include(r => r.Subject)
                .Include(r => r.ProcessedBy)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task ProcessRequestAsync(int requestId, EnrollmentRequestStatus status, string responseMessage, string processedById)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return;

            request.Status = status;
            request.ResponseMessage = responseMessage;
            request.ProcessedById = processedById;
            request.ProcessedDate = DateTime.UtcNow;

            await UpdateAsync(request);
        }
    }
}
