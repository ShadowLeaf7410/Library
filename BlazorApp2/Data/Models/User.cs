namespace BlazorApp2.Data.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; }
        public string Name { get; set; }
        public string? HashedPassword { get; set; }
        public string Status { get; set; } = "Active";
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<LoginToken> LoginTokens { get; set; } = new HashSet<LoginToken>();
        public virtual ICollection<Borrowing> Borrowings { get; set; } = new HashSet<Borrowing>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
        public virtual ICollection<Fine> Fines { get; set; } = new HashSet<Fine>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new HashSet<SystemLog>();
    }
}
