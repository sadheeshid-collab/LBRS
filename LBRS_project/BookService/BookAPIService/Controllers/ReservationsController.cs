using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookAPIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;

        }

        [Authorize(Roles = "Member")]
        [HttpPost("add")]
        public async Task<IActionResult> AddReservation([FromBody] ReservationDTO reservationDto)
        {
            try
            {
                var checkReservationCreated = await _reservationService.Add(reservationDto);

                switch (checkReservationCreated.Item1)
                {   
                    case OperationStatusTypes.Failure:
                        return BadRequest(new
                        {
                            success = false,
                            Message = "Reservation could not be created."
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            Message = "Book is not available for reservation."

                        });

                    case OperationStatusTypes.DuplicateEntry:
                        return Conflict(new
                        {
                            success = false,
                            Message = "Book is already reserved by the same user."
                        });

                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            Message = "Reservation created successfully.",
                            ReservationID = checkReservationCreated.Item2

                        });                            
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, "Exception: AddReservation");
                }
                 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new reservation");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the reservation.");
            }
        }
    }
}
