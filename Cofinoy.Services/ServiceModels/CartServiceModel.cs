using System;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Services.ServiceModels
{
    public class CartServiceModel
    {
        public List<CartItemServiceModel> Items { get; set; } = new List<CartItemServiceModel>();
        public decimal Subtotal => Items.Sum(i => i.TotalPrice);
        public decimal Discount { get; set; }
        public decimal Total => Subtotal - Discount;

        public string Nickname { get; set; }
        public string AdditionalRequest { get; set; }
        public string OrderType { get; set; }
        public string CouponCode { get; set; }
    }
}