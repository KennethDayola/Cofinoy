using Cofinoy.Data.Interfaces;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.Services.Services
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderHistoryRepository _orderHistoryRepository;
        private readonly ILogger<OrderHistoryService> _logger;

        public OrderHistoryService(
            IOrderHistoryRepository orderHistoryRepository,
            ILogger<OrderHistoryService> logger)
        {
            _orderHistoryRepository = orderHistoryRepository;
            _logger = logger;
        }

        public async Task<List<OrderServiceModel>> GetOrderHistoryByUserIdAsync(string userId)
        {
            try
            {
                var orders = _orderHistoryRepository.GetOrderHistoryByUserId(userId);

                var orderModels = orders.Select(o => new OrderServiceModel
                {
                    Id = o.Id,
                    InvoiceNumber = o.InvoiceNumber,
                    UserId = o.UserId,
                    Nickname = o.Nickname,
                    OrderDate = o.OrderDate,
                    PaymentMethod = o.PaymentMethod,
                    Status = o.Status,
                    AdditionalRequest = o.AdditionalRequest,
                    TotalPrice = o.TotalPrice,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemServiceModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Description = oi.Description,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Size = oi.Size,
                        Temperature = oi.Temperature,
                        MilkType = oi.MilkType,
                        ExtraShots = oi.ExtraShots,
                        SweetnessLevel = oi.SweetnessLevel,
                        ImageUrl = oi.Product?.ImageUrl ?? oi.Product?.ImagePath
                    }).ToList()
                }).ToList();

                return await Task.FromResult(orderModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order history for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDetailsServiceModel> GetOrderDetailsByIdAsync(int orderId)
        {
            try
            {
                var order = _orderHistoryRepository.GetOrderDetailsById(orderId);
                
                if (order == null)
                {
                    return null;
                }

                var orderDetails = new OrderDetailsServiceModel
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
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Description = oi.Description,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Size = oi.Size,
                        Temperature = oi.Temperature,
                        MilkType = oi.MilkType,
                        ExtraShots = oi.ExtraShots,
                        SweetnessLevel = oi.SweetnessLevel,
                        ImageUrl = oi.Product?.ImageUrl ?? oi.Product?.ImagePath
                    }).ToList()
                };

                return await Task.FromResult(orderDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<List<OrderStatusServiceModel>> GetOrderStatusesByUserIdAsync(string userId)
        {
            try
            {
                var orders = _orderHistoryRepository.GetOrderStatusesByUserId(userId);

                var orderStatuses = orders.Select(o => new OrderStatusServiceModel
                {
                    Id = o.Id,
                    Status = o.Status
                }).ToList();

                return await Task.FromResult(orderStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statuses for user: {UserId}", userId);
                throw;
            }
        }
    }
}
