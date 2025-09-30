namespace API.SchoolManagement.Models
{
    public class AlertsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AlertInfo> Alerts { get; set; } = new List<AlertInfo>();
        public int UnreadCount { get; set; }
    }
}
