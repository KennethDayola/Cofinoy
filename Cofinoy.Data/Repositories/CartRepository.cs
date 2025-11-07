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
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddOrUpdateCartAsync(Cart cart)
        {
            var existingCart = await this.GetDbSet<Cart>()
                .Include(c => c.CartItems)
                .AsNoTracking() // ✅ Critical: Prevent tracking conflicts
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            if (existingCart == null)
            {
                // New cart
                if (string.IsNullOrEmpty(cart.Id))
                {
                    cart.Id = Guid.NewGuid().ToString();
                }
                cart.CreatedAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;
                this.GetDbSet<Cart>().Add(cart);
            }
            else
            {
                // Update existing cart
                cart.UpdatedAt = DateTime.UtcNow;

                // ✅ Critical: Remove items that no longer exist
                var existingItemIds = existingCart.CartItems.Select(i => i.Id).ToList();
                var currentItemIds = cart.CartItems.Select(i => i.Id).ToList();
                var itemsToRemove = existingItemIds.Except(currentItemIds).ToList();

                foreach (var itemId in itemsToRemove)
                {
                    var itemToRemove = this.GetDbSet<CartItem>().FirstOrDefault(i => i.Id == itemId);
                    if (itemToRemove != null)
                    {
                        this.GetDbSet<CartItem>().Remove(itemToRemove);
                    }
                }

                this.GetDbSet<Cart>().Update(cart);
            }

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