using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cofinoy.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemServiceModel>> GetCartItemsAsync(string userId); 
        Task AddToCartAsync(string userId, CartItemServiceModel item);     
        Task UpdateCartItemQuantityAsync(string userId, string cartItemId, int quantity); // Changed from productId to cartItemId
        Task RemoveFromCartAsync(string userId, string cartItemId); // Changed from productId to cartItemId
        Task ClearCartAsync(string userId);
    }
}