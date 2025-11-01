using AutoMapper;
using Cofinoy.Data;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Controllers
{
    public class MenuController : ControllerBase<MenuController>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomizationService _customizationService;
        private readonly IProductService _productService;
        private readonly CofinoyDbContext _context;
        private readonly IMapper _mapper;

        public MenuController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICategoryService categoryService,
            ICustomizationService customizationService,
            IProductService productService,
            CofinoyDbContext context)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _categoryService = categoryService;
            _customizationService = customizationService;
            _productService = productService;
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult DrinkManagement() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult CategoriesManagement() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult CustomizationManagement() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult OrderManagement() => View("~/Views/Menu/OrderManagement.cshtml");
        public IActionResult ViewOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

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
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && status != "All")
                    query = query.Where(o => o.Status == status);

                var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
                var users = await _context.Users.ToListAsync();

                var orderViewModels = orders.Select(o =>
                {
                    var user = users.FirstOrDefault(u => o.UserId == u.Id.ToString());
                    var customerName = user != null
                        ? $"{user.FirstName} {user.LastName}"
                        : (!string.IsNullOrEmpty(o.Nickname) ? o.Nickname : "Guest");

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        bool matches = (o.InvoiceNumber?.Contains(searchTerm) ?? false)
                                    || (customerName?.Contains(searchTerm) ?? false)
                                    || (o.Nickname?.Contains(searchTerm) ?? false);
                        if (!matches) return null;
                    }

                    return new
                    {
                        o.Id,
                        o.InvoiceNumber,
                        CustomerName = customerName,
                        o.Nickname,
                        OrderDate = o.OrderDate.ToString("MM/dd/yy – h:mm tt"),
                        ItemCount = o.OrderItems.Count,
                        o.TotalPrice,
                        o.Status,
                        o.PaymentMethod
                    };
                })
                .Where(x => x != null)
                .ToList();

                return Json(new { success = true, data = orderViewModels, count = orderViewModels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Add this to your Order controller (MenuController or OrderController)
        [HttpGet]
        public async Task<IActionResult> GetOrderStatuses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new {
                    Id = o.Id,
                    Status = o.Status
                })
                .ToListAsync();

            return Json(new { success = true, data = orders });
        }

        // ✅ Get order details (for modal view)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return Json(new { success = false, error = "Order not found" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == order.UserId);

                var orderDetails = new
                {
                    order.Id,
                    order.InvoiceNumber,
                    order.OrderDate,
                    CustomerName = user != null ? $"{user.FirstName} {user.LastName}" : order.Nickname,
                    order.Nickname,
                    order.PaymentMethod,
                    order.Status,
                    order.AdditionalRequest,
                    order.TotalPrice,
                    CustomerInfo = user != null ? new
                    {
                        user.Email,
                        user.PhoneNumber,
                        user.Country,
                        user.City
                    } : new
                    {
                        Email = "",
                        PhoneNumber = "",
                        Country = "",
                        City = ""
                    },
                    OrderItems = order.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductName,
                        oi.Description,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice,
                        oi.Size,
                        oi.MilkType,
                        oi.Temperature,
                        oi.ExtraShots,
                        oi.SweetnessLevel
                    }).ToList()
                };

                return Json(new { success = true, data = orderDetails });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order details");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ✅ Update order status (dropdown)
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

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return Json(new { success = false, error = "Order not found" });

                var validStatuses = new[] { "Pending", "Confirmed", "Brewing", "Ready", "Completed", "Cancelled" };
                if (!validStatuses.Contains(newStatus))
                    return Json(new { success = false, error = "Invalid status" });

                order.Status = newStatus;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Order status updated to {newStatus}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ✅ Cancel order (cancel button)

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> CancelOrder([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("orderId", out var idProp))
                    return Json(new { success = false, error = "Invalid request data" });

                int orderId = idProp.GetInt32();
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                    return Json(new { success = false, error = "Order not found" });

                if (order.Status == "Completed" || order.Status == "Cancelled")
                    return Json(new { success = false, error = $"Cannot cancel a {order.Status} order" });

                order.Status = "Cancelled";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Order cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult GetAllCategories()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddCategory([FromBody] CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new CategoryServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    Status = model.Status
                };

                _categoryService.AddCategory(serviceModel);
                return Json(new { success = true, message = "Category added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateCategory(string id, [FromBody] CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new CategoryServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    Status = model.Status
                };

                _categoryService.UpdateCategory(id, serviceModel);
                return Json(new { success = true, message = "Category updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteCategory(string id)
        {
            try
            {
                _categoryService.DeleteCategory(id);
                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public JsonResult GetAllCustomizations()
        {
            try
            {
                var customizations = _customizationService.GetAllCustomizations();
                return Json(new { success = true, data = customizations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customizations");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetCustomization(string id)
        {
            try
            {
                var customization = _customizationService.GetCustomizationById(id);
                if (customization == null)
                {
                    return Json(new { success = false, error = "Customization not found" });
                }
                return Json(new { success = true, data = customization });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddCustomization([FromBody] CustomizationServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _customizationService.AddCustomization(model);
                return Json(new { success = true, message = "Customization added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateCustomization(string id, [FromBody] CustomizationServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _customizationService.UpdateCustomization(id, model);
                return Json(new { success = true, message = "Customization updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteCustomization(string id)
        {
            try
            {
                _customizationService.DeleteCustomization(id);
                return Json(new { success = true, message = "Customization deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public JsonResult GetProductsByCategory(string categoryName)
        {
            try
            {
                _logger.LogInformation("Fetching products for category: {CategoryName}", categoryName);
                var products = _productService.GetProductsByCategory(categoryName);
                _logger.LogInformation("Found {Count} products for category: {CategoryName}", products.Count, categoryName);
                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {CategoryName}", categoryName);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public JsonResult GetAllProducts()
        {
            try
            {
                var products = _productService.GetAllProducts();
                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return Json(new { success = false, error = ex.Message });
            }
        }


        [HttpGet]
        public JsonResult GetProduct(string id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    return Json(new { success = false, error = "Product not found" });
                }
                return Json(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddProduct([FromBody] ProductServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _productService.AddProduct(model);
                return Json(new { success = true, message = "Product added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateProduct(string id, [FromBody] ProductServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _productService.UpdateProduct(id, model);
                return Json(new { success = true, message = "Product updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteProduct(string id)
        {
            try
            {
                _productService.DeleteProduct(id);
                return Json(new { success = true, message = "Product deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        

    }
}