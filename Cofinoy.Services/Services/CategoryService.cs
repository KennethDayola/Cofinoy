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
                serviceModels.Add(new CategoryServiceModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    ItemsCount = category.ItemsCount,
                    DisplayOrder = category.DisplayOrder,
                    Status = category.IsActive ? "Active" : "Inactive",
                    CreatedAt = category.CreatedAt
                });
            }

            return serviceModels;
        }

        public CategoryServiceModel GetCategoryById(string id)
        {
            var category = _repository.GetCategoryById(id);
            if (category == null)
                return null;

            return new CategoryServiceModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                ItemsCount = category.ItemsCount,
                DisplayOrder = category.DisplayOrder,
                Status = category.IsActive ? "Active" : "Inactive",
                CreatedAt = category.CreatedAt
            };
        }

        public void AddCategory(CategoryServiceModel model)
        {
            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                ItemsCount = 0,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.Status == "Active",
                CreatedAt = DateTime.UtcNow
            };

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

        public bool CategoryExists(string id)
        {
            return _repository.CategoryExists(id);
        }
    }
}