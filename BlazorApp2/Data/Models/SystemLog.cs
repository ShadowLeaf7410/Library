namespace BlazorApp2.Data.Models
{
    public class SystemLog
    {
        public Guid LogId { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public virtual User? User { get; set; }
    }
}
