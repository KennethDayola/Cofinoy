using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cofinoy.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderServiceModel>> GetAllOrdersAsync(string status = null, string searchTerm = null);
        Task<List<OrderServiceModel>> GetOrdersByUserIdAsync(string userId);
        Task<OrderDetailsServiceModel> GetOrderDetailsAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<bool> CancelOrderAsync(int orderId);
        bool OrderExists(int id);
    }
}