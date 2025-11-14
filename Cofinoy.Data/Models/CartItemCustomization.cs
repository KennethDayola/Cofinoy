using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cofinoy.Data.Models
{
    public class CartItemCustomization
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CartItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Value { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } 

        public int? DisplayOrder { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        [ForeignKey("CartItemId")]
        public virtual CartItem CartItem { get; set; }
    }
}