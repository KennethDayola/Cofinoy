using System;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class PersonalInfoViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [StringLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
        public string Nickname { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Birthdate is required.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; }
    }
}
