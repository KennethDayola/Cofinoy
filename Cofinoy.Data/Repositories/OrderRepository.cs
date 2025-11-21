using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Cofinoy.Data.Repositories
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Order> GetOrders()
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .OrderByDescending(o => o.OrderDate);
        }

        public IQueryable<Order> GetOrdersByUserId(string userId)   
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);
        }

        public Order GetOrderById(int id)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .FirstOrDefault(o => o.Id == id);
        }

        public void AddOrder(Order order)
        {
            order.OrderDate = DateTime.Now; 
            this.GetDbSet<Order>().Add(order);
            UnitOfWork.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            this.GetDbSet<Order>().Update(order);
            UnitOfWork.SaveChanges();
        }

        public void DeleteOrder(int id)
        {
            var order = GetOrderById(id);
            if (order != null)
            {
                this.GetDbSet<Order>().Remove(order);
                UnitOfWork.SaveChanges();
            }
        }

        public bool OrderExists(int id)
        {
            return this.GetDbSet<Order>().Any(o => o.Id == id);
        }

        public int GetOrdersCount()
        {
            return this.GetDbSet<Order>().Count();
        }

        public IQueryable<Order> GetOrdersByDate(DateTime date)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .Where(o => o.OrderDate.Date == date.Date)
                .OrderByDescending(o => o.OrderDate);
        }

        public IQueryable<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date)
                .OrderByDescending(o => o.OrderDate);
        }

        public IQueryable<Order> GetOrdersByStatus(string status)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate);
        }

        public IQueryable<Order> GetOrdersWithFilter(string status, string searchTerm)
        {
            var query = this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(o =>
                    o.InvoiceNumber.ToLower().Contains(searchTerm) ||
                    (o.Nickname != null && o.Nickname.ToLower().Contains(searchTerm)));
            }

            return query.OrderByDescending(o => o.OrderDate);
        }

        public decimal GetTotalRevenue()
        {
            return this.GetDbSet<Order>()
                .Where(o => o.Status != "Cancelled")
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;
        }

        public decimal GetRevenueByDate(DateTime date)
        {
            return this.GetDbSet<Order>()
                .Where(o => o.OrderDate.Date == date.Date && o.Status != "Cancelled")
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;
        }

        public int GetOrdersCountByDate(DateTime date)
        {
            return this.GetDbSet<Order>()
                .Where(o => o.OrderDate.Date == date.Date)
                .Count();
        }
    }
}
