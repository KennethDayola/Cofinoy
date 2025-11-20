using System.Collections.Generic;
using Cofinoy.Services.ServiceModels;

namespace Cofinoy.Services.Interfaces
{
    public interface ICategoryService
    {
        List<CategoryServiceModel> GetAllCategories();
        CategoryServiceModel GetCategoryById(string id);
        void AddCategory(CategoryServiceModel model);
        void UpdateCategory(string id, CategoryServiceModel model);
        void DeleteCategory(string id);
        void UpdateCategoryDisplayOrder(string id, int displayOrder);
        bool CategoryExists(string id);
    }
}