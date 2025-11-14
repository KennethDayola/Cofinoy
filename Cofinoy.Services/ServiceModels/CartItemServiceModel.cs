using System;
using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{
    public class CartItemServiceModel
    {
        public string CartItemId { get; set; } 
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }

        public List<CustomizationData> Customizations { get; set; } = new List<CustomizationData>();

        public decimal TotalPrice => UnitPrice * Quantity;

      

    }
    public class CustomizationData
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal Price { get; set; }
    }
}