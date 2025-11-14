using Cofinoy.Data.Models;
using System.Collections.Generic;

namespace Cofinoy.Data.Interfaces
{
    public interface IOrderHistoryRepository
    {
        List<Order> GetOrderHistoryByUserId(string userId);
        Order GetOrderDetailsById(int orderId);
        List<Order> GetOrderStatusesByUserId(string userId);

  

    }
}
