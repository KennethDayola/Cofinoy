using Basecode.Data.Repositories;
using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.Data.Repositories
{
    public class CartRepository : BaseRepository, ICartRepository
    {
        public CartRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await this.GetDbSet<Cart>()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Customizations)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public void AddCart(Cart cart)
        {
            this.GetDbSet<Cart>().Add(cart);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCart(Cart cart)
        {
            this.GetDbSet<Cart>().Update(cart);
            UnitOfWork.SaveChanges();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                this.GetDbSet<Cart>().Remove(cart);
                UnitOfWork.SaveChanges();
            }
        }
    }
}