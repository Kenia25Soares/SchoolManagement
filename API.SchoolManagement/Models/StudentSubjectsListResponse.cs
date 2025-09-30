namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Response model for student subjects list
    /// </summary>
    public class StudentSubjectsListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StudentSubjectSummary> Results { get; set; } = new List<StudentSubjectSummary>();
    }
}
