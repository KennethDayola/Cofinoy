using System;

namespace Cofinoy.Services.ServiceModels
{
    public class CartItemServiceModel
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ImageUrl { get; set; }

        // Customization properties
        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }
    }
}