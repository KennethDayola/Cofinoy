using AutoMapper;
using Cofinoy.Data.Models;
using Cofinoy.Services.ServiceModels;
using Cofinoy.WebApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cofinoy.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {
                CreateMap<UserServiceModel, User>();
                CreateMap<PersonalInfoViewModel, PersonalInfoServiceModel>();
                CreateMap<AddressViewModel, AddressServiceModel>();
                CreateMap<ChangePasswordViewModel, ChangePasswordServiceModel>();
            }
        }
    }
}
