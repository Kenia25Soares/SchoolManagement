namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Response model for enrollment requests
    /// </summary>
    public class EnrollmentRequestsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EnrollmentRequestInfo> Results { get; set; } = new List<EnrollmentRequestInfo>();
    }
}
