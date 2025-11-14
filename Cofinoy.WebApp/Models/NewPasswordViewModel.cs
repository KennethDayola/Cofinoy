using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class NewPasswordViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).*$",
            ErrorMessage = "Password must include A-Z, a-z, & @#$%.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}

