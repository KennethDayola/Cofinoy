using Cofinoy.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data.Interfaces
{
    public interface IEmailRepository
    {
        Task SendEmailAsync(MailMessage message);
    }
}
