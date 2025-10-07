using Cofinoy.Data.Models;
using System.Linq;

namespace Cofinoy.Data.Interfaces
{
    public interface ICustomizationRepository
    {
        IQueryable<Customization> GetCustomizations();
        Customization GetCustomizationById(string id);
        void AddCustomization(Customization customization);
        void UpdateCustomization(Customization customization);
        void DeleteCustomization(string id);
        bool CustomizationExists(string id);
        int GetCustomizationsCount();
    }
}