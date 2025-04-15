namespace BlazorApp2.Data.Models
{
    public class Fine
    {
        public Guid FineId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BorrowId { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = "Unpaid";
        public virtual User User { get; set; }
        public virtual Borrowing Borrowing { get; set; }
    }
}
