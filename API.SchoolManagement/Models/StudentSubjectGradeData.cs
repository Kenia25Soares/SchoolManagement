namespace API.SchoolManagement.Models
{
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
}
