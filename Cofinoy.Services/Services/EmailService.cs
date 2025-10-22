using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cofinoy.Data.Interfaces;
using Cofinoy.Services.Interfaces;
using Cofinoy.Data.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Cofinoy.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _repository;
        private readonly EmailSettings _settings;

        public EmailService(IEmailRepository repository, IOptions<EmailSettings> settings)
        {
            _repository = repository;
            _settings = settings.Value;
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string code)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.Username, _settings.DisplayName),
                Subject = "Your Cofinoy Password Reset Code",
                Body = $@"
            <p>Hello,</p>
            <p>Your Cofinoy password reset code is: <strong>{code}</strong></p>
            <p>This code will expire in 10 minutes.</p>
            <p>If you didn’t request a reset, you can ignore this message.</p>
            <p>Thanks,<br/>Cofinoy Team</p>",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await _repository.SendEmailAsync(message);
        }
    }
}
