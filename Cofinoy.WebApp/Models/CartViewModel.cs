using Cofinoy.Data.Models;
using Cofinoy.Services.ServiceModels;
using System.Collections.Generic;

namespace Cofinoy.WebApp.Models
{
    public class CartViewModel
    {
        public List<CartItemServiceModel> CartItems { get; set; }
        public User User { get; set; }
    }
}
