using LBRS.Book.DBContext;
using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LBRS.Book.Repo
{
    public class BookRepository : IBookRepository
    {
        private readonly BookServiceDbContext _context;
        private readonly ILogger<BookRepository> _logger;

        public BookRepository(BookServiceDbContext context, ILogger<BookRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool, Guid)> Add(BookDetail bookDetail)
        {
            try
            {
                await _context.BookDetails.AddAsync(bookDetail);
                var isAdded = await _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);

                return(isAdded, !isAdded ? Guid.Empty : bookDetail.BookDetailId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Add new book: {Title}", bookDetail.Title);
                throw;
            }
        }

        public async Task<bool> Update(BookDetail bookDetail)
        {
            try
            {
                _context.BookDetails.Update(bookDetail);
                 return await _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Update book ID: {BookId}, Title {Title}", bookDetail.BookDetailId, bookDetail.Title);
                throw;
            }
        }

        public async Task<IEnumerable<BookDetail>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageSize == 0)
                {
                    var getAllBooks = await _context.BookDetails
                                        .AsNoTracking()
                                        .OrderBy(b => b.Title)
                                        .ToListAsync();
                    return getAllBooks;
                }

                var getBooks = await _context.BookDetails
                                    .AsNoTracking()
                                    .OrderBy(b => b.Title)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
                return getBooks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Getall books");
                throw;
            }
        }

        public async Task<BookDetail?> GetById(Guid bookID)
        {
            try
            {
                return await _context.BookDetails
                            .AsNoTracking()
                            .FirstOrDefaultAsync(b => b.BookDetailId == bookID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetbyID: {BookId}", bookID);
                throw;
            }
        }

        public async Task<bool> IsIsbnExistsAsync(string isbn)
        {
            try
            {
                return await _context.BookDetails
                            .AsNoTracking()
                            .AnyAsync(b => b.ISBN == isbn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Check ISBN exists: {ISBN}", isbn);
                throw;
            }
        }

        public async Task<IEnumerable<BookDetail>> SearchBooks(string genre, string author, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                return  await _context.BookDetails
                              .AsNoTracking()
                              .Where(b => b.Genre.ToLower().Contains(genre) &&
                                          b.Author.ToLower().Contains(author) &&
                                          b.IsActive == true)
                              .OrderBy(t => t.Title)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: search books with Genre: {Genre} and Author: {Author}", genre, author);
                throw;
            }
        }
    }
}
