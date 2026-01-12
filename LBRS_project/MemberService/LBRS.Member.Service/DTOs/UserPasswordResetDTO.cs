using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBRS.Member.Service.DTOs
{
    public class UserPasswordResetDTO
    {

        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [MinLength(6, ErrorMessage = "Current password must be at least 6 characters long")]
        public string CurrentPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "New and confirm Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
