using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cofinoy.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemServiceModel>> GetCartItemsAsync(string userId); 
        Task AddToCartAsync(string userId, CartItemServiceModel item);     
        Task UpdateCartItemQuantityAsync(string userId, string cartItemId, int quantity);
        Task RemoveFromCartAsync(string userId, string cartItemId);
        Task ClearCartAsync(string userId);
    }
}