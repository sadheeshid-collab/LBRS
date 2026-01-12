using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookAPIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;


        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddBook([FromBody] BookDetailAddDTO bookAddDto)
        {
            try
            {
                var checkBookCreated = await _bookService.Add(bookAddDto);

                switch (checkBookCreated.Item1)
                {
                    case OperationStatusTypes.Failure:
                        return BadRequest(new
                        {
                            success = false,
                            message = "Book details could not be created."
                        });

                    case OperationStatusTypes.DuplicateEntry:
                        return Conflict(new
                        {
                            success = false,
                            message = "Book with the same ISBN already exists."
                        });

                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "Book details created successfully.",
                            BookID = checkBookCreated.Item2
                        });

                    default:
                        return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Add Book.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception: Add Book..");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateBook([FromBody] BookDetailUpdateDTO bookUpdateDto)
        {
            try
            {
                var updateResult = await _bookService.Update(bookUpdateDto);
                switch (updateResult)
                {
                    case OperationStatusTypes.Failure:
                        return BadRequest(new
                        {
                            success = false,
                            message = "Book details could not be updated."
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "Book not found."
                        });

                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "Book details updated successfully."
                        });

                    default:
                        return NoContent();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Update book with ID {BookId}", bookUpdateDto.BookID);
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception: Update book");
            }
        }


        [Authorize(Roles = "Member")] 
        [HttpGet("searchbooks")]
        public async Task<IActionResult> SearchBooks([FromQuery][Required] string genre, [FromQuery][Required] string author,
                                                    int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var books = await _bookService.SearchBooks(genre, author, pageNumber, pageSize);
                if (books.Count() == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No books found matching the search criteria.",
                        data = (IEnumerable<BookDetailAddDTO>?)null
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Books retrieved successfully.",
                    data = books
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception :  Search books with Genre: {genre}, Author: {author}", genre, author);
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception :  Search books");
            }
        }


        [Authorize] // Both Admin and User roles can access
        [HttpGet("get/{bookID}")]
        public async Task<IActionResult> GetBookById(Guid bookID)
        {
            try
            {
                var book = await _bookService.GetById(bookID);
                if (book == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Book not found.",
                        data = (BookDetailAddDTO?)null
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Book retrieved successfully.",
                    data = book
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception: Get book by ID {bookID}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception: Get book byID");
            }
        }

        [Authorize]
        [HttpGet("getall")]
        /// <summary>   
        /// Retrieves a paginated list of all books.
        /// If pageSize is set to 0, all books are returned.
        /// </summary>
        public async Task<IActionResult> GetAllBooks(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var books = await _bookService.GetAll(pageNumber, pageSize);

                if (books == null || !books.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No books found.",
                        data = (IEnumerable<BookDetailAddDTO>?)null
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Books retrieved successfully.",
                    data = books
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetAll");
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception: GetAll.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{bookID}")]
        public async Task<IActionResult> DeleteBook(Guid bookID)
        {
            try
            {
                var deleteResult = await _bookService.Delete(bookID);
                switch (deleteResult)
                {
                    case OperationStatusTypes.Failure:
                        return BadRequest(new 
                        {
                            success = false,
                            message = "Book could not be deleted."
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "Book not found."

                        });

                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "Book deleted successfully."
                        });
                    default:
                        return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Delete book with ID {BookId}", bookID);
                return StatusCode(StatusCodes.Status500InternalServerError, "Exception: Delete book");
            }
        }
    }
}
