using LBRS.Book.DBContext.Enums;

namespace LBRS.Book.Service.DTOs
{
    public class ReservationViewDTO
    {

        public Guid BookID { get; set; }

        public Guid UserID { get; set; }

        public string? Remarks { get; set; }

        //public virtual ReservationStatusDTO reservationStatus { get; set; } = null!;

    }
}
