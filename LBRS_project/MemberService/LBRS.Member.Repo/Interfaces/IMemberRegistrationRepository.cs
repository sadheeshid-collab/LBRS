using LBRS.Member.DBContext.Models;

namespace LBRS.Member.Repo.Interfaces
{
    public interface IMemberRegistrationRepository
    {
        Task<(bool, Guid)> Add(User user);

        Task<bool> Update(User user);

        Task<User?> GetByUserId(Guid UserID);

        Task<IEnumerable<User>> GetAll(int pageNumber = 1, int pageSize = 10);

        Task<User?> CheckExistingUser(string email);



    }
}
