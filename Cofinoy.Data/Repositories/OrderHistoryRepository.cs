using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Data.Repositories
{
    public class OrderHistoryRepository : BaseRepository, IOrderHistoryRepository
    {
        public OrderHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<Order> GetOrderHistoryByUserId(string userId)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order GetOrderDetailsById(int orderId)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Customizations)
                .FirstOrDefault(o => o.Id == orderId);
        }

        public List<Order> GetOrderStatusesByUserId(string userId)
        {
            return this.GetDbSet<Order>()
                .Where(o => o.UserId == userId)
                .Select(o => new Order
                {
                    Id = o.Id,
                    Status = o.Status
                })
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }
    }
}
