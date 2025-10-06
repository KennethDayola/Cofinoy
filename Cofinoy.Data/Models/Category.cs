using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Data.Models
{
    public class Category
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public int ItemsCount { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Many-to-many with products
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
