using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofinoy.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository repository, ILogger<CartService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<CartItemServiceModel>> GetCartItemsAsync(string userId)
        {
            try
            {
                var cart = await _repository.GetCartByUserIdAsync(userId);
                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return new List<CartItemServiceModel>();

                return cart.CartItems.Select(item => new CartItemServiceModel
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Description = item.Description,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    ImageUrl = item.ImageUrl,
                    Size = item.Size,
                    MilkType = item.MilkType,
                    Temperature = item.Temperature,
                    ExtraShots = item.ExtraShots,
                    SweetnessLevel = item.SweetnessLevel
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items for user {UserId}", userId);
                return new List<CartItemServiceModel>();
            }
        }

        public async Task AddToCartAsync(string userId, CartItemServiceModel item)
        {
            try
            {
                _logger.LogInformation("Adding item to cart for user {UserId}, Product: {ProductId}", userId, item.ProductId);

                var cart = await _repository.GetCartByUserIdAsync(userId);

                if (cart == null)
                {
                    _logger.LogInformation("Creating new cart for user {UserId}", userId);

                    cart = new Cart
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };

                    var cartItem = MapToCartItem(cart.Id, item);
                    cart.CartItems.Add(cartItem);

                    _repository.AddCart(cart);
                }
                else
                {
                    _logger.LogInformation("Found existing cart with {Count} items", cart.CartItems?.Count ?? 0);

                    var existingItem = cart.CartItems.FirstOrDefault(i =>
                        i.ProductId == item.ProductId &&
                        i.Size == item.Size &&
                        i.MilkType == item.MilkType &&
                        i.Temperature == item.Temperature &&
                        i.SweetnessLevel == item.SweetnessLevel &&
                        i.ExtraShots == item.ExtraShots);

                    if (existingItem != null)
                    {
                        _logger.LogInformation("Updating existing item quantity");
                        existingItem.Quantity += item.Quantity;
                        existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                    }
                    else
                    {
                        _logger.LogInformation("Adding new item to cart");
                        var cartItem = MapToCartItem(cart.Id, item);
                        cart.CartItems.Add(cartItem);
                    }

                    cart.UpdatedAt = DateTime.UtcNow;

                    _repository.UpdateCart(cart);
                }

                _logger.LogInformation("Cart updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                throw;
            }
        }

        public async Task UpdateCartItemQuantityAsync(string userId, string productId, int quantity)
        {
            try
            {
                // Get cart
                var cart = await _repository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart not found for user {UserId}", userId);
                    return;
                }

                // Find item
                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    // Business rule: Remove if quantity <= 0
                    if (quantity <= 0)
                    {
                        cart.CartItems.Remove(item);
                        _logger.LogInformation("Removed item {ProductId} from cart", productId);
                    }
                    else
                    {
                        // Update quantity and recalculate total
                        item.Quantity = quantity;
                        item.TotalPrice = item.UnitPrice * quantity;
                        _logger.LogInformation("Updated item {ProductId} quantity to {Quantity}", productId, quantity);
                    }

                    cart.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    _repository.UpdateCart(cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            try
            {
                // Get cart
                var cart = await _repository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart not found for user {UserId}", userId);
                    return;
                }

                // Find and remove item
                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    cart.CartItems.Remove(item);
                    cart.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    _repository.UpdateCart(cart);
                    _logger.LogInformation("Removed item {ProductId} from cart", productId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item for user {UserId}", userId);
                throw;
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            try
            {
                await _repository.ClearCartAsync(userId);
                _logger.LogInformation("Cleared cart for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }

        // Helper method to map ServiceModel to Entity
        private CartItem MapToCartItem(string cartId, CartItemServiceModel item)
        {
            return new CartItem
            {
                Id = Guid.NewGuid().ToString(),
                CartId = cartId,
                ProductId = item.ProductId,
                ProductName = item.Name,
                Description = item.Description ?? string.Empty,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity,
                ImageUrl = item.ImageUrl ?? string.Empty,
                Temperature = item.Temperature ?? string.Empty,
                Size = item.Size ?? string.Empty,
                MilkType = item.MilkType ?? string.Empty,
                SweetnessLevel = item.SweetnessLevel ?? string.Empty,
                ExtraShots = item.ExtraShots
            };
        }
    }
}