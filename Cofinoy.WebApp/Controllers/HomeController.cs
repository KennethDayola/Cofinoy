using AutoMapper;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Cofinoy.WebApp.Controllers
{
    /// <summary>
    /// Home Controller - Refactored with proper service layer
    /// Uses YOUR existing DashboardViewModel
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IDashboardService dashboardService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _dashboardService = dashboardService;
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
        /// Gets existing DashboardViewModel from service
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            try
            {
                var viewModel = _dashboardService.GetDashboardData();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View(new DashBoardViewModel());
            }
        }

        /// <summary>
        /// API endpoint to get real-time revenue statistics
        /// Can be used for AJAX refresh without full page reload
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult GetRevenueStats()
        {
            try
            {
                var stats = _dashboardService.GetRevenueStats();
                return Json(new
                {
                    success = true,
                    revenueToday = stats.today,
                    revenueYesterday = stats.yesterday,
                    totalRevenue = stats.total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching revenue stats");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// API endpoint to get real-time order statistics
        /// Can be used for AJAX refresh without full page reload
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult GetOrderStats()
        {
            try
            {
                var stats = _dashboardService.GetOrderStats();
                return Json(new
                {
                    success = true,
                    totalOrders = stats.total,
                    activeOrders = stats.active,
                    completedOrders = stats.completed,
                    cancelledOrders = stats.cancelled
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order stats");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}