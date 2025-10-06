using System.Collections.Generic;

namespace Cofinoy.Services.ServiceModels
{
    public class CustomizationServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public int DisplayOrder { get; set; }
        public string Description { get; set; }
        public int? MaxQuantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public List<CustomizationOptionServiceModel> Options { get; set; }
    }
}