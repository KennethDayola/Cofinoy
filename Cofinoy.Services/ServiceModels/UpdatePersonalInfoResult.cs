using Cofinoy.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Services.ServiceModels
{
    public class UpdatePersonalInfoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
        public bool EmailChanged { get; set; }
        public User UpdatedUser { get; set; }
    }
}
