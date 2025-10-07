using Cofinoy.Data.Models;
using System.Linq;

namespace Cofinoy.Data.Interfaces
{
    public interface ICategoryRepository
    {
        IQueryable<Category> GetCategories();
        Category GetCategoryById(string id);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(string id);
        bool CategoryExists(string id);
        int GetCategoriesCount();
    }
}