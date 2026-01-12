using LBRS.Member.DBContext.Enums;
using System.ComponentModel.DataAnnotations;

namespace LBRS.Member.Service.DTOs
{
    public class UserViewDTO
    {

        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;


        public string MobileNumber { get; set; } = string.Empty;

        public UserRoleTypes UserRoleType { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;


    }
}
