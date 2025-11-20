using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Data.Repositories
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Product> GetProducts()
        {
            return this.GetDbSet<Product>()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductCustomizations)
                    .ThenInclude(pc => pc.Customization)
                .OrderByDescending(p => p.UpdatedAt)
                .ThenByDescending(p => p.CreatedAt);
        }

        public IQueryable<Product> GetProductsByCategory(string categoryName)
        {
            return this.GetDbSet<Product>()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductCustomizations)
                    .ThenInclude(pc => pc.Customization)
                .Where(p => p.ProductCategories.Any(pc => pc.Category.Name == categoryName))
                .OrderByDescending(p => p.UpdatedAt)
                .ThenByDescending(p => p.CreatedAt);
        }

        public Product GetProductById(string id)
        {
            return this.GetDbSet<Product>()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductCustomizations)
                    .ThenInclude(pc => pc.Customization)
                .FirstOrDefault(p => p.Id == id);
        }

        public void AddProduct(Product product)
        {
            this.GetDbSet<Product>().Add(product);
            UnitOfWork.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            this.GetDbSet<Product>().Update(product);
            UnitOfWork.SaveChanges();
        }

        public void DeleteProduct(string id)
        {
            var product = GetProductById(id);
            if (product != null)
            {
                this.GetDbSet<Product>().Remove(product);
                UnitOfWork.SaveChanges();
            }
        }

        public bool ProductExists(string id)
        {
            return this.GetDbSet<Product>().Any(p => p.Id == id);
        }

        public int GetProductsCount()
        {
            return this.GetDbSet<Product>().Count();
        }

        public void AddProductCategories(string productId, List<string> categoryIds)
        {
            foreach (var categoryId in categoryIds)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId
                };
                this.GetDbSet<ProductCategory>().Add(productCategory);
            }
            UnitOfWork.SaveChanges();
        }

        public void RemoveProductCategories(string productId)
        {
            var productCategories = this.GetDbSet<ProductCategory>()
                .Where(pc => pc.ProductId == productId)
                .ToList();

            this.GetDbSet<ProductCategory>().RemoveRange(productCategories);
            UnitOfWork.SaveChanges();
        }

        public List<string> GetProductCategoryIds(string productId)
        {
            return this.GetDbSet<ProductCategory>()
                .Where(pc => pc.ProductId == productId)
                .Select(pc => pc.CategoryId)
                .ToList();
        }

        public void AddProductCustomizations(string productId, List<string> customizationIds)
        {
            foreach (var customizationId in customizationIds)
            {
                var productCustomization = new ProductCustomization
                {
                    ProductId = productId,
                    CustomizationId = customizationId
                };
                this.GetDbSet<ProductCustomization>().Add(productCustomization);
            }
            UnitOfWork.SaveChanges();
        }

        public void RemoveProductCustomizations(string productId)
        {
            var productCustomizations = this.GetDbSet<ProductCustomization>()
                .Where(pc => pc.ProductId == productId)
                .ToList();

            this.GetDbSet<ProductCustomization>().RemoveRange(productCustomizations);
            UnitOfWork.SaveChanges();
        }

        public List<string> GetProductCustomizationIds(string productId)
        {
            return this.GetDbSet<ProductCustomization>()
                .Where(pc => pc.ProductId == productId)
                .Select(pc => pc.CustomizationId)
                .ToList();
        }

        public void ReduceStock(string productId, int quantity)
        {
            var product = GetProductById(productId);
            if (product != null)
            {
                product.Stock -= quantity;
                this.GetDbSet<Product>().Update(product);
                UnitOfWork.SaveChanges();
            }
        }

        public bool HasSufficientStock(string productId, int quantity)
        {
            var product = GetProductById(productId);
            return product != null && product.Stock >= quantity;
        }
    }
}