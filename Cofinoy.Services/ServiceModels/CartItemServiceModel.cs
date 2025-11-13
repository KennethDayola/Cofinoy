using System;
using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{
    public class CartItemServiceModel
    {
        public string CartItemId { get; set; } // Unique ID for this specific cart item
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        // Customization properties (legacy support)
        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }

        // New: Store customizations as collection
        public List<CustomizationData> Customizations { get; set; } = new List<CustomizationData>();

        // Calculated property - this ensures consistency
        public decimal TotalPrice => UnitPrice * Quantity;

      

    }

    // Class to store customization details
    public class CustomizationData
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal Price { get; set; }
    }
}