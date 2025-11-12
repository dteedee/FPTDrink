using System.Collections.Generic;

namespace FPTDrink.Web.ViewModels
{
	public class OrderLookupViewModel
	{
		public string? OrderCode { get; set; }
		public string? TenKhachHang { get; set; }
		public string? SoDienThoai { get; set; }
		public List<OrderDetailViewModel> Results { get; set; } = new();
		public bool Searched { get; set; }
		public bool SearchedByCode { get; set; }
		public string? Message { get; set; }
	}
}

