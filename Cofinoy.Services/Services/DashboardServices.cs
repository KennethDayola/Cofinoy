using Cofinoy.Data.Interfaces;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Services
{
   
    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IOrderRepository orderRepository,
            ILogger<DashboardService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public DashboardServiceModel GetDashboardData()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                // Get orders for calculations
                var todayOrders = _orderRepository.GetOrdersByDate(today).ToList();
                var yesterdayOrders = _orderRepository.GetOrdersByDate(yesterday).ToList();

                // Calculate revenue metrics
                var revenueToday = CalculateRevenue(todayOrders);
                var revenueYesterday = CalculateRevenue(yesterdayOrders);
                var totalRevenue = _orderRepository.GetTotalRevenue();

                // Calculate order metrics
                var ordersToday = todayOrders.Count;
                var ordersYesterday = yesterdayOrders.Count;
                var activeOrders = CountActiveOrders(todayOrders);
                var completedOrdersToday = CountOrdersByStatus(todayOrders, "Served");
                var cancelledOrdersToday = CountOrdersByStatus(todayOrders, "Cancelled");

                // Calculate percentage changes
                var revenueTodayChange = CalculatePercentageChange(revenueToday, revenueYesterday);
                var ordersTodayChange = CalculatePercentageChange(ordersToday, ordersYesterday);

                // Get recent orders list
                var recentOrders = BuildRecentOrdersList(todayOrders);

                // ✅ Return YOUR existing DashboardViewModel with all properties populated
                return new DashboardServiceModel
                {
                    // Revenue Stats - matches your model
                    RevenueToday = revenueToday,
                    TotalRevenue = totalRevenue,
                    RevenueTodayChange = Math.Round(revenueTodayChange, 1),

                    // Order Stats - matches your model
                    TotalOrdersToday = ordersToday,
                    ActiveOrders = activeOrders,
                    CompletedOrdersToday = completedOrdersToday,
                    CancelledOrdersToday = cancelledOrdersToday,
                    OrdersTodayChange = Math.Round(ordersTodayChange, 1),

                    // Recent Orders - matches your model (List<DashboardOrderItem>)
                    RecentOrders = recentOrders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating dashboard data");
                throw;
            }
        }

        public (decimal today, decimal yesterday, decimal total) GetRevenueStats()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                var todayOrders = _orderRepository.GetOrdersByDate(today).ToList();
                var yesterdayOrders = _orderRepository.GetOrdersByDate(yesterday).ToList();

                var revenueToday = CalculateRevenue(todayOrders);
                var revenueYesterday = CalculateRevenue(yesterdayOrders);
                var totalRevenue = _orderRepository.GetTotalRevenue();

                return (revenueToday, revenueYesterday, totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue stats");
                throw;
            }
        }

        public (int total, int active, int completed, int cancelled) GetOrderStats()
        {
            try
            {
                var todayOrders = _orderRepository.GetOrdersByDate(DateTime.Today).ToList();

                var total = todayOrders.Count;
                var active = CountActiveOrders(todayOrders);
                var completed = CountOrdersByStatus(todayOrders, "Served");
                var cancelled = CountOrdersByStatus(todayOrders, "Cancelled");

                return (total, active, completed, cancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating order stats");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Calculates total revenue excluding cancelled orders
        /// </summary>
        private decimal CalculateRevenue(List<Cofinoy.Data.Models.Order> orders)
        {
            return orders
                .Where(o => o.Status != "Cancelled")
                .Sum(o => o.TotalPrice);
        }

        /// <summary>
        /// Counts orders that are not completed or cancelled
        /// </summary>
        private int CountActiveOrders(List<Cofinoy.Data.Models.Order> orders)
        {
            return orders.Count(o =>
                o.Status != "Served" &&
                o.Status != "Cancelled");
        }

        /// <summary>
        /// Counts orders with a specific status
        /// </summary>
        private int CountOrdersByStatus(List<Cofinoy.Data.Models.Order> orders, string status)
        {
            return orders.Count(o => o.Status == status);
        }

        /// <summary>
        /// Calculates percentage change between two decimal values
        /// </summary>
        private decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return 0;

            return ((current - previous) / previous) * 100;
        }

        /// <summary>
        /// Calculates percentage change between two integer values
        /// </summary>
        private decimal CalculatePercentageChange(int current, int previous)
        {
            if (previous == 0)
                return 0;

            return ((decimal)(current - previous) / previous) * 100;
        }

        /// <summary>
        /// Builds list of DashboardOrderItem matching YOUR existing model
        /// Populates: Id, InvoiceNumber, CustomerName, OrderTime, ItemCount, TotalPrice, Status
        /// </summary>
        private List<DashboardOrderItem> BuildRecentOrdersList(List<Cofinoy.Data.Models.Order> orders)
        {
            return orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new DashboardOrderItem
                {
                    // ✅ All properties match YOUR DashboardOrderItem model
                    Id = o.Id,
                    InvoiceNumber = o.InvoiceNumber,
                    CustomerName = !string.IsNullOrEmpty(o.Nickname) ? o.Nickname : "Guest",
                    OrderTime = o.OrderDate.ToString("h:mm tt"),
                    ItemCount = o.OrderItems?.Count ?? 0,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status ?? "Pending"
                })
                .ToList();
        }

        #endregion
    }
}