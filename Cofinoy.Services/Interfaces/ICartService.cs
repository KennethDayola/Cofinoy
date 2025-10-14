using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cofinoy.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemServiceModel>> GetCartItemsAsync(string userId); 
        Task AddToCartAsync(string userId, CartItemServiceModel item);     
        Task UpdateCartItemQuantityAsync(string userId, string productId, int quantity);
        Task RemoveFromCartAsync(string userId, string productId);
        Task ClearCartAsync(string userId);
    }
}