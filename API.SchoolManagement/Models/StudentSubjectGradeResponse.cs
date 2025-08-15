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

    /// <summary>
    /// Student subject grade data
    /// </summary>
    public class StudentSubjectGradeData
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public double WeightedAverage { get; set; }
        public int TotalAbsences { get; set; }
        public int AllowedAbsences { get; set; }
        public bool FailedDueToAbsences { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<GradeDetail> GradeDetails { get; set; } = new List<GradeDetail>();
        public List<AbsenceDetail> AbsenceDetails { get; set; } = new List<AbsenceDetail>();
    }

    /// <summary>
    /// Grade detail information
    /// </summary>
    public class GradeDetail
    {
        public string Description { get; set; } = string.Empty;
        public double Grade { get; set; }
        public double Weight { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Absence detail information
    /// </summary>
    public class AbsenceDetail
    {
        public DateTime Date { get; set; }
        public string Justification { get; set; } = string.Empty;
        public bool IsJustified { get; set; }
    }

    /// <summary>
    /// Student status in subject
    /// </summary>
    public enum StudentStatus
    {
        Approved,
        Failed,
        ExcludedByAbsences,
        InProgress
    }
}
