namespace API.SchoolManagement.Models
{
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
}
