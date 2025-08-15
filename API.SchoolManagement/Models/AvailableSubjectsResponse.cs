namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Response model for available subjects
    /// </summary>
    public class AvailableSubjectsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AvailableSubject> Results { get; set; } = new List<AvailableSubject>();
    }
}
