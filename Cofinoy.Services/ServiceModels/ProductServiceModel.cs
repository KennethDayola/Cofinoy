using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Services.ServiceModels
{
    public class ProductServiceModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        public string Stock { get; set; }

        public string ImageUrl { get; set; }

        public string ImagePath { get; set; }

        public List<string> Categories { get; set; } = new List<string>();

        public List<string> Customizations { get; set; } = new List<string>();

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}