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
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    [Authorize]
    public class OrderHistoryController : ControllerBase<OrderHistoryController>
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IMapper _mapper;

        public OrderHistoryController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IOrderHistoryService orderHistoryService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderHistoryService = orderHistoryService;
            _mapper = mapper;
        }

        public async Task<IActionResult> OrderHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var orders = await _orderHistoryService.GetOrderHistoryByUserIdAsync(userId);

                return View("~/Views/Order/OrderHistory.cshtml", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order history");
                return View("~/Views/Order/OrderHistory.cshtml", new System.Collections.Generic.List<Services.ServiceModels.OrderServiceModel>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetOrderDetails(int id)
        {
            try
            {
                var orderDetails = await _orderHistoryService.GetOrderDetailsByIdAsync(id);

                if (orderDetails == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var result = new
                {
                    success = true,
                    order = new
                    {
                        invoiceNumber = orderDetails.InvoiceNumber,
                        nickname = orderDetails.Nickname,
                        orderDate = orderDetails.OrderDate.ToString("MMMM d, yyyy - h:mm tt"),
                        additionalRequest = orderDetails.AdditionalRequest,
                        totalPrice = orderDetails.TotalPrice,
                        paymentMethod = orderDetails.PaymentMethod,
                        status = orderDetails.Status,
                        items = orderDetails.OrderItems.Select(oi => new
                        {
                            productName = oi.ProductName,
                            unitPrice = oi.UnitPrice,
                            quantity = oi.Quantity,
                            totalPrice = oi.TotalPrice,
                            size = oi.Size,
                            temperature = oi.Temperature,
                            milkType = oi.MilkType,
                            extraShots = oi.ExtraShots,
                            sweetnessLevel = oi.SweetnessLevel,
                            productImageUrl = oi.ImageUrl,
                            imageUrl = oi.ImageUrl,
                            // Include customizations
                            customizations = oi.Customizations.Select(c => new
                            {
                                name = c.Name,
                                value = c.Value,
                                type = c.Type,
                                displayOrder = c.DisplayOrder,
                                price = c.Price
                            }).ToList()
                        }).ToList()
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order: {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetOrderStatuses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var orderStatuses = await _orderHistoryService.GetOrderStatusesByUserIdAsync(userId);

                return Json(new { success = true, data = orderStatuses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statuses");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAllOrderStatuses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var orderStatuses = await _orderHistoryService.GetOrderStatusesByUserIdAsync(userId);

                return Json(new
                {
                    success = true,
                    orders = orderStatuses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all order statuses");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}