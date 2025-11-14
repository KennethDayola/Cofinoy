using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{

    public class CustomizationOptionServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal PriceModifier { get; set; }
        public string Description { get; set; }
        public bool Default { get; set; }
        public int DisplayOrder { get; set; }
    }
}