using LBRS.Member.DBContext.Enums;
using System.ComponentModel.DataAnnotations;

namespace LBRS.Member.Service.DTOs
{
    public class UserAddDTO
    {
        private string _userName = string.Empty;

        [Required(ErrorMessage = "Username is required. Enter your email ID")]
        [RegularExpression(@"^[^@\s]+@([a-z0-9-]+\.)+[a-z]{2,}$", ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Username
        {
            get => _userName;
            set => _userName = value?.Trim().ToLower() ?? string.Empty;
        }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Confirm passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;


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
