using Cofinoy.Data.Models;
using System;
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
        IQueryable<Order> GetOrdersByDate(DateTime date);
        IQueryable<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate);
        IQueryable<Order> GetOrdersByStatus(string status);
        IQueryable<Order> GetOrdersWithFilter(string status, string searchTerm);
        decimal GetTotalRevenue();
        decimal GetRevenueByDate(DateTime date);
        int GetOrdersCountByDate(DateTime date);
    }
}