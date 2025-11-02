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
using Cofinoy.Data;

namespace Cofinoy.WebApp.Controllers
{
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;
        private readonly CofinoyDbContext _context;

        public CartController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICartService cartService,
            CofinoyDbContext context) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
            _mapper = mapper;
            _context = context;
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

                Console.WriteLine("=== CHECKOUT START ===");
                Console.WriteLine($"User ID: {userId}");

                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (cartItems == null || !cartItems.Any())
                {
                    Console.WriteLine("Cart is empty, redirecting...");
                    return RedirectToAction("Index", "Cart");
                }

                Console.WriteLine($"Cart items count: {cartItems.Count}");

               
                var order = new Cofinoy.Data.Models.Order
                {
                    UserId = userId,
                    InvoiceNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    OrderDate = DateTime.UtcNow,
                    Nickname = model.Nickname,
                    AdditionalRequest = model.AdditionalRequest ?? "",
                    PaymentMethod = model.PaymentMethod,
                    TotalPrice = cartItems.Sum(i => i.TotalPrice),
                    Status = "Pending"
                };

                Console.WriteLine($"Order created with invoice: {order.InvoiceNumber}");

               
                order.OrderItems = cartItems.Select(item => new Cofinoy.Data.Models.OrderItem
                {
                    ProductId = item.ProductId,              
                    ProductName = item.Name,                 
                    Description = item.Description ?? "",
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                    Size = item.Size ?? "",
                    MilkType = item.MilkType ?? "",
                    Temperature = item.Temperature ?? "",
                    ExtraShots = item.ExtraShots,
                    SweetnessLevel = item.SweetnessLevel ?? ""
                }).ToList();

                Console.WriteLine($"Order items created: {order.OrderItems.Count}");

                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                Console.WriteLine("Order saved to database");

                
                await _cartService.ClearCartAsync(userId);

                Console.WriteLine("Cart cleared");

               
                var invoiceModel = new CheckoutViewModel
                {
                    InvoiceNumber = order.InvoiceNumber,
                    OrderDate = order.OrderDate,
                    Nickname = order.Nickname,
                    AdditionalRequest = order.AdditionalRequest,
                    PaymentMethod = order.PaymentMethod,
                    TotalPrice = order.TotalPrice,
                    CartItems = order.OrderItems.Select(oi => new InvoiceItem
                    {
                        Name = oi.ProductName,
                        Description = oi.Description,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                };

                Console.WriteLine("=== CHECKOUT COMPLETE ===");

               
                return View("~/Views/Checkout/Checkout.cshtml", invoiceModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== CHECKOUT ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");

                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                var innerTrace = ex.InnerException?.StackTrace ?? "";

                Console.WriteLine($"Inner Exception: {innerMessage}");
                Console.WriteLine($"Inner Stack Trace: {innerTrace}");

                _logger.LogError(ex, "Error during checkout. Inner: {Inner}", innerMessage);

                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerError = innerMessage
                });
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
}