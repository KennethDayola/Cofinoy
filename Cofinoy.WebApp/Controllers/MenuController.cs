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



        [HttpGet]
        public JsonResult GetAllCategories()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
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

        [HttpGet]
        public JsonResult GetAllCustomizations()
        {
            try
            {
                var customizations = _customizationService.GetAllCustomizations();
                return Json(new { success = true, data = customizations });
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
                return Json(new { success = true, data = customization });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customization");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult AddCustomization([FromBody] CustomizationServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _customizationService.AddCustomization(model);
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
        public JsonResult UpdateCustomization(string id, [FromBody] CustomizationServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _customizationService.UpdateCustomization(id, model);
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


        [HttpGet]
        public JsonResult GetProductsByCategory(string categoryName)
        {
            try
            {
                _logger.LogInformation("Fetching products for category: {CategoryName}", categoryName);
                var products = _productService.GetProductsByCategory(categoryName);
                _logger.LogInformation("Found {Count} products for category: {CategoryName}", products.Count, categoryName);
                return Json(new { success = true, data = products });
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
                return Json(new { success = true, data = products });
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
                return Json(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult AddProduct([FromBody] ProductServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _productService.AddProduct(model);
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
        public JsonResult UpdateProduct(string id, [FromBody] ProductServiceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data" });
                }

                _productService.UpdateProduct(id, model);
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
    }
}