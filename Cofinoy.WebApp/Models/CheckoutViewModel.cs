using System;
using System.Collections.Generic;
using Cofinoy.Services.ServiceModels; 

namespace Cofinoy.WebApp.Models
{
    public class CheckoutViewModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Nickname { get; set; }
        public string AdditionalRequest { get; set; }
        public List<InvoiceItem> CartItems { get; set; }

        public decimal OrderType { get; set; }
        public decimal CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class InvoiceItem
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Legacy customization fields (kept for backward compatibility)
        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }
        
        // New: Store customizations as collection
        public List<CustomizationData> Customizations { get; set; } = new List<CustomizationData>();
    }
}
