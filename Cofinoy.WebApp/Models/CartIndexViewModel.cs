using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;

namespace Cofinoy.WebApp.Models
{
    public class CartPageViewModel
    {
        public List<CartItemServiceModel> CartItems { get; set; } = new();
        public string Nickname { get; set; }
    }

}
