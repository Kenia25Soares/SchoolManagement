using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Repositories
{
    public interface ISubjectEnrollmentRequestRepository : IGenericRepository<SubjectEnrollmentRequest>
    {
        Task<List<SubjectEnrollmentRequest>> GetPendingRequestsAsync();
        Task<List<SubjectEnrollmentRequest>> GetRequestsByStudentAsync(string studentId);
        Task<List<Subject>> GetAvailableSubjectsForStudentAsync(string studentId);
        Task<bool> HasPendingRequestForSubjectAsync(string studentId, int subjectId);
        Task<List<SubjectEnrollmentRequest>> GetRequestsByStatusAsync(EnrollmentRequestStatus status);
        Task<SubjectEnrollmentRequest?> GetRequestWithDetailsAsync(int requestId);
        Task ProcessRequestAsync(int requestId, EnrollmentRequestStatus status, string responseMessage, string processedById);
    }
}
