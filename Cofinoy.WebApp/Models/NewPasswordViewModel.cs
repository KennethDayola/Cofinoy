using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class NewPasswordViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
