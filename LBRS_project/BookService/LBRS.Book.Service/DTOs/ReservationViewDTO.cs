using LBRS.Book.DBContext.Enums;
using LBRS.Book.DBContext.Models;

namespace LBRS.Book.Service.DTOs
{
    public class ReservationViewDTO
    {
        public Guid ReservationId { get; set; }

        public Guid UserId { get; set; }

        public string? Remarks { get; set; }

        public virtual ICollection<ReservationStatusViewDTO> ReservationStatusListDTO { get; set; } = new List<ReservationStatusViewDTO>();

        public virtual BookDetailResViewDTO BookDetailResViewDTO { get; set; } = null!;

    }
}
