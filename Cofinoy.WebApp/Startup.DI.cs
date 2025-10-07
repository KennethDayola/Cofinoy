using Cofinoy.Data;
using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Cofinoy.Data.Repositories;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.ServiceModels;
using Cofinoy.Services.Services;
using Cofinoy.WebApp.Authentication;
using Cofinoy.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cofinoy.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<ICategoryService, CategoryService>();
            this._services.AddScoped<ICustomizationService, CustomizationService>();
            this._services.AddScoped<IProductService, ProductService>();

            //Email Service
            this._services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            this._services.AddScoped<IEmailRepository, EmailRepository>();
            this._services.AddScoped<IEmailService, EmailService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<ICategoryRepository, CategoryRepository>();
            this._services.AddScoped<ICustomizationRepository, CustomizationRepository>();
            this._services.AddScoped<IProductRepository, ProductRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            this._services.AddHttpClient();
        }
    }
}
