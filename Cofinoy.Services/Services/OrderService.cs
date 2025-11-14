using Cofinoy.Data.Interfaces;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cofinoy.Data.Models;

namespace Cofinoy.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ICartRepository cartRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<List<OrderServiceModel>> GetAllOrdersAsync(string status = null, string searchTerm = null)
        {
            try
            {
                var query = _orderRepository.GetOrders();

                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    var normalized = status == "Brewing" ? "Pending" : status;
                    query = query.Where(o => o.Status == normalized);
                }

                var orders = query.ToList();
                var users = _userRepository.GetUsers().ToList();

                var list = orders
                    .Select(o =>
                    {
                        var user = users.FirstOrDefault(u => o.UserId == u.Id.ToString());
                        var customerName = user != null
                            ? $"{user.FirstName} {user.LastName}"
                            : (!string.IsNullOrEmpty(o.Nickname) ? o.Nickname : "Guest");

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            bool matches =
                                (o.InvoiceNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (customerName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (o.Nickname?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false);

                            if (!matches) return null;
                        }

                        return new OrderServiceModel
                        {
                            Id = o.Id,
                            InvoiceNumber = o.InvoiceNumber,
                            UserId = o.UserId,
                            Nickname = o.Nickname,
                            OrderDate = o.OrderDate,
                            PaymentMethod = o.PaymentMethod,
                            Status = o.Status,
                            TotalPrice = o.TotalPrice,
                            OrderItems = o.OrderItems.Select(oi => new OrderItemServiceModel
                            {
                                Id = oi.Id,
                                ProductName = oi.ProductName,
                                Quantity = oi.Quantity,
                                UnitPrice = oi.UnitPrice,
                                TotalPrice = oi.TotalPrice
                            }).ToList()
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                return await Task.FromResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders");
                throw;
            }
        }

        public async Task<List<OrderServiceModel>> GetOrdersByUserIdAsync(string userId)
        {
            try
            {
                var orders = _orderRepository.GetOrdersByUserId(userId)
                    .Select(o => new OrderServiceModel
                    {
                        Id = o.Id,
                        Status = o.Status,
                        OrderDate = o.OrderDate,
                        TotalPrice = o.TotalPrice,
                        InvoiceNumber = o.InvoiceNumber
                    })
                    .ToList();

                return await Task.FromResult(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDetailsServiceModel> GetOrderDetailsAsync(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                return null;

            var user = _userRepository.GetUsers()
                .FirstOrDefault(u => u.Id.ToString() == order.UserId || u.Email == order.UserId);

            var details = new OrderDetailsServiceModel
            {
                Id = order.Id,
                InvoiceNumber = order.InvoiceNumber,
                UserId = order.UserId,
                Nickname = order.Nickname,
                OrderDate = order.OrderDate,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                AdditionalRequest = order.AdditionalRequest,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new OrderItemServiceModel
                {
                    Id = oi.Id,
                    ProductName = oi.ProductName,
                    Description = oi.Description,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    // Legacy fields
                    Size = oi.Size,
                    MilkType = oi.MilkType,
                    Temperature = oi.Temperature,
                    ExtraShots = oi.ExtraShots,
                    SweetnessLevel = oi.SweetnessLevel,
                    
                    Customizations = oi.Customizations?
                        .OrderBy(c => c.DisplayOrder ?? int.MaxValue)
                        .ThenBy(c => c.Name)
                        .Select(c => new CustomizationData
                        {
                            Name = c.Name,
                            Value = c.Value,
                            Type = c.Type,
                            DisplayOrder = c.DisplayOrder,
                            Price = c.Price
                        }).ToList() ?? new List<CustomizationData>()
                }).ToList(),
                CustomerInfo = user != null
                    ? new CustomerInfoServiceModel
                    {
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Country = user.Country,
                        City = user.City
                    }
                    : new CustomerInfoServiceModel
                    {
                        Email = "",
                        PhoneNumber = "",
                        Country = "",
                        City = ""
                    },
                CustomerName = user != null && (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                    ? $"{user.FirstName} {user.LastName}".Trim()
                    : order.Nickname
            };

            return await Task.FromResult(details);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);
                if (order == null)
                    throw new InvalidDataException("Order not found");

                var validStatuses = new[] { "Pending", "Confirmed", "Brewing", "Ready", "Serving", "Served", "Cancelled" };
                if (!validStatuses.Contains(newStatus))
                    throw new InvalidDataException("Invalid status");

                if (order.Status == "Served")
                    throw new InvalidDataException("Cannot update a served order");

                if (order.Status == "Cancelled")
                    throw new InvalidDataException("Cannot update a cancelled order");

                _logger.LogInformation("Updating order {OrderId} from {OldStatus} to {NewStatus}",
                    orderId, order.Status, newStatus);

                order.Status = newStatus;
                _orderRepository.UpdateOrder(order);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);
                if (order == null)
                    throw new InvalidDataException("Order not found");

                if (order.Status == "Completed" || order.Status == "Cancelled")
                    throw new InvalidDataException($"Cannot cancel a {order.Status} order");

                order.Status = "Cancelled";
                _orderRepository.UpdateOrder(order);

                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderDetailsServiceModel> CreateOrderAsync(string userId, string nickname, string additionalRequest, string paymentMethod, List<CartItemServiceModel> cartItems)
        {
            try
            {
                if (cartItems == null || !cartItems.Any())
                {
                    throw new InvalidOperationException("Cannot create order with empty cart");
                }

                _logger.LogInformation("Creating order for user {UserId}", userId);

                // Create order entity
                var order = new Order
                {
                    UserId = userId,
                    InvoiceNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    OrderDate = DateTime.Now,
                    Nickname = nickname,
                    AdditionalRequest = additionalRequest ?? "",
                    PaymentMethod = paymentMethod,
                    TotalPrice = cartItems.Sum(i => i.TotalPrice),
                    Status = "Pending",
                    OrderItems = cartItems.Select(item => 
                    {
                        var orderItem = new OrderItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            Description = item.Description ?? "",
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalPrice,
                            // Legacy fields for backward compatibility
                            Size = item.Size ?? "",
                            MilkType = item.MilkType ?? "",
                            Temperature = item.Temperature ?? "",
                            ExtraShots = item.ExtraShots,
                            SweetnessLevel = item.SweetnessLevel ?? "",
                            Customizations = new List<OrderItemCustomization>()
                        };

                        // Map customizations from cart item to order item
                        if (item.Customizations != null && item.Customizations.Any())
                        {
                            _logger.LogInformation("Mapping {Count} customizations for product {ProductName}", 
                                item.Customizations.Count, item.Name);

                            foreach (var customization in item.Customizations)
                            {
                                orderItem.Customizations.Add(new OrderItemCustomization
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Name = customization.Name,
                                    Value = customization.Value,
                                    Type = customization.Type,
                                    DisplayOrder = customization.DisplayOrder,
                                    Price = customization.Price
                                });
                            }
                        }

                        return orderItem;
                    }).ToList()
                };

                _logger.LogInformation("Order created with invoice: {InvoiceNumber}, Items: {ItemCount}", 
                    order.InvoiceNumber, order.OrderItems.Count);

                // Save order using repository
                _orderRepository.AddOrder(order);

                _logger.LogInformation("Order saved to database with ID: {OrderId}", order.Id);

                // Clear cart
                await _cartRepository.ClearCartAsync(userId);

                _logger.LogInformation("Cart cleared for user {UserId}", userId);

                // Return order details
                var orderDetails = await GetOrderDetailsAsync(order.Id);
                
                return orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public bool OrderExists(int id)
        {
            return _orderRepository.OrderExists(id);
        }
    }
}