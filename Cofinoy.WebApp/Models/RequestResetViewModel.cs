using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class RequestResetViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
    }
}
