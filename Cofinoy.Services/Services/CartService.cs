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
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<List<CartItemServiceModel>> GetCartItemsAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
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
                Console.WriteLine("=== START: AddToCartAsync ===");
                Console.WriteLine($"User: {userId}, Product: {item.ProductId}, Quantity: {item.Quantity}");

                var cart = await _cartRepository.GetCartByUserIdAsync(userId);

                if (cart == null)
                {
                    Console.WriteLine("Creating new cart...");
                    cart = new Cart
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };

                  
                    var cartItem = CreateCartItem(cart.Id, item);
                    cart.CartItems.Add(cartItem);
                    Console.WriteLine("Added first item to new cart");
                }
                else
                {
                    Console.WriteLine($"Found existing cart with {cart.CartItems.Count} items");

                 
                    var existingItem = cart.CartItems.FirstOrDefault(i =>
                        i.ProductId == item.ProductId &&
                        i.Size == item.Size &&
                        i.MilkType == item.MilkType &&
                        i.Temperature == item.Temperature &&
                        i.SweetnessLevel == item.SweetnessLevel &&
                        i.ExtraShots == item.ExtraShots);

                    if (existingItem != null)
                    {
                     
                        Console.WriteLine("Updating quantity of existing item");
                        existingItem.Quantity += item.Quantity;
                        existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                        Console.WriteLine($"Item quantity updated to: {existingItem.Quantity}");
                    }
                    else
                    {
                       
                        Console.WriteLine("Adding as new item to existing cart");
                        var cartItem = CreateCartItem(cart.Id, item);
                        cart.CartItems.Add(cartItem);
                        Console.WriteLine("New item added to cart");
                    }

                    cart.UpdatedAt = DateTime.UtcNow;
                }

                Console.WriteLine($"Cart now has {cart.CartItems.Count} total items");
                await _cartRepository.AddOrUpdateCartAsync(cart);
                Console.WriteLine("=== SUCCESS: Item added/updated in cart ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message}");
                throw;
            }
        }

        

        public async Task UpdateCartItemQuantityAsync(string userId, string productId, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null) return;

                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.CartItems.Remove(item);
                    }
                    else
                    {
                        item.Quantity = quantity;
                        item.TotalPrice = item.UnitPrice * quantity;
                    }
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _cartRepository.AddOrUpdateCartAsync(cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for user {UserId}", userId);
                throw;
            }
        }


        //helper
        private CartItem CreateCartItem(string cartId, CartItemServiceModel item)
        {
            return new CartItem
            {
                Id = Guid.NewGuid().ToString(),
                CartId = cartId,
                ProductId = item.ProductId,
                ProductName = item.Name,
                Description = item.Description,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity,
                ImageUrl = item.ImageUrl,
                Temperature = item.Temperature,
                Size = item.Size,
                MilkType = item.MilkType,
                SweetnessLevel = item.SweetnessLevel,
                ExtraShots = item.ExtraShots
            };
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null) return;

                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    cart.CartItems.Remove(item);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _cartRepository.AddOrUpdateCartAsync(cart);
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
                await _cartRepository.ClearCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }
    }
}