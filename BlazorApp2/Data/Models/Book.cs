namespace BlazorApp2.Data.Models
{
    public class Book
    {
        public Guid BookId { get; set; } = Guid.NewGuid();
        public string? ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; } = 1;
        public int? AvailableCopies { get; set; } = 1;
        public string? Location { get; set; }
        public string Status { get; set; } = "Available";
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public virtual ICollection<Borrowing> Borrowings { get; set; } = new HashSet<Borrowing>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    }
}
