using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;

namespace Cofinoy.Services.Interfaces
{
    public interface IProductService
    {
        List<ProductServiceModel> GetAllProducts();
        List<ProductServiceModel> GetProductsByCategory(string categoryName);
        ProductServiceModel GetProductById(string id);
        void AddProduct(ProductServiceModel model);
        void UpdateProduct(string id, ProductServiceModel model);
        void DeleteProduct(string id);
        bool ProductExists(string id);
        
        // Stock management
        void ReduceStock(string productId, int quantity);
        bool HasSufficientStock(string productId, int quantity);
    }
}