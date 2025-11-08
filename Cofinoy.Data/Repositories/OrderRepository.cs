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
                .OrderByDescending(o => o.OrderDate);
        }

        public IQueryable<Order> GetOrdersByUserId(string userId)   
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);
        }

        public Order GetOrderById(int id)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);
        }

        public void AddOrder(Order order)
        {
            order.OrderDate = DateTime.UtcNow;
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
    }
}