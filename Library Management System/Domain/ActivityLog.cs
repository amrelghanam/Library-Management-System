namespace Library_Management_System.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Action { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Details { get; set; }
    }
}
