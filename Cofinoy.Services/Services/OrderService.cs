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

            // Resolve user by Id or Email stored in Order.UserId
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
                    Size = oi.Size,
                    MilkType = oi.MilkType,
                    Temperature = oi.Temperature,
                    ExtraShots = oi.ExtraShots,
                    SweetnessLevel = oi.SweetnessLevel
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

        public bool OrderExists(int id)
        {
            return _orderRepository.OrderExists(id);
        }
    }
}