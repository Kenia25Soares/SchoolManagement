using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Models
{
    public class EnrollmentRequestsIndexViewModel
    {
        public List<SubjectEnrollmentRequest> Requests { get; set; } = new List<SubjectEnrollmentRequest>();
        public EnrollmentRequestStatus? SelectedStatus { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
