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
                    CartItemId = item.Id, // Include the unique cart item ID
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
                    SweetnessLevel = item.SweetnessLevel,
                    Customizations = item.Customizations?
                        .OrderBy(c => c.DisplayOrder ?? int.MaxValue) // Handle null DisplayOrder
                        .ThenBy(c => c.Name)
                        .Select(c => new CustomizationData
                        {
                            Name = c.Name,
                            Value = c.Value,
                            Type = c.Type,
                            DisplayOrder = c.DisplayOrder,
                            Price = c.Price
                        }).ToList() ?? new List<CustomizationData>()
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

                if (item.Customizations != null && item.Customizations.Any())
                {
                    _logger.LogInformation("Item has {Count} customizations", item.Customizations.Count);
                    foreach (var custom in item.Customizations)
                    {
                        _logger.LogInformation("  - {Name}: {Value} ({Type}) - Order: {DisplayOrder}, Price: {Price}", 
                            custom.Name, custom.Value, custom.Type, custom.DisplayOrder, custom.Price);
                    }
                }

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

                    // Check for matching item with same customizations
                    var existingItem = FindMatchingCartItem(cart.CartItems, item);

                    if (existingItem != null)
                    {
                        _logger.LogInformation("Updating existing item quantity");
                        existingItem.Quantity += item.Quantity;
                        existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                    }
                    else
                    {
                        _logger.LogInformation("Adding new item to cart (different customizations or new product)");
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

        public async Task UpdateCartItemQuantityAsync(string userId, string cartItemId, int quantity)
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

                // Find item by CartItemId (not ProductId)
                var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
                if (item != null)
                {
                    // Business rule: Remove if quantity <= 0
                    if (quantity <= 0)
                    {
                        cart.CartItems.Remove(item);
                        _logger.LogInformation("Removed item {CartItemId} from cart", cartItemId);
                    }
                    else
                    {
                        // Update quantity and recalculate total
                        item.Quantity = quantity;
                        item.TotalPrice = item.UnitPrice * quantity;
                        _logger.LogInformation("Updated item {CartItemId} quantity to {Quantity}", cartItemId, quantity);
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

        public async Task RemoveFromCartAsync(string userId, string cartItemId)
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

                // Find and remove item by CartItemId (not ProductId)
                var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
                if (item != null)
                {
                    cart.CartItems.Remove(item);
                    cart.UpdatedAt = DateTime.UtcNow;

                    // Save changes
                    _repository.UpdateCart(cart);
                    _logger.LogInformation("Removed item {CartItemId} from cart", cartItemId);
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

        // Helper method to find matching cart item with same customizations
        private CartItem FindMatchingCartItem(ICollection<CartItem> cartItems, CartItemServiceModel item)
        {
            foreach (var cartItem in cartItems)
            {
                if (cartItem.ProductId != item.ProductId)
                    continue;

                // Check if customizations match
                if (!CustomizationsMatch(cartItem.Customizations, item.Customizations))
                    continue;

                return cartItem;
            }

            return null;
        }

        // Helper method to compare customizations
        private bool CustomizationsMatch(ICollection<CartItemCustomization> dbCustomizations, List<CustomizationData> serviceCustomizations)
        {
            var dbList = dbCustomizations?.ToList() ?? new List<CartItemCustomization>();
            var serviceList = serviceCustomizations ?? new List<CustomizationData>();

            if (dbList.Count != serviceList.Count)
                return false;

            // Sort both lists by name for comparison
            var dbSorted = dbList.OrderBy(c => c.Name).ToList();
            var serviceSorted = serviceList.OrderBy(c => c.Name).ToList();

            for (int i = 0; i < dbSorted.Count; i++)
            {
                if (dbSorted[i].Name != serviceSorted[i].Name ||
                    dbSorted[i].Value != serviceSorted[i].Value ||
                    dbSorted[i].Type != serviceSorted[i].Type)
                {
                    return false;
                }
            }

            return true;
        }

        // Helper method to map ServiceModel to Entity
        private CartItem MapToCartItem(string cartId, CartItemServiceModel item)
        {
            var cartItem = new CartItem
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
                ExtraShots = item.ExtraShots,
                Customizations = new List<CartItemCustomization>()
            };

            // Map customizations
            if (item.Customizations != null && item.Customizations.Any())
            {
                foreach (var customization in item.Customizations)
                {
                    cartItem.Customizations.Add(new CartItemCustomization
                    {
                        Id = Guid.NewGuid().ToString(),
                        CartItemId = cartItem.Id,
                        Name = customization.Name,
                        Value = customization.Value,
                        Type = customization.Type,
                        DisplayOrder = customization.DisplayOrder,
                        Price = customization.Price
                    });
                }
            }

            return cartItem;
        }
    }
}