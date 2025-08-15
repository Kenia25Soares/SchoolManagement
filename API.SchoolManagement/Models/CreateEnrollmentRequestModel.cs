namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Request model for creating a subject enrollment request
    /// </summary>
    public class CreateEnrollmentRequestModel
    {
        public string StudentId { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
