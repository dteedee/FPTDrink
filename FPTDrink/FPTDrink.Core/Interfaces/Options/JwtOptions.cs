namespace FPTDrink.Core.Interfaces.Options
{
	public class JwtOptions
	{
		public string Key { get; set; } = string.Empty;
		public string Issuer { get; set; } = string.Empty;
		public string Audience { get; set; } = string.Empty;
		public int ExpiryMinutes { get; set; } = 10080; // 7 ngày
	}
}

