using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        public string Nickname { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string postalCode { get; set; }
    }
}
