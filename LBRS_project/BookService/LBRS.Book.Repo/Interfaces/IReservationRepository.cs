using LBRS.Book.DBContext.Models;

namespace LBRS.Book.Repo.Interfaces
{
    public interface IReservationRepository
    {
        Task<(bool, Guid)> Add(Reservation reservation);

        Task<bool> checkBookAvailability(Guid bookDetailId);

        Task<bool> checkBookAlreadyReservedByUser(Guid bookDetailId, Guid userId);

        Task<Reservation?> GetReservationById(Guid reservationId);

        Task<IEnumerable<Reservation?>> GetReservedBooks(Guid UserID);

    }
}
