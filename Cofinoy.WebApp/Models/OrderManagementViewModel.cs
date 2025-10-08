namespace Cofinoy.WebApp.Models
{
    public class OrderManagementViewModel
    {
        public string Id { get; set; }
        public string Customer { get; set; }
        public string DateTime { get; set; }
        public int Items { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }

    }
}
