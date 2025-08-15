namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Generic response model for enrollment operations
    /// </summary>
    public class EnrollmentOperationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
