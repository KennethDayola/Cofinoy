using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Services.ServiceModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "This field is required.")]

        public string Role { get; set; } = "User";
        public string Nickname { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).*$",
            ErrorMessage = "Password must include A-Z, a-z, & @#$%.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set;}
    }
}
