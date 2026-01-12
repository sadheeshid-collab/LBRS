using LBRS.Book.DBContext.Enums;

namespace LBRS.Book.DBContext.Models
{
    public class ReservationStatus
    {
        public Guid ReservationStatusId { get; set; }

        public Guid ReservationID { get; set; }

        public virtual Reservation Reservation { get; set; } = null!;

        public ReservationStatusTypes ReservationStatusType { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedByUserID { get; set; }

    }
}
