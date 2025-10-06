using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Data.Models
{
    public class CustomizationOption
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public decimal PriceModifier { get; set; } = 0;

        [StringLength(200)]
        public string Description { get; set; }

        public bool Default { get; set; } = false;

        public string CustomizationId { get; set; }
        public Customization Customization { get; set; }
    }
}