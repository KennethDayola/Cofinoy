using AutoMapper;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cofinoy.WebApp.Controllers
{
    public class MenuController : ControllerBase<MenuController>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomizationService _customizationService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public MenuController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ICategoryService categoryService,
            ICustomizationService customizationService,
            IProductService productService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _categoryService = categoryService;
            _customizationService = customizationService;
            _productService = productService;
            _mapper = mapper;
        }


        public IActionResult Index() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult DrinkManagement() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult CategoriesManagement() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult CustomizationManagement() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        #region Category Methods

        [HttpGet]
        [HttpPost]
        public JsonResult GetAllCategories()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
                
                if (categories == null)
                {
                    categories = new List<CategoryServiceModel>();
                }
                
                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult AddCategory([FromBody] CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new CategoryServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    Status = model.Status
                };

                _categoryService.AddCategory(serviceModel);
                return Json(new { success = true, message = "Category added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult UpdateCategory(string id, [FromBody] CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new CategoryServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    Status = model.Status
                };

                _categoryService.UpdateCategory(id, serviceModel);
                return Json(new { success = true, message = "Category updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult DeleteCategory(string id)
        {
            try
            {
                _categoryService.DeleteCategory(id);
                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Customization Methods

        [HttpGet]
        public JsonResult GetAllCustomizations()
        {
            try
            {
                var customizations = _customizationService.GetAllCustomizations();
                
                var viewModels = customizations.Select(c => new CustomizationViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    Required = c.Required,
                    DisplayOrder = c.DisplayOrder,
                    Description = c.Description,
                    MaxQuantity = c.MaxQuantity,
                    PricePerUnit = c.PricePerUnit,
                    Options = c.Options?.Select(o => new CustomizationOptionViewModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceModifier = o.PriceModifier,
                        Description = o.Description,
                        Default = o.Default,
                        DisplayOrder = o.DisplayOrder
                    }).ToList() ?? new List<CustomizationOptionViewModel>()
                }).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customizations");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetCustomization(string id)
        {
            try
            {
                var customization = _customizationService.GetCustomizationById(id);
                if (customization == null)
                {
                    return Json(new { success = false, error = "Customization not found" });
                }

                var viewModel = new CustomizationViewModel
                {
                    Id = customization.Id,
                    Name = customization.Name,
                    Type = customization.Type,
                    Required = customization.Required,
                    DisplayOrder = customization.DisplayOrder,
                    Description = customization.Description,
                    MaxQuantity = customization.MaxQuantity,
                    PricePerUnit = customization.PricePerUnit,
                    Options = customization.Options?.Select(o => new CustomizationOptionViewModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceModifier = o.PriceModifier,
                        Description = o.Description,
                        Default = o.Default,
                        DisplayOrder = o.DisplayOrder
                    }).ToList() ?? new List<CustomizationOptionViewModel>()
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult AddCustomization([FromBody] CustomizationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new CustomizationServiceModel
                {
                    Name = model.Name,
                    Type = model.Type,
                    Required = model.Required,
                    DisplayOrder = model.DisplayOrder,
                    Description = model.Description,
                    MaxQuantity = model.MaxQuantity,
                    PricePerUnit = model.PricePerUnit,
                    Options = model.Options?.Select(o => new CustomizationOptionServiceModel
                    {
                        Name = o.Name,
                        PriceModifier = o.PriceModifier,
                        Description = o.Description,
                        Default = o.Default,
                        DisplayOrder = o.DisplayOrder
                    }).ToList() ?? new List<CustomizationOptionServiceModel>()
                };

                _customizationService.AddCustomization(serviceModel);
                return Json(new { success = true, message = "Customization added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult UpdateCustomization(string id, [FromBody] CustomizationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                // Map ViewModel to ServiceModel
                var serviceModel = new CustomizationServiceModel
                {
                    Name = model.Name,
                    Type = model.Type,
                    Required = model.Required,
                    DisplayOrder = model.DisplayOrder,
                    Description = model.Description,
                    MaxQuantity = model.MaxQuantity,
                    PricePerUnit = model.PricePerUnit,
                    Options = model.Options?.Select(o => new CustomizationOptionServiceModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceModifier = o.PriceModifier,
                        Description = o.Description,
                        Default = o.Default,
                        DisplayOrder = o.DisplayOrder
                    }).ToList() ?? new List<CustomizationOptionServiceModel>()
                };

                _customizationService.UpdateCustomization(id, serviceModel);
                return Json(new { success = true, message = "Customization updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult DeleteCustomization(string id)
        {
            try
            {
                _customizationService.DeleteCustomization(id);
                return Json(new { success = true, message = "Customization deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Product Methods

        [HttpGet]
        public JsonResult GetProductsByCategory(string categoryName)
        {
            try
            {
                _logger.LogInformation("Fetching products for category: {CategoryName}", categoryName);
                var products = _productService.GetProductsByCategory(categoryName);
                _logger.LogInformation("Found {Count} products for category: {CategoryName}", products.Count, categoryName);
                
                var viewModels = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Status = p.Status,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    ImagePath = p.ImagePath,
                    Categories = p.Categories,
                    Customizations = p.Customizations,
                    DisplayOrder = p.DisplayOrder,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {CategoryName}", categoryName);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAllProducts()
        {
            try
            {
                var products = _productService.GetAllProducts();
                
                var viewModels = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Status = p.Status,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    ImagePath = p.ImagePath,
                    Categories = p.Categories,
                    Customizations = p.Customizations,
                    DisplayOrder = p.DisplayOrder,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetProduct(string id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                var viewModel = new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Status = product.Status,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    ImagePath = product.ImagePath,
                    Categories = product.Categories,
                    Customizations = product.Customizations,
                    DisplayOrder = product.DisplayOrder,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult AddProduct([FromBody] ProductViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new ProductServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Status = model.Status,
                    Stock = model.Stock,
                    ImageUrl = model.ImageUrl,
                    ImagePath = model.ImagePath,
                    Categories = model.Categories ?? new List<string>(),
                    Customizations = model.Customizations ?? new List<string>(),
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                _productService.AddProduct(serviceModel);
                return Json(new { success = true, message = "Product added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult UpdateProduct(string id, [FromBody] ProductViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                var serviceModel = new ProductServiceModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Status = model.Status,
                    Stock = model.Stock,
                    ImageUrl = model.ImageUrl,
                    ImagePath = model.ImagePath,
                    Categories = model.Categories ?? new List<string>(),
                    Customizations = model.Customizations ?? new List<string>(),
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                _productService.UpdateProduct(id, serviceModel);
                return Json(new { success = true, message = "Product updated successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult DeleteProduct(string id)
        {
            try
            {
                _productService.DeleteProduct(id);
                return Json(new { success = true, message = "Product deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion
    }
}