using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class AddressViewModel
    {
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters.")]
        public string Country { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Postal code must contain only numbers.")]
        public string PostalCode { get; set; }
    }
}