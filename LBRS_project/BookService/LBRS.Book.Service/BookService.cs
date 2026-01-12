using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Helper;
using LBRS.Book.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace LBRS.Book.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookService> _logger;
        private readonly IHttpClaimContext _httpClaimContext;

        public BookService(IBookRepository bookRepository, IHttpClaimContext httpClaimContext, ILogger<BookService> logger)
        {
            _bookRepository = bookRepository;
            _httpClaimContext = httpClaimContext;
            _logger = logger;
        }

        public async Task<(OperationStatusTypes, Guid)> Add(BookDetailAddDTO bookDTO)
        {
            try
            {
                // Duplicate validation
                if (await _bookRepository.IsIsbnExistsAsync(bookDTO.ISBN))
                {
                    return (OperationStatusTypes.DuplicateEntry, Guid.Empty);
                }

                var bookModel = new BookDetail
                {
                    Title = bookDTO.Title,
                    ISBN = bookDTO.ISBN,
                    Author = bookDTO.Author,
                    Genre = bookDTO.Genre,
                    PublishedYear = bookDTO.PublishedYear,
                    Publisher = bookDTO.Publisher,
                    Description = bookDTO.Description,
                    TotalCopies = bookDTO.TotalCopies,

                    AvailableCopies = bookDTO.TotalCopies,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByUserID = _httpClaimContext.UserId // from claim

                };

                var addedBook = await _bookRepository.Add(bookModel);

                return (addedBook.Item1 ? OperationStatusTypes.Success : OperationStatusTypes.Failure, addedBook.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Add new book: {Title}", bookDTO.Title);
                throw;
            }
        }

        public async Task<OperationStatusTypes> Update(BookDetailUpdateDTO bookDTO)
        {
            try
            {
                var existingBook = await _bookRepository.GetById(bookDTO.BookID);

                if (existingBook == null)
                {
                    return OperationStatusTypes.NotFound;
                }

                existingBook.Title = bookDTO.Title;
                existingBook.ISBN = bookDTO.ISBN;
                existingBook.Author = bookDTO.Author;
                existingBook.Genre = bookDTO.Genre;
                existingBook.PublishedYear = bookDTO.PublishedYear;
                existingBook.Publisher = bookDTO.Publisher;
                existingBook.Description = bookDTO.Description;
                existingBook.TotalCopies = bookDTO.TotalCopies;
                existingBook.AvailableCopies = bookDTO.AvailableCopies;
                existingBook.UpdatedDate = DateTime.UtcNow;
                existingBook.UpdatedByUserID = _httpClaimContext.UserId; // get it from claim

                var isUpdated = await _bookRepository.Update(existingBook);

                return isUpdated ? OperationStatusTypes.Success : OperationStatusTypes.Failure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Update book: {BookID}", bookDTO.BookID);
                throw;
            }
        }


        public async Task<IEnumerable<BookDetailViewDTO>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                // pageSize=0 => return all records
                if (pageSize < 0)
                    pageSize = 10;

                var getBooks = await _bookRepository.GetAll(pageNumber, pageSize);

                var bookViewDTOs = new List<BookDetailViewDTO>();

                foreach (var book in getBooks)
                {
                    var bookViewDTO = new BookDetailViewDTO
                    {
                        BookId = book.BookDetailId,
                        Title = book.Title,
                        ISBN = book.ISBN,
                        Author = book.Author,
                        Genre = book.Genre,
                        PublishedYear = book.PublishedYear,
                        Publisher = book.Publisher,
                        Description = book.Description,
                        TotalCopies = book.TotalCopies,
                        AvailableCopies = book.AvailableCopies,
                        IsActive = book.IsActive
                    };
                    bookViewDTOs.Add(bookViewDTO);
                }

                return bookViewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Get all books");
                throw;
            }
        }

        public async Task<BookDetailViewDTO?> GetById(Guid bookID)
        {
            try
            {
                var getBook = await _bookRepository.GetById(bookID);

                if (getBook == null)
                {
                    return null;
                }

                var bookViewDTO = new BookDetailViewDTO
                {
                    BookId = getBook.BookDetailId,
                    Title = getBook.Title,
                    ISBN = getBook.ISBN,
                    Author = getBook.Author,
                    Genre = getBook.Genre,
                    PublishedYear = getBook.PublishedYear,
                    Publisher = getBook.Publisher,
                    Description = getBook.Description,
                    TotalCopies = getBook.TotalCopies,
                    AvailableCopies = getBook.AvailableCopies,
                    IsActive = getBook.IsActive
                };

                return bookViewDTO;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetbookbyID");
                throw;
            }
        }

        public async Task<OperationStatusTypes> Delete(Guid bookID)
        {
            try
            {
                var existingBook = await _bookRepository.GetById(bookID);

                if (existingBook == null)
                {
                    return OperationStatusTypes.NotFound;
                }

                existingBook.IsActive = false;
                existingBook.UpdatedDate = DateTime.UtcNow;
                existingBook.UpdatedByUserID = _httpClaimContext.UserId; // get it from claim

                var isDeleted = await _bookRepository.Update(existingBook);
                return isDeleted ? OperationStatusTypes.Success : OperationStatusTypes.Failure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception:  Delete book: {BookID}", bookID);
                throw;
            }
        }

        public async Task<IEnumerable<BookDetailViewDTO>> SearchBooks(string genre, string author, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                var searchResults = await _bookRepository.SearchBooks(genre, author, pageNumber, pageSize);

                if (!searchResults.Any())
                {
                    return Enumerable.Empty<BookDetailViewDTO>();
                }

                var bookViewDTOs = new List<BookDetailViewDTO>();
                foreach (var book in searchResults)
                {
                    var bookViewDTO = new BookDetailViewDTO
                    {
                        BookId = book.BookDetailId,
                        Title = book.Title,
                        ISBN = book.ISBN,
                        Author = book.Author,
                        Genre = book.Genre,
                        PublishedYear = book.PublishedYear,
                        Publisher = book.Publisher,
                        Description = book.Description,
                        TotalCopies = book.TotalCopies,
                        AvailableCopies = book.AvailableCopies,
                        IsActive = book.IsActive
                    };
                    bookViewDTOs.Add(bookViewDTO);
                }
                return bookViewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Search books with Genre: {Genre} and Author: {Author}", genre, author);
                throw;
            }
        }
    }
}
