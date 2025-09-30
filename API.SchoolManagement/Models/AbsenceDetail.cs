namespace API.SchoolManagement.Models
{
    /// <summary>
    /// Absence detail information
    /// </summary>
    public class AbsenceDetail
    {
        public DateTime Date { get; set; }
        public string Justification { get; set; } = string.Empty;
        public bool IsJustified { get; set; }
    }
}
