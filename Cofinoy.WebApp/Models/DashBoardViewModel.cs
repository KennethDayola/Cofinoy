using System;
using System.Collections.Generic;

namespace Cofinoy.WebApp.Models
{
    public class DashboardViewModel
    {
        // Revenue Stats
        public decimal RevenueToday { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueTodayChange { get; set; } // Percentage change from yesterday

        // Order Stats
        public int TotalOrdersToday { get; set; }
        public int ActiveOrders { get; set; }
        public int CompletedOrdersToday { get; set; }
        public int CancelledOrdersToday { get; set; }
        public decimal OrdersTodayChange { get; set; } // Percentage change from yesterday

        // Recent Orders
        public List<DashboardOrderItem> RecentOrders { get; set; }

        public DashboardViewModel()
        {
            RecentOrders = new List<DashboardOrderItem>();
        }
    }

    public class DashboardOrderItem
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string OrderTime { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}