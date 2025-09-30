namespace API.SchoolManagement.Models
{
    public class AlertInfo
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? SubjectName { get; set; }
        public string? ClassName { get; set; }
        public string? Metadata { get; set; }
    }
}
