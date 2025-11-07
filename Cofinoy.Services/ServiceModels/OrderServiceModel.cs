using System;
using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{
    public class OrderServiceModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string UserId { get; set; }
        public string Nickname { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string AdditionalRequest { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemServiceModel> OrderItems { get; set; } = new List<OrderItemServiceModel>();
    }

    public class OrderItemServiceModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int? ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }
    }

    public class OrderDetailsServiceModel : OrderServiceModel
    {
        public string CustomerName { get; set; }
        public CustomerInfoServiceModel CustomerInfo { get; set; }
    }

    public class CustomerInfoServiceModel
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
}