using LBRS.Book.DBContext.Models;
using LBRS.Book.Service.DTOs;

namespace LBRS.Book.Service.Interfaces
{
    public interface IReservationService
    {

        Task<(OperationStatusTypes, Guid)> Add(ReservationDTO reservationDto);

        Task<ReservationViewDTO?> GetReservationById(Guid reservationId);

        Task<IEnumerable<ReservationViewDTO?>> GetAllReservations();
    }
}
