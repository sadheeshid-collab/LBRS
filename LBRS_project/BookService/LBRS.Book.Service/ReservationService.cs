using LBRS.Book.DBContext.Enums;
using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Helper;
using LBRS.Book.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace LBRS.Book.Service
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<ReservationService> _logger;
        private readonly IHttpClaimContext _httpClaimContext;

        public ReservationService(IReservationRepository reservationRepository,
                IHttpClaimContext httpClaimContext,
                ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _httpClaimContext = httpClaimContext;
            _logger = logger;
        }


        public async Task<(OperationStatusTypes, Guid)> Add(ReservationDTO reservationDto)
        {
            try
            {
                /// Condition#1: Validate if the book is available for reservation                 
                var checkBookAvailablity = await _reservationRepository.checkBookAvailability(reservationDto.BookID);

                if (!checkBookAvailablity)
                {
                    return (OperationStatusTypes.NotFound, Guid.Empty);
                }

                /// Condition#2: Validate if the book is already reserved by the same user
                var checkBookAlreadyReservedByUser = await _reservationRepository.checkBookAlreadyReservedByUser(reservationDto.BookID
                                                                , _httpClaimContext.UserId);

                if (checkBookAlreadyReservedByUser)
                {
                    return (OperationStatusTypes.DuplicateEntry, Guid.Empty);
                }

                var reservation = new Reservation
                {
                    BookDetailId = reservationDto.BookID,
                    UserId = _httpClaimContext.UserId,
                    Remarks = reservationDto.Remarks,
                    ReservationStatuses = new List<ReservationStatus>()
                    {
                        new ReservationStatus
                        {
                            ReservationStatusType = ReservationStatusTypes.Reserved,
                            CreatedDate = DateTime.UtcNow,
                            CreatedByUserID = _httpClaimContext.UserId // get it from claim
                        }
                    }
                };

                var checkRecCreated = await _reservationRepository.Add(reservation);

                return (checkRecCreated.Item1 ? OperationStatusTypes.Success : OperationStatusTypes.Failure, checkRecCreated.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Add reservation");
                throw;
            }
        }

        public async Task<IEnumerable<ReservationViewDTO?>> GetReservedBooks()
        {
            try
            {
                var reservationList = await _reservationRepository.GetReservedBooks(_httpClaimContext.UserId);

                if (reservationList == null || !reservationList.Any())
                {
                    return Enumerable.Empty<ReservationViewDTO?>();
                }

                var reservationViewDTOList = reservationList.Select(
                    r => new ReservationViewDTO
                    {
                        ReservationId = r.ReservationId,
                        UserId = r.UserId,
                        Remarks = r.Remarks,

                        BookDetailResViewDTO = new BookDetailResViewDTO
                        {
                            BookId  = r.BookDetail.BookDetailId,
                            Title = r.BookDetail.Title,
                            ISBN = r.BookDetail.ISBN,
                            Author = r.BookDetail.Author,
                            Genre = r.BookDetail.Genre,
                            PublishedYear = r.BookDetail.PublishedYear,
                            Publisher = r.BookDetail.Publisher,
                            Description = r.BookDetail.Description
                        },

                        ReservationStatusListDTO = r.ReservationStatuses.Select(rs => new ReservationStatusViewDTO
                        {
                            ReservationStatusId = rs.ReservationStatusId,
                            ReservationStatusType = rs.ReservationStatusType.ToString(),
                            CreatedDate = rs.CreatedDate,
                            CreatedByUserID = rs.CreatedByUserID
                        }).ToList()

                    });

                return reservationViewDTOList;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetAllReservations");
                throw ex;
            }
        }

        public async Task<ReservationViewDTO?> GetReservationById(Guid reservationId)
        {
            throw new NotImplementedException();
        }
    }
}
