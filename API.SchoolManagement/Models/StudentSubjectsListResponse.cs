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

    /// <summary>
    /// Summary information for each subject
    /// </summary>
    public class StudentSubjectSummary
    {
        public string SubjectId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public double? WeightedAverage { get; set; }
        public int TotalAbsences { get; set; }
        public int AllowedAbsences { get; set; }
        public bool FailedDueToAbsences { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
