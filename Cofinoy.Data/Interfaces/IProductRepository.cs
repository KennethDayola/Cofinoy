using Cofinoy.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Data.Interfaces
{
    public interface IProductRepository
    {
        IQueryable<Product> GetProducts();
        IQueryable<Product> GetProductsByCategory(string categoryName);
        Product GetProductById(string id);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(string id);
        bool ProductExists(string id);
        int GetProductsCount();

        // Category relationships
        void AddProductCategories(string productId, List<string> categoryIds);
        void RemoveProductCategories(string productId);
        List<string> GetProductCategoryIds(string productId);

        // Customization relationships
        void AddProductCustomizations(string productId, List<string> customizationIds);
        void RemoveProductCustomizations(string productId);
        List<string> GetProductCustomizationIds(string productId);

        // Stock management
        void ReduceStock(string productId, int quantity);
        bool HasSufficientStock(string productId, int quantity);
    }
}