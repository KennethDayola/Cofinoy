using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;

namespace Cofinoy.Services.Interfaces
{
    public interface ICustomizationService
    {
        List<CustomizationServiceModel> GetAllCustomizations();
        CustomizationServiceModel GetCustomizationById(string id);
        void AddCustomization(CustomizationServiceModel model);
        void UpdateCustomization(string id, CustomizationServiceModel model);
        void DeleteCustomization(string id);
        bool CustomizationExists(string id);
        void UpdateCustomizationDisplayOrder(string id, int displayOrder);
    }
}