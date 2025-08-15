namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Enrollment request information
    /// </summary>
    public class EnrollmentRequestInfo
    {
        public int RequestId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string? ResponseMessage { get; set; }
        public string? ProcessedByName { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}
