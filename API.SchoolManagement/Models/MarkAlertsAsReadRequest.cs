namespace API.SchoolManagement.Models
{
    public class MarkAlertsAsReadRequest
    {
        public List<int> AlertIds { get; set; } = new List<int>();
    }
}
