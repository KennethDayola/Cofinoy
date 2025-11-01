using Cofinoy.Data;
using Cofinoy.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cofinoy.Controllers
{
    public class OrderHistoryController : Controller
    {
        private readonly CofinoyDbContext _context;

        public OrderHistoryController(CofinoyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
    .Where(o => o.UserId == userId)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .OrderByDescending(o => o.OrderDate)
    .ToListAsync();


            return View("~/Views/Order/OrderHistory.cshtml", orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var result = new
                {
                    success = true,
                    order = new
                    {
                        invoiceNumber = order.InvoiceNumber,
                        nickname = order.Nickname,
                        orderDate = order.OrderDate.ToString("MMMM d, yyyy - h:mm tt"),
                        additionalRequest = order.AdditionalRequest,
                        totalPrice = order.TotalPrice,
                        paymentMethod = order.PaymentMethod,
                        status = order.Status,
                        items = order.OrderItems.Select(oi => new
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
                            productImageUrl = oi.Product?.ImageUrl,
                            imageUrl = oi.Product?.ImageUrl 
                        }).ToList()
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderStatuses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new {
                        id = o.Id,
                        status = o.Status
                    })
                    .ToListAsync();

                return Json(orders);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllOrderStatuses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Select(o => new
                    {
                        id = o.Id,
                        status = o.Status
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    orders = orders
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}