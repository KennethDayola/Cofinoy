using AutoMapper;
using Cofinoy.Data;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly CofinoyDbContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        public HomeController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            CofinoyDbContext context)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _context = context;
        }

        /// <summary>
        /// Returns Home View (Customer Landing Page)
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns Dashboard View (Admin Dashboard)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                // Get today's orders
                var todayOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.OrderDate.Date == today)
                    .ToListAsync();

                // Get yesterday's orders for comparison
                var yesterdayOrders = await _context.Orders
                    .Where(o => o.OrderDate.Date == yesterday)
                    .ToListAsync();

                // Get all orders for total revenue
                var allOrders = await _context.Orders.ToListAsync();

                // Calculate stats
                var revenueToday = todayOrders.Sum(o => o.TotalPrice);
                var revenueYesterday = yesterdayOrders.Sum(o => o.TotalPrice);
                var totalRevenue = allOrders.Sum(o => o.TotalPrice);

                var ordersToday = todayOrders.Count;
                var ordersYesterday = yesterdayOrders.Count;

                var activeOrders = todayOrders.Count(o =>
                    o.Status != "Served" &&
                    o.Status != "Cancelled");

                var completedOrdersToday = todayOrders.Count(o => o.Status == "Served");
                var cancelledOrdersToday = todayOrders.Count(o => o.Status == "Cancelled");

                // Calculate percentage changes
                var revenueTodayChange = revenueYesterday > 0
                    ? ((revenueToday - revenueYesterday) / revenueYesterday) * 100
                    : 0;

                var ordersTodayChange = ordersYesterday > 0
                    ? ((decimal)(ordersToday - ordersYesterday) / ordersYesterday) * 100
                    : 0;

                // Get recent orders (last 10)
                var recentOrders = todayOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new DashboardOrderItem
                    {
                        Id = o.Id,
                        InvoiceNumber = o.InvoiceNumber,
                        CustomerName = !string.IsNullOrEmpty(o.Nickname) ? o.Nickname : "Guest",
                        OrderTime = o.OrderDate.ToString("h:mm tt"),
                        ItemCount = o.OrderItems?.Count ?? 0,
                        TotalPrice = o.TotalPrice,
                        Status = o.Status
                    })
                    .ToList();

                var viewModel = new DashboardViewModel
                {
                    RevenueToday = revenueToday,
                    TotalRevenue = totalRevenue,
                    RevenueTodayChange = Math.Round(revenueTodayChange, 1),
                    TotalOrdersToday = ordersToday,
                    ActiveOrders = activeOrders,
                    CompletedOrdersToday = completedOrdersToday,
                    CancelledOrdersToday = cancelledOrdersToday,
                    OrdersTodayChange = Math.Round(ordersTodayChange, 1),
                    RecentOrders = recentOrders
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");

                // Return empty dashboard on error
                return View(new DashboardViewModel());
            }
        }
    }
}