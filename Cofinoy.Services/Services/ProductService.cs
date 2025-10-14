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
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                BasePrice = model.Price,
                Status = model.Status ?? "Available",
                Stock = int.TryParse(model.Stock, out int stock) ? stock : 0,
                ImageUrl = model.ImageUrl ?? string.Empty,
                ImagePath = model.ImagePath ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsAvailable = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _productRepository.AddProduct(product);

            // Add category relationships
            if (model.Categories != null && model.Categories.Count > 0)
            {
                _productRepository.AddProductCategories(product.Id, model.Categories);

                // Update category item counts
                foreach (var categoryId in model.Categories)
                {
                    UpdateCategoryItemCount(categoryId, true);
                }
            }

            // Add customization relationships
            if (model.Customizations != null && model.Customizations.Count > 0)
            {
                _productRepository.AddProductCustomizations(product.Id, model.Customizations);
            }
        }

        public void UpdateProduct(string id, ProductServiceModel model)
        {
            if (!_productRepository.ProductExists(id))
            {
                throw new InvalidDataException("Product not found");
            }

            var existingProduct = _productRepository.GetProductById(id);

            // Get old categories for count updates
            var oldCategoryIds = existingProduct.ProductCategories
                .Select(pc => pc.CategoryId)
                .ToList();

            var product = new Product
            {
                Id = id,
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                BasePrice = model.Price,
                Status = model.Status ?? "Available",
                Stock = int.TryParse(model.Stock, out int stock) ? stock : 0,
                ImageUrl = model.ImageUrl ?? existingProduct.ImageUrl,
                ImagePath = model.ImagePath ?? existingProduct.ImagePath,
                DisplayOrder = model.DisplayOrder,
                IsAvailable = model.IsActive
            };

            _productRepository.UpdateProduct(product);

            // Update category relationships
            _productRepository.RemoveProductCategories(id);

            if (model.Categories != null && model.Categories.Count > 0)
            {
                _productRepository.AddProductCategories(id, model.Categories);
            }

            // Update category counts
            // Decrement old categories not in new list
            foreach (var oldCatId in oldCategoryIds)
            {
                if (!model.Categories.Contains(oldCatId))
                {
                    UpdateCategoryItemCount(oldCatId, false);
                }
            }

            // Increment new categories not in old list
            foreach (var newCatId in model.Categories)
            {
                if (!oldCategoryIds.Contains(newCatId))
                {
                    UpdateCategoryItemCount(newCatId, true);
                }
            }

            // Update customization relationships
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

            // Get categories before deletion to update counts
            var categoryIds = product.ProductCategories
                .Select(pc => pc.CategoryId)
                .ToList();

            _productRepository.DeleteProduct(id);

            // Update category item counts
            foreach (var categoryId in categoryIds)
            {
                UpdateCategoryItemCount(categoryId, false);
            }
        }

        public bool ProductExists(string id)
        {
            return _productRepository.ProductExists(id);
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