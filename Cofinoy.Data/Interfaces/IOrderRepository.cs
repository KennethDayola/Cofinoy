using Cofinoy.Data.Models;
using System.Linq;

namespace Cofinoy.Data.Interfaces
{
    public interface IOrderRepository
    {
        IQueryable<Order> GetOrders();
        IQueryable<Order> GetOrdersByUserId(string userId);
        Order GetOrderById(int id);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
        bool OrderExists(int id);
        int GetOrdersCount();
    }
}