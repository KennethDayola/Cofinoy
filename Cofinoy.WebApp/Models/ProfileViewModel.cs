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

        // E add ari ang ProfileDetailsViewModel para magamit adtos ProfileDetails.cshmtl - Composite model ni sya kay dli pwede daghan model ang tawagon adto sa cshmtl
    }
}
