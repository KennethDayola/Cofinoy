using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Services.ServiceModels
{
    public class ChangePasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }

        public ChangePasswordResult()
        {
            Errors = new Dictionary<string, string[]>();
        }
    }
}
