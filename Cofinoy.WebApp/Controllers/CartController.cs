using AutoMapper;
using Cofinoy.Data.Models;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public CartController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICartService cartService,
            IOrderService orderService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
            _orderService = orderService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItems = await _cartService.GetCartItemsAsync(userId);

                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart page");
                return View(new List<CartItemServiceModel>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddToCart([FromBody] CartItemServiceModel item)
        {
            try
            {
                Console.WriteLine("=== CART CONTROLLER ADD TO CART DEBUG ===");
                Console.WriteLine($"Product: {item.Name}");
                Console.WriteLine($"UnitPrice received: {item.UnitPrice}");
                Console.WriteLine($"Quantity: {item.Quantity}");
                Console.WriteLine($"Expected Total: {item.UnitPrice * item.Quantity}");

                if (item.Customizations != null && item.Customizations.Any())
                {
                    Console.WriteLine($"Customizations count: {item.Customizations.Count}");
                    foreach (var custom in item.Customizations)
                    {
                        Console.WriteLine($"  - {custom.Name}: {custom.Value} ({custom.Type})");
                    }
                }
                else
                {
                    Console.WriteLine("No customizations in request!");
                }

                var userId = GetCurrentUserId();
                await _cartService.AddToCartAsync(userId, item);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var cartCount = cartItems.Sum(i => i.Quantity);

                var storedItem = cartItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (storedItem != null)
                {
                    Console.WriteLine($"Stored in cart - UnitPrice: {storedItem.UnitPrice}, Total: {storedItem.UnitPrice * storedItem.Quantity}");
                }

                return Json(new { success = true, cartCount = cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateQuantity([FromBody] UpdateQuantityModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _cartService.UpdateCartItemQuantityAsync(userId, model.ProductId, model.Quantity);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var updatedItem = cartItems.FirstOrDefault(i => i.ProductId == model.ProductId);
                var subtotal = cartItems.Sum(i => i.TotalPrice);
                var cartCount = cartItems.Sum(i => i.Quantity);

                return Json(new
                {
                    success = true,
                    itemTotal = updatedItem?.TotalPrice ?? 0,
                    subtotal = subtotal,
                    total = subtotal - 60,
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveFromCart([FromBody] RemoveFromCartModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _cartService.RemoveFromCartAsync(userId, model.ProductId);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var subtotal = cartItems.Sum(i => i.TotalPrice);
                var cartCount = cartItems.Sum(i => i.Quantity);

                return Json(new
                {
                    success = true,
                    subtotal = subtotal,
                    total = subtotal - 0,
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();

                _logger.LogInformation("=== CHECKOUT START ===");
                _logger.LogInformation("User ID: {UserId}", userId);

                // Get cart items
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (cartItems == null || !cartItems.Any())
                {
                    _logger.LogWarning("Cart is empty for user {UserId}, redirecting...", userId);
                    return RedirectToAction("Index", "Cart");
                }

                _logger.LogInformation("Cart items count: {Count}", cartItems.Count);

                // Create order through OrderService
                var orderDetails = await _orderService.CreateOrderAsync(
                    userId,
                    model.Nickname,
                    model.AdditionalRequest,
                    model.PaymentMethod,
                    cartItems
                );

                _logger.LogInformation("Order created successfully with ID: {OrderId}", orderDetails.Id);

                // Prepare invoice model for view
                var invoiceModel = new CheckoutViewModel
                {
                    InvoiceNumber = orderDetails.InvoiceNumber,
                    OrderDate = orderDetails.OrderDate,
                    Nickname = orderDetails.Nickname,
                    AdditionalRequest = orderDetails.AdditionalRequest,
                    PaymentMethod = orderDetails.PaymentMethod,
                    TotalPrice = orderDetails.TotalPrice,
                    CartItems = orderDetails.OrderItems.Select(oi => new InvoiceItem
                    {
                        Name = oi.ProductName,
                        Description = oi.Description,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                };

                _logger.LogInformation("=== CHECKOUT COMPLETE ===");

                return View("~/Views/Checkout/Checkout.cshtml", invoiceModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");

                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message ?? "No inner exception"
                });
            }
        }

        private string GetCurrentUserId()
        {
            var userId = User?.Identity?.IsAuthenticated == true ?
                   User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value :
                   HttpContext.Session.Id;

            return userId;
        }
    }

    public class UpdateQuantityModel
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveFromCartModel
    {
        public string ProductId { get; set; }
    }
}