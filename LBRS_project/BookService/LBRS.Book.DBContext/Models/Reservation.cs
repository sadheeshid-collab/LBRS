namespace LBRS.Book.DBContext.Models
{
    public class Reservation
    {

        public Guid ReservationId { get; set; }

        public Guid BookDetailId { get; set; }

        public Guid UserId { get; set; }

        public string? Remarks { get; set; }

        public virtual ICollection<ReservationStatus> ReservationStatuses { get; set; } = new List<ReservationStatus>(); 

        public virtual BookDetail BookDetail { get; set; } = null!; 

        

    }
}
