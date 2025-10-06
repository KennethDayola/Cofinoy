using System;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.Services.ServiceModels
{
    public class CategoryServiceModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public int ItemsCount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } // "Active" or "Inactive"

        public DateTime CreatedAt { get; set; }
    }
}