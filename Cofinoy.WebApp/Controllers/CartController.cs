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
using System.Linq;

namespace Cofinoy.WebApp.Controllers
{
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        public CartController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICartService cartService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
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
                Console.WriteLine("=== CART CONTROLLER ADD TO CART START ===");
                Console.WriteLine($"Received item: {item.Name}, Price: {item.UnitPrice}, Quantity: {item.Quantity}");

                var userId = GetCurrentUserId();
                Console.WriteLine($"User ID: {userId}");

                await _cartService.AddToCartAsync(userId, item);

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var cartCount = cartItems.Sum(i => i.Quantity);

                Console.WriteLine($"Success! Cart count: {cartCount}, Total items: {cartItems.Count}");
                Console.WriteLine("=== CART CONTROLLER ADD TO CART COMPLETED ===");

                return Json(new { success = true, cartCount = cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== CART CONTROLLER ADD TO CART FAILED ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                _logger.LogError(ex, "Error adding item to cart");
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
                    total = subtotal - 60, 
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
               

                return Json(new { success = true, message = "Order placed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            var userId = User?.Identity?.IsAuthenticated == true ?
                   User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value :
                   HttpContext.Session.Id;

            Console.WriteLine($"GetCurrentUserId returned: {userId}");
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

    public class CheckoutViewModel
    {
        public string Nickname { get; set; }
        public string AdditionalRequest { get; set; }
        public string OrderType { get; set; }
        public string CouponCode { get; set; }
    }
}