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
			string sender = _config["Email:Sender"] ?? "";
			string password = _config["Email:Password"] ?? "";
			if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(password))
			{
				return;
			}
			using var client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(sender, password)
			};
			using var msg = new MailMessage(sender, to, subject, htmlBody) { IsBodyHtml = true };
			await client.SendMailAsync(msg, cancellationToken);
		}
	}
}


