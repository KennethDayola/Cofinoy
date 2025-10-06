using Cofinoy.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Product
{
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    public decimal BasePrice { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string ImageUrl { get; set; }

    public string ImagePath { get; set; } // Add this for Firebase Storage path

    public int Stock { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Available"; // Available, Out of Stock, etc.

    public int DisplayOrder { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public ICollection<ProductCustomization> ProductCustomizations { get; set; } = new List<ProductCustomization>();
}