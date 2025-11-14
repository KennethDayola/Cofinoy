using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cofinoy.Services.Interfaces
{
    public interface IOrderHistoryService
    {
        Task<List<OrderServiceModel>> GetOrderHistoryByUserIdAsync(string userId);
        Task<OrderDetailsServiceModel> GetOrderDetailsByIdAsync(int orderId);
        Task<List<OrderStatusServiceModel>> GetOrderStatusesByUserIdAsync(string userId);

        Task<bool> CancelOrderAsync(int orderId, string userId); 

    }
}
