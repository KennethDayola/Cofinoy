using AutoMapper;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Cofinoy.Services.Services;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CartController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICartService cartService,
            IOrderService orderService,
             IUserService userService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItems = await _cartService.GetCartItemsAsync(userId);

                // Get user's nickname if authenticated
                string nickname = null;
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var email = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        var user = _userService.GetUserByEmail(email);
                        nickname = user?.Nickname;
                    }
                }

                ViewBag.UserNickname = nickname;

                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart page");
                ViewBag.UserNickname = null;
                return View(new List<CartItemServiceModel>());
            }
        }

            [HttpPost]
        public async Task<JsonResult> AddToCart([FromBody] CartItemServiceModel item)
        {
            try
            {
                _logger.LogInformation("=== CART CONTROLLER ADD TO CART DEBUG ===");
                _logger.LogInformation("Product: {ProductName}", item.Name);
                _logger.LogInformation("UnitPrice received: {UnitPrice}", item.UnitPrice);
                _logger.LogInformation("Quantity: {Quantity}", item.Quantity);
                _logger.LogInformation("Expected Total: {Total}", item.UnitPrice * item.Quantity);

                if (item.Customizations != null && item.Customizations.Any())
                {
                    _logger.LogInformation("Customizations count: {Count}", item.Customizations.Count);
                    foreach (var custom in item.Customizations)
                    {
                        _logger.LogInformation("  - {Name}: {Value} ({Type})", custom.Name, custom.Value, custom.Type);
                    }
                }
                else
                {
                    _logger.LogInformation("No customizations in request!");
                }

                var userId = GetCurrentUserId();
                await _cartService.AddToCartAsync(userId, item);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var cartCount = cartItems.Sum(i => i.Quantity);

                return Json(new { success = true, cartCount = cartCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateQuantity([FromBody] UpdateQuantityModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _cartService.UpdateCartItemQuantityAsync(userId, model.CartItemId, model.Quantity);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var updatedItem = cartItems.FirstOrDefault(i => i.CartItemId == model.CartItemId);
                var subtotal = cartItems.Sum(i => i.TotalPrice);
                var cartCount = cartItems.Sum(i => i.Quantity);

                return Json(new
                {
                    success = true,
                    itemTotal = updatedItem?.TotalPrice ?? 0,
                    subtotal = subtotal,
                    total = subtotal,
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
                await _cartService.RemoveFromCartAsync(userId, model.CartItemId);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var subtotal = cartItems.Sum(i => i.TotalPrice);
                var cartCount = cartItems.Sum(i => i.Quantity);

                return Json(new
                {
                    success = true,
                    subtotal = subtotal,
                    total = subtotal,
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
                        UnitPrice = oi.UnitPrice,
                        // Map customizations
                        Customizations = oi.Customizations ?? new List<CustomizationData>()
                    }).ToList()
                };

                _logger.LogInformation("=== CHECKOUT COMPLETE ===");

                return View("~/Views/Cart/Checkout.cshtml", invoiceModel);
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
        public string CartItemId { get; set; } // Changed from ProductId
        public int Quantity { get; set; }
    }

    public class RemoveFromCartModel
    {
        public string CartItemId { get; set; } // Changed from ProductId
    }
}