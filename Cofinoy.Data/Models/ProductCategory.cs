using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Data.Models
{
    public class ProductCategory
    {
        [Required]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
