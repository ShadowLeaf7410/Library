namespace BlazorApp2.Data.Models
{
    public class Borrowing
    {
        public Guid BorrowId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Borrowed";
        public decimal FineAmount { get; set; } = 0;
        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
        public virtual ICollection<Fine> Fines { get; set; } = new HashSet<Fine>();
    }
}
