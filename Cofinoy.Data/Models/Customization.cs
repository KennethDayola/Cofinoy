using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Data.Models
{
    public class Customization
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public bool Required { get; set; } = false;

        public int DisplayOrder { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(1, 100)]
        public int? MaxQuantity { get; set; }

        [Range(0, 1000)]
        public decimal PricePerUnit { get; set; } = 0;

        // One-to-many with options
        public ICollection<CustomizationOption> Options { get; set; } = new List<CustomizationOption>();

        // Many-to-many with products
        public ICollection<ProductCustomization> ProductCustomizations { get; set; } = new List<ProductCustomization>();
    }
}
