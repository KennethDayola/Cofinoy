using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.Username, _settings.DisplayName),
                Subject = "Reset your Cofinoy password",
                Body = $@"
                    <p>Hello,</p>
                    <p>Follow this link to reset your password:</p>
                    <p><a href='{resetLink}'>{resetLink}</a></p>
                    <p>If you didn’t request a reset, ignore this email.</p>
                    <p>Thanks,<br/>Cofinoy Team</p>",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await _repository.SendEmailAsync(message);
        }
    }
}
