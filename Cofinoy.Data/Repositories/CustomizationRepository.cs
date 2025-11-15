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
            var customization = this.GetDbSet<Customization>()
                .Include(c => c.Options)
                .FirstOrDefault(c => c.Id == id);

            if (customization?.Options != null)
            {
                customization.Options = customization.Options
                    .OrderBy(o => o.DisplayOrder)
                    .ThenBy(o => o.Name)
                    .ToList();
            }

            return customization;
        }

        public void AddCustomization(Customization customization)
        {
            this.GetDbSet<Customization>().Add(customization);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCustomization(Customization customization)
        {
            this.GetDbSet<Customization>().Update(customization);
            UnitOfWork.SaveChanges();
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