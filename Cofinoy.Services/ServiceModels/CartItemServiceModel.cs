using System;
using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{
    public class CartItemServiceModel
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; } // Remove duplicate - only keep this one
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        // Customization properties
        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }

        // Add this property to store customizations
        public List<CustomizationData> Customizations { get; set; } = new List<CustomizationData>();

        // Calculated property - this ensures consistency
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    // Add this class to store customization details
    public class CustomizationData
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}