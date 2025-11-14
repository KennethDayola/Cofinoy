using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class CustomizationOptionViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Option name is required")]
        [StringLength(100, ErrorMessage = "Option name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Range(-1000, 1000, ErrorMessage = "Price modifier must be between -1000 and 1000")]
        public decimal PriceModifier { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        public bool Default { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; }
    }
}
