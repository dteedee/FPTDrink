namespace FPTDrink.Core.Interfaces.Options
{
	public interface IEmailOptions
	{
		string Sender { get; set; }
		string Admin { get; set; }
		string Password { get; set; }
		string? SMTPServer { get; set; }
		int? Port { get; set; }
		bool? EnableSSL { get; set; }
		string? Username { get; set; }
		string? FromEmail { get; set; }
	}
}


