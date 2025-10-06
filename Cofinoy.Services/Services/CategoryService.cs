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
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                ItemsCount = 0, // Default value
                DisplayOrder = model.DisplayOrder,
                IsActive = model.Status == "Active",
                CreatedAt = DateTime.UtcNow
            };

            _repository.AddCategory(category);
        }

        public void UpdateCategory(string id, CategoryServiceModel model)
        {
            if (!_repository.CategoryExists(id))
            {
                throw new InvalidDataException("Category not found");
            }

            var category = new Category
            {
                Id = id,
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.Status == "Active"
                // ItemsCount and CreatedAt are typically not updated in this scenario
            };

            _repository.UpdateCategory(category);
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