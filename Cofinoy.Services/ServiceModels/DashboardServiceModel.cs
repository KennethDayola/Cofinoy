using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Services.ServiceModels
{
    public class DashboardServiceModel
    {
        public decimal RevenueToday { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueTodayChange { get; set; }

        public int TotalOrdersToday { get; set; }
        public int ActiveOrders { get; set; }
        public int CompletedOrdersToday { get; set; }
        public int CancelledOrdersToday { get; set; }
        public decimal OrdersTodayChange { get; set; }

        public List<DashboardOrderItem> RecentOrders { get; set; }

        public DashboardServiceModel()
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