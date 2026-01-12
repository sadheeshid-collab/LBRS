using LBRS.Member.DBContext.Enums;
using System.ComponentModel.DataAnnotations;

namespace LBRS.Member.Service.DTOs
{
    public class UserUpdateDTO
    {


        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(
            @"^\+[1-9]\d{1,3}\d{6,12}$",
            ErrorMessage = "Mobile number must include country code (e.g., +919876543210)"
        )]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName { get; set; }


        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName { get; set; }

    }
}
