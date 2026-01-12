using LBRS.Member.DBContext.Enums;

namespace LBRS.Member.DBContext.Models
{
    public class User
    {


        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        public UserRoleTypes UserRoleType { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public Guid CreatedByUserID { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid? UpdatedByUserID { get; set; }
    }
}
