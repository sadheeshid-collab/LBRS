using LBRS.Book.DBContext.Enums;
using LBRS.Book.DBContext.Models;

namespace LBRS.Book.Service.DTOs
{
    public class ReservationStatusViewDTO
    {
        public Guid ReservationStatusId { get; set; }

        public string ReservationStatusType { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedByUserID { get; set; }

    }
}
