using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cofinoy.WebApp.Models
{
    /// <summary>
    /// Login View Model
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>ユーザーID</summary>
        [JsonPropertyName("email")]
        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
        /// <summary>パスワード</summary>
        [JsonPropertyName("password")]
        [Required(ErrorMessage = "This field is required.")]
        public string Password { get; set; }
    }
}
