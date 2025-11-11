namespace FPTDrink.Core.Interfaces.Options
{
	public interface IVnPayOptions
	{
		string Url { get; set; }
		string Api { get; set; }
		string TmnCode { get; set; }
		string HashSecret { get; set; }
		string ReturnUrl { get; set; }
	}
}


