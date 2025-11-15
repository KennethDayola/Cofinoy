using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cofinoy.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public List<ProductServiceModel> GetProductsByCategory(string categoryName)
        {
            var products = _productRepository.GetProductsByCategory(categoryName).ToList();
            var serviceModels = new List<ProductServiceModel>();

            foreach (var product in products)
            {
                var categoryIds = product.ProductCategories
                    .Select(pc => pc.CategoryId)
                    .ToList();

                var customizationIds = product.ProductCustomizations
                    .Select(pc => pc.CustomizationId)
                    .ToList();

                serviceModels.Add(new ProductServiceModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.BasePrice,
                    Status = product.Status ?? "Available",
                    Stock = product.Stock.ToString(),
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    ImagePath = product.ImagePath ?? string.Empty,
                    Categories = categoryIds,
                    Customizations = customizationIds,
                    DisplayOrder = product.DisplayOrder,
                    IsActive = product.IsAvailable,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            return serviceModels;
        }

        public List<ProductServiceModel> GetAllProducts()
        {
            var products = _productRepository.GetProducts().ToList();
            var serviceModels = new List<ProductServiceModel>();

            foreach (var product in products)
            {
                var categoryIds = product.ProductCategories
                    .Select(pc => pc.CategoryId)
                    .ToList();

                var customizationIds = product.ProductCustomizations
                    .Select(pc => pc.CustomizationId)
                    .ToList();

                serviceModels.Add(new ProductServiceModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.BasePrice,
                    Status = product.Status ?? "Available",
                    Stock = product.Stock.ToString(),
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    ImagePath = product.ImagePath ?? string.Empty,
                    Categories = categoryIds,
                    Customizations = customizationIds,
                    DisplayOrder = product.DisplayOrder,
                    IsActive = product.IsAvailable,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            return serviceModels;
        }

        public ProductServiceModel GetProductById(string id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
                return null;

            var categoryIds = product.ProductCategories
                .Select(pc => pc.CategoryId)
                .ToList();

            var customizationIds = product.ProductCustomizations
                .Select(pc => pc.CustomizationId)
                .ToList();

            return new ProductServiceModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.BasePrice,
                Status = product.Status ?? "Available",
                Stock = product.Stock.ToString(),
                ImageUrl = product.ImageUrl ?? string.Empty,
                ImagePath = product.ImagePath ?? string.Empty,
                Categories = categoryIds,
                Customizations = customizationIds,
                DisplayOrder = product.DisplayOrder,
                IsActive = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void AddProduct(ProductServiceModel model)
        {
            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                BasePrice = model.Price,
                Status = model.Status ?? "Available",
                Stock = int.TryParse(model.Stock, out int stock) ? stock : 0,
                ImageUrl = model.ImageUrl ?? string.Empty,
                ImagePath = model.ImagePath ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsAvailable = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _productRepository.AddProduct(product);

            if (model.Categories != null && model.Categories.Count > 0)
            {
                _productRepository.AddProductCategories(product.Id, model.Categories);

                foreach (var categoryId in model.Categories)
                {
                    UpdateCategoryItemCount(categoryId, true);
                }
            }

            if (model.Customizations != null && model.Customizations.Count > 0)
            {
                _productRepository.AddProductCustomizations(product.Id, model.Customizations);
            }
        }

        public void UpdateProduct(string id, ProductServiceModel model)
        {
            var existingProduct = _productRepository.GetProductById(id);
            if (existingProduct == null)
            {
                throw new InvalidDataException("Product not found");
            }

            var oldCategoryIds = existingProduct.ProductCategories
                .Select(pc => pc.CategoryId)
                .ToList();

            existingProduct.Name = model.Name;
            existingProduct.Description = model.Description ?? string.Empty;
            existingProduct.BasePrice = model.Price;
            existingProduct.Status = model.Status ?? "Available";
            existingProduct.Stock = int.TryParse(model.Stock, out int stock) ? stock : 0;
            existingProduct.ImageUrl = model.ImageUrl ?? existingProduct.ImageUrl;
            existingProduct.ImagePath = model.ImagePath ?? existingProduct.ImagePath;
            existingProduct.DisplayOrder = model.DisplayOrder;
            existingProduct.IsAvailable = model.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _productRepository.UpdateProduct(existingProduct);

            _productRepository.RemoveProductCategories(id);

            if (model.Categories != null && model.Categories.Count > 0)
            {
                _productRepository.AddProductCategories(id, model.Categories);
            }

           
            foreach (var oldCatId in oldCategoryIds)
            {
                if (model.Categories == null || !model.Categories.Contains(oldCatId))
                {
                    UpdateCategoryItemCount(oldCatId, false);
                }
            }

            if (model.Categories != null)
            {
                foreach (var newCatId in model.Categories)
                {
                    if (!oldCategoryIds.Contains(newCatId))
                    {
                        UpdateCategoryItemCount(newCatId, true);
                    }
                }
            }

            _productRepository.RemoveProductCustomizations(id);

            if (model.Customizations != null && model.Customizations.Count > 0)
            {
                _productRepository.AddProductCustomizations(id, model.Customizations);
            }
        }

        public void DeleteProduct(string id)
        {
            if (!_productRepository.ProductExists(id))
            {
                throw new InvalidDataException("Product not found");
            }

            var product = _productRepository.GetProductById(id);

            var categoryIds = product.ProductCategories
                .Select(pc => pc.CategoryId)
                .ToList();

            _productRepository.DeleteProduct(id);

            foreach (var categoryId in categoryIds)
            {
                UpdateCategoryItemCount(categoryId, false);
            }
        }

        public bool ProductExists(string id)
        {
            return _productRepository.ProductExists(id);
        }

        public void ReduceStock(string productId, int quantity)
        {
            var product = _productRepository.GetProductById(productId);
            if (product == null)
            {
                throw new InvalidDataException("Product not found");
            }

            if (!HasSufficientStock(productId, quantity))
            {
                throw new InvalidDataException("Insufficient stock available");
            }

            _productRepository.ReduceStock(productId, quantity);

            product = _productRepository.GetProductById(productId);
            
            if (product.Stock <= 0)
            {
                product.Stock = 0;
                product.Status = "Unavailable";
                product.IsAvailable = false;
            }
            
            product.UpdatedAt = DateTime.UtcNow;
            _productRepository.UpdateProduct(product);
        }

        public bool HasSufficientStock(string productId, int quantity)
        {
            return _productRepository.HasSufficientStock(productId, quantity);
        }

        private void UpdateCategoryItemCount(string categoryId, bool increment)
        {
            var category = _categoryRepository.GetCategoryById(categoryId);
            if (category != null)
            {
                if (increment)
                {
                    category.ItemsCount++;
                }
                else
                {
                    category.ItemsCount = Math.Max(0, category.ItemsCount - 1);
                }
                _categoryRepository.UpdateCategory(category);
            }
        }
    }
}