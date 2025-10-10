using System;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class SendCodeViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        public string ResetCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }
    }
}
