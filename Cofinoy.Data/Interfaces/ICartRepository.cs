using Cofinoy.Data.Models;
using System.Threading.Tasks;

namespace Cofinoy.Data.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        void AddCart(Cart cart);
        void UpdateCart(Cart cart);
        Task ClearCartAsync(string userId);
    }
}