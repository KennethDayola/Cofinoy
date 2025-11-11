using AutoMapper;
using Cofinoy.Services.Interfaces;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    /// <summary>
    /// Order Controller
    /// </summary>
    public class OrderController : ControllerBase<OrderController>
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IOrderService orderService
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult OrderManagement() => View();

        public async Task<IActionResult> ViewOrder(int orderId)
        {
            var order = await _orderService.GetOrderDetailsAsync(orderId);

            if (order == null)
                return NotFound();

            return View("OrderDetails", order);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetAllOrders(string status = null, string searchTerm = null)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(status, searchTerm);

                var orderViewModels = orders.Select(o => new
                {
                    o.Id,
                    o.InvoiceNumber,
                    CustomerName = !string.IsNullOrEmpty(o.Nickname) ? o.Nickname : "Guest",
                    o.Nickname,
                    OrderDate = o.OrderDate.ToString("MM/dd/yy – h:mm tt"),
                    ItemCount = o.OrderItems.Count,
                    o.TotalPrice,
                    o.Status,
                    o.PaymentMethod
                }).ToList();

                return Json(new { success = true, data = orderViewModels, count = orderViewModels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderStatuses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);

                var orderStatuses = orders.Select(o => new
                {
                    Id = o.Id,
                    Status = o.Status
                }).ToList();

                return Json(new { success = true, data = orderStatuses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order statuses for user {UserId}", userId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetOrderDetails(int orderId)
        {
            try
            {
                var orderDetails = await _orderService.GetOrderDetailsAsync(orderId);

                if (orderDetails == null)
                    return Json(new { success = false, error = "Order not found" });

                var result = new
                {
                    orderDetails.Id,
                    orderDetails.InvoiceNumber,
                    orderDetails.OrderDate,
                    orderDetails.CustomerName,
                    orderDetails.Nickname,
                    orderDetails.PaymentMethod,
                    orderDetails.Status,
                    orderDetails.AdditionalRequest,
                    orderDetails.TotalPrice,
                    orderDetails.CustomerInfo,
                    OrderItems = orderDetails.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductName,
                        oi.Description,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice,
                        // Legacy fields
                        oi.Size,
                        oi.MilkType,
                        oi.Temperature,
                        oi.ExtraShots,
                        oi.SweetnessLevel,
                        // New: Include customizations
                        Customizations = oi.Customizations.Select(c => new
                        {
                            c.Name,
                            c.Value,
                            c.Type,
                            c.DisplayOrder,
                            c.Price
                        }).ToList()
                    }).ToList()
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order details");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> UpdateOrderStatus([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("orderId", out var idProp) ||
                    !body.TryGetProperty("newStatus", out var statusProp))
                    return Json(new { success = false, error = "Invalid request data" });

                int orderId = idProp.GetInt32();
                string newStatus = statusProp.GetString();

                var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

                if (!result)
                    return Json(new { success = false, error = "Failed to update order status" });

                return Json(new
                {
                    success = true,
                    message = $"Order status updated to {newStatus}",
                    orderId = orderId,
                    newStatus = newStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> CancelOrder([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("orderId", out var idProp))
                    return Json(new { success = false, error = "Invalid request data" });

                int orderId = idProp.GetInt32();
                var result = await _orderService.CancelOrderAsync(orderId);

                if (!result)
                    return Json(new { success = false, error = "Failed to cancel order" });

                return Json(new { success = true, message = "Order cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}