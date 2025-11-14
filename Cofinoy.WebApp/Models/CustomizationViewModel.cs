using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class CustomizationViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Customization name is required")]
        [StringLength(100, ErrorMessage = "Customization name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } // single_select, multi_select, quantity

        public bool Required { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Range(1, 100, ErrorMessage = "Max quantity must be between 1 and 100")]
        public int? MaxQuantity { get; set; }

        [Range(0, 1000, ErrorMessage = "Price per unit must be between 0 and 1000")]
        public decimal PricePerUnit { get; set; }

        public List<CustomizationOptionViewModel> Options { get; set; } = new List<CustomizationOptionViewModel>();
    }
}
