using Cofinoy.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.WebApp.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; } = new();

        public PersonalInfoViewModel PersonalInfo { get; set; } = new();
        public AddressViewModel Address { get; set; } = new();
    }
}
