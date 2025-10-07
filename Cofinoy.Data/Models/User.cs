using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Cofinoy.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Nickname { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string postalCode { get; set; }
    }
}
