using FluentAssertions;
using LBRS.Book.DBContext.Enums;
using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using LBRS.Book.Service;
using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Helper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Book.Service.Tests
{
    public class ReservationServiceTests
    {
        private readonly Mock<IReservationRepository> _reservationRepository_Mock;
        private readonly Mock<ILogger<ReservationService>> _logger_Mock;
        private readonly Mock<IHttpClaimContext> _httpClaimContext_Mock;

        private readonly ReservationService _reservationService;




        public ReservationServiceTests()
        {
            _reservationRepository_Mock = new Mock<IReservationRepository>();
            _logger_Mock = new Mock<ILogger<ReservationService>>();
            _httpClaimContext_Mock = new Mock<IHttpClaimContext>();


            _reservationService = new ReservationService(
                _reservationRepository_Mock.Object,
                _httpClaimContext_Mock.Object,
                _logger_Mock.Object);
        }


        [Fact]
        public async Task Reservation_Add_Should_Return_Success_When_Valid()
        {
            var userID = Guid.NewGuid();
            var reservationID = Guid.NewGuid();

            var dto = new ReservationDTO
            {
                BookID = Guid.NewGuid(),
                Remarks = "Reserve book"
            };

            _httpClaimContext_Mock.Setup(x => x.UserId).Returns(userID);

            _reservationRepository_Mock.Setup(x => x.checkBookAvailability(dto.BookID))
                .ReturnsAsync(true);

            _reservationRepository_Mock.Setup(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID))
                .ReturnsAsync(false);

            _reservationRepository_Mock.Setup(x => x.Add(It.IsAny<Reservation>()))
                .ReturnsAsync((true, reservationID));



            var resData = await _reservationService.Add(dto);



            resData.Item1.Should().Be(OperationStatusTypes.Success);
            resData.Item2.Should().Be(reservationID);

            _reservationRepository_Mock.Verify(x => x.checkBookAvailability(dto.BookID), Times.Once);
            _reservationRepository_Mock.Verify(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID), Times.Once);
            _httpClaimContext_Mock.Verify(x => x.UserId, Times.AtLeastOnce);

            _reservationRepository_Mock.Verify(x => x.Add(It.Is<Reservation>(
                r =>
                    r.BookDetailId == dto.BookID &&
                    r.UserId == userID &&
                    r.Remarks == dto.Remarks &&
                    r.ReservationStatuses.Count == 1 &&
                    r.ReservationStatuses.First().ReservationStatusType == ReservationStatusTypes.Reserved
            )), Times.Once);
        }


        [Fact]
        public async Task Reservation_Add_Should_Return_Not_Found_When_Book_Not_Available()
        {

            var dto = new ReservationDTO
            {
                BookID = Guid.NewGuid(),
                Remarks = "Reserve book"
            };


            _reservationRepository_Mock.Setup(x => x.checkBookAvailability(dto.BookID))
                .ReturnsAsync(false);


            var resData = await _reservationService.Add(dto);



            resData.Item1.Should().Be(OperationStatusTypes.NotFound);
            resData.Item2.Should().BeEmpty();

            _reservationRepository_Mock.Verify(x => x.checkBookAvailability(dto.BookID), Times.Once);


        }


        [Fact]
        public async Task Reservation_Add_Should_Return_Duplicate_Entry_When_Book_Already_Reserved_By_User()
        {
            var userID = Guid.NewGuid();

            var dto = new ReservationDTO
            {
                BookID = Guid.NewGuid(),
                Remarks = "Reserve book"
            };

            _httpClaimContext_Mock.Setup(x => x.UserId).Returns(userID);

            _reservationRepository_Mock.Setup(x => x.checkBookAvailability(dto.BookID))
                .ReturnsAsync(true);

            _reservationRepository_Mock.Setup(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID))
                .ReturnsAsync(true);


            var resData = await _reservationService.Add(dto);



            resData.Item1.Should().Be(OperationStatusTypes.DuplicateEntry);
            resData.Item2.Should().BeEmpty();

            _reservationRepository_Mock.Verify(x => x.checkBookAvailability(dto.BookID), Times.Once);
            _reservationRepository_Mock.Verify(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID), Times.Once);
            _httpClaimContext_Mock.Verify(x => x.UserId, Times.AtLeastOnce);


        }


        [Fact]
        public async Task Reservation_Add_Should_Return_Failure_When_Repo_Fails()
        {
            var userID = Guid.NewGuid();

            var dto = new ReservationDTO
            {
                BookID = Guid.NewGuid(),
                Remarks = "Reserve book"
            };

            _httpClaimContext_Mock.Setup(x => x.UserId).Returns(userID);

            _reservationRepository_Mock.Setup(x => x.checkBookAvailability(dto.BookID))
                .ReturnsAsync(true);

            _reservationRepository_Mock.Setup(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID))
                .ReturnsAsync(false);

            _reservationRepository_Mock.Setup(x => x.Add(It.IsAny<Reservation>()))
                .ReturnsAsync((false, Guid.Empty));



            var resData = await _reservationService.Add(dto);



            resData.Item1.Should().Be(OperationStatusTypes.Failure);
            resData.Item2.Should().BeEmpty();

            _reservationRepository_Mock.Verify(x => x.checkBookAvailability(dto.BookID), Times.Once);
            _reservationRepository_Mock.Verify(x => x.checkBookAlreadyReservedByUser(dto.BookID, userID), Times.Once);
            _httpClaimContext_Mock.Verify(x => x.UserId, Times.AtLeastOnce);

            _reservationRepository_Mock.Verify(x => x.Add(It.Is<Reservation>(
                r =>
                    r.BookDetailId == dto.BookID &&
                    r.UserId == userID &&
                    r.Remarks == dto.Remarks &&
                    r.ReservationStatuses.Count == 1 &&
                    r.ReservationStatuses.First().ReservationStatusType == ReservationStatusTypes.Reserved
            )), Times.Once);
        }
    }
}