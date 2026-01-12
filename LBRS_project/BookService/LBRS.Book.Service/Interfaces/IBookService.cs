using LBRS.Book.DBContext.Models;
using LBRS.Book.Service.DTOs;

namespace LBRS.Book.Service.Interfaces
{
    public interface IBookService
    {
        Task<(OperationStatusTypes, Guid)> Add(BookDetailAddDTO bookDTO);

        Task<OperationStatusTypes> Update(BookDetailUpdateDTO bookDTO);

        Task<OperationStatusTypes> Delete(Guid bookID);

        Task<BookDetailViewDTO?> GetById(Guid bookID);

        Task<IEnumerable<BookDetailViewDTO>> GetAll(int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<BookDetailViewDTO>> SearchBooks(string genre, string author, int pageNumber = 1, int pageSize = 10);

    }
}
