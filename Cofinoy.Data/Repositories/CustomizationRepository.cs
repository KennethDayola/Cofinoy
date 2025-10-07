using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Cofinoy.Data.Repositories
{
    public class CustomizationRepository : BaseRepository, ICustomizationRepository
    {
        public CustomizationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Customization> GetCustomizations()
        {
            return this.GetDbSet<Customization>()
                .Include(c => c.Options)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);
        }

        public Customization GetCustomizationById(string id)
        {
            return this.GetDbSet<Customization>()
                .Include(c => c.Options)
                .FirstOrDefault(c => c.Id == id);
        }

        public void AddCustomization(Customization customization)
        {
            customization.Id = Guid.NewGuid().ToString();
            this.GetDbSet<Customization>().Add(customization);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCustomization(Customization customization)
        {
            var existing = GetCustomizationById(customization.Id);
            if (existing != null)
            {
                existing.Name = customization.Name;
                existing.Type = customization.Type;
                existing.Required = customization.Required;
                existing.DisplayOrder = customization.DisplayOrder;
                existing.Description = customization.Description;
                existing.MaxQuantity = customization.MaxQuantity;
                existing.PricePerUnit = customization.PricePerUnit;

                // Remove old options
                var existingOptions = existing.Options.ToList();
                foreach (var option in existingOptions)
                {
                    this.GetDbSet<CustomizationOption>().Remove(option);
                }

                // Add new options
                existing.Options = customization.Options;

                this.GetDbSet<Customization>().Update(existing);
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteCustomization(string id)
        {
            var customization = GetCustomizationById(id);
            if (customization != null)
            {
                this.GetDbSet<Customization>().Remove(customization);
                UnitOfWork.SaveChanges();
            }
        }

        public bool CustomizationExists(string id)
        {
            return this.GetDbSet<Customization>().Any(c => c.Id == id);
        }

        public int GetCustomizationsCount()
        {
            return this.GetDbSet<Customization>().Count();
        }
    }
}