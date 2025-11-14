using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).*$",
            ErrorMessage = "Password must include A-Z, a-z, & @#$%.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}