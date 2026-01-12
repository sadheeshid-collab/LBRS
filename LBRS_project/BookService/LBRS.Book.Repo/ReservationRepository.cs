using LBRS.Book.DBContext;
using LBRS.Book.DBContext.Enums;
using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LBRS.Book.Repo
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly BookServiceDbContext _context;
        private readonly ILogger<ReservationRepository> _logger;

        public ReservationRepository(BookServiceDbContext context, ILogger<ReservationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool, Guid)> Add(Reservation reservation)
        {
            try
            {
                _context.Reservations.Add(reservation);
                var isAdded = await _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);

                return (isAdded, !isAdded ? Guid.Empty : reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Add reservation");
                throw;
            }
        }

        public async Task<bool> checkBookAvailability(Guid bookDetailId)
        {
            try
            {
                var bookDetail = await _context.BookDetails
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(b => b.BookDetailId == bookDetailId
                                        && b.IsActive == true
                                        && b.AvailableCopies > 0);

                //Not available
                return bookDetail != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Check book availability for Book ID: {BookId}", bookDetailId);
                throw;
            }
        }

        public async Task<bool> checkBookAlreadyReservedByUser(Guid bookDetailId, Guid userId)
        {
            try
            {
                var existingReservation = await _context.Reservations
                    .AsNoTracking()
                    .Include(r => r.ReservationStatuses)
                    .Where(b => b.BookDetailId == bookDetailId
                            && b.UserId == userId
                            && b.BookDetail.AvailableCopies > 0
                            && (b.ReservationStatuses.OrderByDescending(c => c.CreatedDate)
                                .Select(c => c.ReservationStatusType)
                                .FirstOrDefault()) == ReservationStatusTypes.Reserved
                    )
                    .FirstOrDefaultAsync();

                return existingReservation != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Check existing reservation for Book ID: {BookId} & User ID: {UserId}", bookDetailId, userId);
                throw;
            }
        }

        public async Task<Reservation?> GetReservationById(Guid reservationId)
        {
            try
            {
                return await _context.Reservations
                            .AsNoTracking()
                            .Include(r => r.ReservationStatuses)
                            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetreservationByID: {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<IEnumerable<Reservation?>> GetAllReservations()
        {
            try
            {
                return await _context.Reservations
                            .AsNoTracking()
                            .Include(r => r.ReservationStatuses)
                            .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetAllReservations");
                throw;
            }
        }
    }


}
