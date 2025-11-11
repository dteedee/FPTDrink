using FPTDrink.Core.Interfaces.Options;

namespace FPTDrink.Web.Extensions
{
	public class EmailOptions : IEmailOptions
	{
		public string Sender { get; set; } = string.Empty;
		public string Admin { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class VnPayOptions : IVnPayOptions
	{
		public string Url { get; set; } = string.Empty;
		public string Api { get; set; } = string.Empty;
		public string TmnCode { get; set; } = string.Empty;
		public string HashSecret { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
	}
}


