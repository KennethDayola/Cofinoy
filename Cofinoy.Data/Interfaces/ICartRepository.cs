using Cofinoy.Data.Models;
using System.Threading.Tasks;

namespace Cofinoy.Data.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task AddOrUpdateCartAsync(Cart cart);
        Task ClearCartAsync(string userId);
    }
}