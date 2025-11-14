using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cofinoy.Data.Models
{
    public class CartItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CartId { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string ImageUrl { get; set; }


        [MaxLength(50)]
        public string Size { get; set; }

        [MaxLength(50)]
        public string MilkType { get; set; }

        [MaxLength(50)]
        public string Temperature { get; set; }

        public int ExtraShots { get; set; }

        [MaxLength(50)]
        public string SweetnessLevel { get; set; }


        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public virtual ICollection<CartItemCustomization> Customizations { get; set; } = new List<CartItemCustomization>();
    }
}
