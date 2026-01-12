using LBRS.Book.DBContext.Models;

namespace LBRS.Book.Repo.Interfaces
{
    public interface IBookRepository
    {
        Task<(bool, Guid)> Add(BookDetail bookDetail);

        Task<bool> Update(BookDetail bookDetail);

        //Task<bool> Delete(Guid bookID);

        Task<BookDetail?> GetById(Guid bookID);

        Task<IEnumerable<BookDetail>> GetAll(int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<BookDetail>> SearchBooks(string genre, string author, int pageNumber = 1, int pageSize = 10);

        Task<bool> IsIsbnExistsAsync(string isbn);
    }
}
