using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.ViewModels
{
	public class CheckoutViewModel
	{
		[Required] public string TenKhachHang { get; set; } = string.Empty;
		[Required] public string SoDienThoai { get; set; } = string.Empty;
		public string? DiaChi { get; set; }
		[EmailAddress] public string? Email { get; set; }
		public string? CCCD { get; set; }
		// 1: COD, 2: Chuyển khoản (VNPay), 3: Trực tiếp
		[Range(1, 3)] public int TypePayment { get; set; } = 1;

		public List<CartItem> Items { get; set; } = new();

		public class CartItem
		{
			public string ProductId { get; set; } = string.Empty;
			public int Quantity { get; set; }
		}
	}

	public class CreateOrderResponse
	{
		public string? OrderCode { get; set; }
	}

	public class VnPayInitResponse
	{
		public string? PaymentUrl { get; set; }
	}
}

