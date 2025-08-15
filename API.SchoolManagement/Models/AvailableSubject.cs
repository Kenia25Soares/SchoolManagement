namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Available subject information
    /// </summary>
    public class AvailableSubject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int Workload { get; set; }
        public int AllowedAbsences { get; set; }
        public bool HasPendingRequest { get; set; }
    }
}
