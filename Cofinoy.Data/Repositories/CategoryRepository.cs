using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;

namespace Cofinoy.Data.Repositories
{
    public class CategoryRepository : BaseRepository, ICategoryRepository
    {
        public CategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Category> GetCategories()
        {
            return this.GetDbSet<Category>()
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CreatedAt);
        }

        public Category GetCategoryById(string id)
        {
            return this.GetDbSet<Category>()
                .FirstOrDefault(c => c.Id == id);
        }

        public void AddCategory(Category category)
        {
            category.Id = Guid.NewGuid().ToString();
            category.CreatedAt = DateTime.UtcNow;
            this.GetDbSet<Category>().Add(category);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCategory(Category category)
        {
            var existingCategory = GetCategoryById(category.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.DisplayOrder = category.DisplayOrder;
                existingCategory.IsActive = category.IsActive;

                this.GetDbSet<Category>().Update(existingCategory);
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteCategory(string id)
        {
            var category = GetCategoryById(id);
            if (category != null)
            {
                this.GetDbSet<Category>().Remove(category);
                UnitOfWork.SaveChanges();
            }
        }

        public bool CategoryExists(string id)
        {
            return this.GetDbSet<Category>().Any(c => c.Id == id);
        }

        public int GetCategoriesCount()
        {
            return this.GetDbSet<Category>().Count();
        }
    }
}