using System;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class PersonalInfoViewModel
    {
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Nickname is required.")]
        [StringLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
        public string Nickname { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DateNotInFuture(ErrorMessage = "Birth date cannot be in the future.")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must contain '@' followed by a domain with an extension (e.g., example@domain.com).")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string PhoneNumber { get; set; }
    }

    public class DateNotInFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Null is valid (optional field)
                return ValidationResult.Success;
            }

            DateTime date;

            // Handle both DateTime and DateTime? types
            if (value is DateTime dateTime)
            {
                date = dateTime;
            }
            else
            {
                return new ValidationResult("Invalid date format.");
            }

            // Check if date is in the future
            if (date.Date > DateTime.Now.Date)
            {
                return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
            }

            return ValidationResult.Success;
        }
    }
}