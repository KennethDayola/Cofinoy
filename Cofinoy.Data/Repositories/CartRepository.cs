using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.Data.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CofinoyDbContext _context;

        public CartRepository(CofinoyDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddOrUpdateCartAsync(Cart cart)
        {
            try
            {
                Console.WriteLine("=== START: AddOrUpdateCartAsync ===");
                Console.WriteLine($"Cart has {cart.CartItems.Count} items to save");

                var existingCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);

                if (existingCart == null)
                {
                    Console.WriteLine("Cart is new - adding to database");
                    await _context.Carts.AddAsync(cart);
                }
                else
                {
                    Console.WriteLine("Cart exists - updating properties only");

                    // Update cart properties without touching CartItems collection
                    existingCart.UpdatedAt = cart.UpdatedAt;

                    // The CartItems are already tracked by EF, so they will be saved automatically
                    _context.Carts.Update(existingCart);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("=== SUCCESS: Cart saved without replacing items ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
    }
}