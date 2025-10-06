using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Data.Models
{
    public class ProductCustomization
    {
        [Required]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string CustomizationId { get; set; }
        public Customization Customization { get; set; }
    }
}