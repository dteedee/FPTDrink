using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Checkout
{
	public class VnPayInitRequest
	{
		[Required] public string OrderCode { get; set; } = string.Empty;
		[Range(0, 3)] public int TypePaymentVN { get; set; } = 0;
		public string? ReturnUrlOverride { get; set; }
		public string? ClientIp { get; set; }
	}

	public class VnPayReturnDto
	{
		public string OrderCode { get; set; } = string.Empty;
		public long Amount { get; set; }
		public string ResponseCode { get; set; } = string.Empty;
		public string TransactionStatus { get; set; } = string.Empty;
		public bool Success { get; set; }
	}
}


