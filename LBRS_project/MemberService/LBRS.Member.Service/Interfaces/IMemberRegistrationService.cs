using LBRS.Member.DBContext.Enums;
using LBRS.Member.DBContext.Models;
using LBRS.Member.Service.DTOs;

namespace LBRS.Member.Service.Interfaces
{
    public interface IMemberRegistrationService
    {
        Task<(OperationStatusTypes, Guid)> Add(UserAddDTO user, UserRoleTypes userRoleType);

        Task<OperationStatusTypes> Update(UserUpdateDTO user);

        Task<UserViewDTO?> GetByUserId(Guid UserID);

        Task<IEnumerable<UserViewDTO>> GetAll(int pageNumber = 1, int pageSize = 10);

        Task<OperationStatusTypes> useraccountdeactivate(Guid UserID);

        Task<OperationStatusTypes> UserPasswordReset(UserPasswordResetDTO passwordResetDTO);

        Task<(OperationStatusTypes, string)> UserLogin(string userName, string password);

    }
}
