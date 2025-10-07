using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly EmailSettings _settings;

        public EmailRepository(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(MailMessage message)
        {
            using (var smtp = new SmtpClient(_settings.Host, _settings.Port))
            {
                smtp.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                smtp.EnableSsl = _settings.EnableSsl;
                await smtp.SendMailAsync(message);
            }
        }
    }
}
