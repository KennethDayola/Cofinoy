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
    public class CustomizationService : ICustomizationService
    {
        private readonly ICustomizationRepository _repository;

        public CustomizationService(ICustomizationRepository repository)
        {
            _repository = repository;
        }

        public List<CustomizationServiceModel> GetAllCustomizations()
        {
            var customizations = _repository.GetCustomizations().ToList();
            var serviceModels = new List<CustomizationServiceModel>();

            foreach (var customization in customizations)
            {
                serviceModels.Add(MapToServiceModel(customization));
            }

            return serviceModels;
        }

        public CustomizationServiceModel GetCustomizationById(string id)
        {
            var customization = _repository.GetCustomizationById(id);
            return customization == null ? null : MapToServiceModel(customization);
        }

        public void AddCustomization(CustomizationServiceModel model)
        {
            var customization = MapToEntity(model);
            _repository.AddCustomization(customization);
        }

        public void UpdateCustomization(string id, CustomizationServiceModel model)
        {
            var existing = _repository.GetCustomizationById(id);
            if (existing == null)
            {
                throw new InvalidDataException("Customization not found");
            }

            existing.Name = model.Name;
            existing.Type = model.Type;
            existing.Required = model.Required;
            existing.DisplayOrder = model.DisplayOrder;
            existing.Description = model.Description ?? string.Empty;
            existing.MaxQuantity = model.MaxQuantity;
            existing.PricePerUnit = model.PricePerUnit;

           
            existing.Options.Clear();

            if (model.Options != null && model.Options.Any())
            {
                foreach (var optionModel in model.Options)
                {
                    var option = new CustomizationOption
                    {
                        Id = string.IsNullOrEmpty(optionModel.Id)
                            ? Guid.NewGuid().ToString()
                            : optionModel.Id,
                        Name = optionModel.Name,
                        PriceModifier = optionModel.PriceModifier,
                        Description = optionModel.Description ?? string.Empty,
                        Default = optionModel.Default
                    };
                    existing.Options.Add(option);
                }
            }

            _repository.UpdateCustomization(existing);
        }

        public void DeleteCustomization(string id)
        {
            if (!_repository.CustomizationExists(id))
            {
                throw new InvalidDataException("Customization not found");
            }

            _repository.DeleteCustomization(id);
        }

        public bool CustomizationExists(string id)
        {
            return _repository.CustomizationExists(id);
        }

        private CustomizationServiceModel MapToServiceModel(Customization entity)
        {
            return new CustomizationServiceModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                Required = entity.Required,
                DisplayOrder = entity.DisplayOrder,
                Description = entity.Description ?? string.Empty,
                MaxQuantity = entity.MaxQuantity,
                PricePerUnit = entity.PricePerUnit,
                Options = entity.Options?.Select(o => new CustomizationOptionServiceModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    PriceModifier = o.PriceModifier,
                    Description = o.Description ?? string.Empty,
                    Default = o.Default
                }).ToList() ?? new List<CustomizationOptionServiceModel>()
            };
        }

        private Customization MapToEntity(CustomizationServiceModel model)
        {
            return new Customization
            {
                Name = model.Name,
                Type = model.Type,
                Required = model.Required,
                DisplayOrder = model.DisplayOrder,
                Description = model.Description ?? string.Empty,
                MaxQuantity = model.MaxQuantity,
                PricePerUnit = model.PricePerUnit,
                Options = model.Options?.Select(o => new CustomizationOption
                {
                    Id = string.IsNullOrEmpty(o.Id) ? Guid.NewGuid().ToString() : o.Id,
                    Name = o.Name,
                    PriceModifier = o.PriceModifier,
                    Description = o.Description ?? string.Empty,
                    Default = o.Default
                }).ToList() ?? new List<CustomizationOption>()
            };
        }
    }
}