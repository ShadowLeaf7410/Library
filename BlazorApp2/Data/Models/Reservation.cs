namespace BlazorApp2.Data.Models
{
    public class Reservation
    {
        public Guid ReservationId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = "Pending";
        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
}
