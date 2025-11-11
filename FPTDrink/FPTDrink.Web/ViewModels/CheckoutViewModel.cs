using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.ViewModels
{
	public class CheckoutViewModel
	{
		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		public string TenKhachHang { get; set; } = string.Empty;
		
		[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
		[RegularExpression(@"^0[1-9]\d{8}$", ErrorMessage = "Số điện thoại phải có 10 số, bắt đầu bằng 0 và số thứ 2 khác 0")]
		public string SoDienThoai { get; set; } = string.Empty;
		
		public string? DiaChi { get; set; }
		
		[EmailAddress(ErrorMessage = "Email không đúng định dạng")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không được chứa dấu và phải có @")]
		public string? Email { get; set; }
		
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

