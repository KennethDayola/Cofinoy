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
using System.Threading.Tasks;

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

                var serviceModel = MapCategoryViewModelToServiceModel(model);
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

                var serviceModel = MapCategoryViewModelToServiceModel(model);
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult UpdateCategoriesDisplayOrder([FromBody] List<CategoryDisplayOrderUpdate> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                {
                    return Json(new { success = false, error = "No updates provided" });
                }

                foreach (var update in updates)
                {
                    _categoryService.UpdateCategoryDisplayOrder(update.Id, update.DisplayOrder);
                }

                return Json(new { success = true, message = "Display orders updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category display orders");
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
                var viewModels = customizations.Select(c => MapCustomizationServiceModelToViewModel(c)).ToList();

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

                var viewModel = MapCustomizationServiceModelToViewModel(customization);

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

                var serviceModel = MapCustomizationViewModelToServiceModel(model);
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

                var serviceModel = MapCustomizationViewModelToServiceModel(model);
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult UpdateCustomizationsDisplayOrder([FromBody] List<CustomizationDisplayOrderUpdate> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                {
                    return Json(new { success = false, error = "No updates provided" });
                }

                foreach (var update in updates)
                {
                    _customizationService.UpdateCustomizationDisplayOrder(update.Id, update.DisplayOrder);
                }

                return Json(new { success = true, message = "Display orders updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customization display orders");
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

                var viewModels = products.Select(p => MapProductServiceModelToViewModel(p)).ToList();

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
                var viewModels = products.Select(p => MapProductServiceModelToViewModel(p)).ToList();

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

                var viewModel = MapProductServiceModelToViewModel(product);

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

                var serviceModel = MapProductViewModelToServiceModel(model);
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

                var serviceModel = MapProductViewModelToServiceModel(model);
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

        [HttpGet]
        public JsonResult GetProductStock(string productId)
        {
            try
            {
                var product = _productService.GetProductById(productId);
                if (product == null)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                return Json(new
                {
                    success = true,
                    stock = product.Stock,
                    productName = product.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product stock");
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Mapping Methods

        private CategoryServiceModel MapCategoryViewModelToServiceModel(CategoryViewModel viewModel)
        {
            return new CategoryServiceModel
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                DisplayOrder = viewModel.DisplayOrder,
                Status = viewModel.Status
            };
        }

        private CustomizationViewModel MapCustomizationServiceModelToViewModel(CustomizationServiceModel serviceModel)
        {
            return new CustomizationViewModel
            {
                Id = serviceModel.Id,
                Name = serviceModel.Name,
                Type = serviceModel.Type,
                Required = serviceModel.Required,
                DisplayOrder = serviceModel.DisplayOrder,
                Description = serviceModel.Description,
                MaxQuantity = serviceModel.MaxQuantity,
                PricePerUnit = serviceModel.PricePerUnit,
                Options = serviceModel.Options?.Select(o => MapCustomizationOptionServiceModelToViewModel(o)).ToList() 
                    ?? new List<CustomizationOptionViewModel>()
            };
        }

        private CustomizationServiceModel MapCustomizationViewModelToServiceModel(CustomizationViewModel viewModel)
        {
            return new CustomizationServiceModel
            {
                Name = viewModel.Name,
                Type = viewModel.Type,
                Required = viewModel.Required,
                DisplayOrder = viewModel.DisplayOrder,
                Description = viewModel.Description,
                MaxQuantity = viewModel.MaxQuantity,
                PricePerUnit = viewModel.PricePerUnit,
                Options = viewModel.Options?.Select(o => MapCustomizationOptionViewModelToServiceModel(o)).ToList() 
                    ?? new List<CustomizationOptionServiceModel>()
            };
        }

        private CustomizationOptionViewModel MapCustomizationOptionServiceModelToViewModel(CustomizationOptionServiceModel serviceModel)
        {
            return new CustomizationOptionViewModel
            {
                Id = serviceModel.Id,
                Name = serviceModel.Name,
                PriceModifier = serviceModel.PriceModifier,
                Description = serviceModel.Description,
                Default = serviceModel.Default,
                DisplayOrder = serviceModel.DisplayOrder
            };
        }

        private CustomizationOptionServiceModel MapCustomizationOptionViewModelToServiceModel(CustomizationOptionViewModel viewModel)
        {
            return new CustomizationOptionServiceModel
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                PriceModifier = viewModel.PriceModifier,
                Description = viewModel.Description,
                Default = viewModel.Default,
                DisplayOrder = viewModel.DisplayOrder
            };
        }

        private ProductViewModel MapProductServiceModelToViewModel(ProductServiceModel serviceModel)
        {
            return new ProductViewModel
            {
                Id = serviceModel.Id,
                Name = serviceModel.Name,
                Description = serviceModel.Description,
                Price = serviceModel.Price,
                Status = serviceModel.Status,
                Stock = serviceModel.Stock,
                ImageUrl = serviceModel.ImageUrl,
                ImagePath = serviceModel.ImagePath,
                Categories = serviceModel.Categories,
                Customizations = serviceModel.Customizations,
                DisplayOrder = serviceModel.DisplayOrder,
                IsActive = serviceModel.IsActive,
                CreatedAt = serviceModel.CreatedAt,
                UpdatedAt = serviceModel.UpdatedAt
            };
        }

        private ProductServiceModel MapProductViewModelToServiceModel(ProductViewModel viewModel)
        {
            return new ProductServiceModel
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                Status = viewModel.Status,
                Stock = viewModel.Stock,
                ImageUrl = viewModel.ImageUrl,
                ImagePath = viewModel.ImagePath,
                Categories = viewModel.Categories ?? new List<string>(),
                Customizations = viewModel.Customizations ?? new List<string>(),
                DisplayOrder = viewModel.DisplayOrder,
                IsActive = viewModel.IsActive
            };
        }

        #endregion
    }
}