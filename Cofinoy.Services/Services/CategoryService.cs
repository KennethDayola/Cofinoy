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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public List<CategoryServiceModel> GetAllCategories()
        {
            var categories = _repository.GetCategories().ToList();
            var serviceModels = new List<CategoryServiceModel>();

            foreach (var category in categories)
            {
                serviceModels.Add(MapToServiceModel(category));
            }

            return serviceModels;
        }

        public CategoryServiceModel GetCategoryById(string id)
        {
            var category = _repository.GetCategoryById(id);
            return category == null ? null : MapToServiceModel(category);
        }

        public void AddCategory(CategoryServiceModel model)
        {
            var category = MapToEntity(model);
            category.Id = Guid.NewGuid().ToString();
            category.ItemsCount = 0;
            category.CreatedAt = DateTime.UtcNow;

            _repository.AddCategory(category);
        }

        public void UpdateCategory(string id, CategoryServiceModel model)
        {
            var existingCategory = _repository.GetCategoryById(id);
            if (existingCategory == null)
            {
                throw new InvalidDataException("Category not found");
            }

            existingCategory.Name = model.Name;
            existingCategory.Description = model.Description ?? string.Empty;
            existingCategory.DisplayOrder = model.DisplayOrder;
            existingCategory.IsActive = model.Status == "Active";

            _repository.UpdateCategory(existingCategory);
        }

        public void DeleteCategory(string id)
        {
            if (!_repository.CategoryExists(id))
            {
                throw new InvalidDataException("Category not found");
            }

            _repository.DeleteCategory(id);
        }

        public void UpdateCategoryDisplayOrder(string id, int displayOrder)
        {
            var existingCategory = _repository.GetCategoryById(id);
            if (existingCategory == null)
            {
                throw new InvalidDataException("Category not found");
            }

            existingCategory.DisplayOrder = displayOrder;
            _repository.UpdateCategory(existingCategory);
        }

        public bool CategoryExists(string id)
        {
            return _repository.CategoryExists(id);
        }

        private CategoryServiceModel MapToServiceModel(Category entity)
        {
            return new CategoryServiceModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                ItemsCount = entity.ItemsCount,
                DisplayOrder = entity.DisplayOrder,
                Status = entity.IsActive ? "Active" : "Inactive",
                CreatedAt = entity.CreatedAt
            };
        }

        private Category MapToEntity(CategoryServiceModel model)
        {
            return new Category
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.Status == "Active"
            };
        }
    }
}