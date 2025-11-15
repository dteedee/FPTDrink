using FPTDrink.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FPTDrink.Infrastructure.Services
{
	public class SmtpEmailService : IEmailService
	{
		private readonly IConfiguration _config;
		public SmtpEmailService(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
		{
			var smtpServer = _config["Email:SMTPServer"] ?? _config["EmailSettings:SMTPServer"] ?? "smtp.gmail.com";
			var port = int.TryParse(_config["Email:Port"] ?? _config["EmailSettings:Port"], out var p) ? p : 587;
			var enableSsl = bool.TryParse(_config["Email:EnableSSL"] ?? _config["EmailSettings:EnableSSL"], out var ssl) ? ssl : true;
			var username = _config["Email:Username"] ?? _config["EmailSettings:Username"] ?? _config["Email:Sender"] ?? "";
			var password = _config["Email:Password"] ?? _config["EmailSettings:Password"] ?? "";
			var fromEmail = _config["Email:FromEmail"] ?? _config["EmailSettings:FromEmail"] ?? _config["Email:Sender"] ?? username;

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return;
			}

			using var client = new SmtpClient(smtpServer, port)
			{
				EnableSsl = enableSsl,
				Credentials = new NetworkCredential(username, password)
			};
			using var msg = new MailMessage(fromEmail, to, subject, htmlBody) { IsBodyHtml = true };
			msg.From = new MailAddress(fromEmail, "FPTDrink");
			await client.SendMailAsync(msg, cancellationToken);
		}
	}
}


