namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Response model for student subject grades and attendance
    /// </summary>
    public class StudentSubjectGradeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StudentSubjectGradeData? Result { get; set; }
    }
}
