using System;
using System.Collections.Generic;
using Cofinoy.Services.ServiceModels; // Add this import

namespace Cofinoy.WebApp.Models
{
    public class CheckoutViewModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Nickname { get; set; }
        public string AdditionalRequest { get; set; }

        // Change from CartItem to CartItemServiceModel
        public List<CartItemServiceModel> CartItems { get; set; }

        public decimal OrderType { get; set; }
        public decimal CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class InvoiceItem
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}
