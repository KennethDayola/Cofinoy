using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cofinoy.Data.Models
{
    public class OrderItemCustomization
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int OrderItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Value { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // e.g., "single_select", "multi_select", "quantity"

        // DisplayOrder from the Customization model - nullable to handle legacy data
        public int? DisplayOrder { get; set; }

        // Price for this customization (could be PricePerUnit * quantity or PriceModifier)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }
    }
}
