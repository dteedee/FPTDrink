using System.Collections.Generic;

namespace FPTDrink.Web.ViewModels
{
	public class CustomerOrdersViewModel
	{
		public List<CustomerOrderSummaryDto> Orders { get; set; } = new();
	}

	public class CustomerOrderSummaryDto
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public System.DateTime CreatedDate { get; set; }
		public int TrangThai { get; set; }
		public decimal TongTien { get; set; }
		public string TrangThaiHienThi => GetStatusText(TrangThai);

		private string GetStatusText(int status) => status switch
		{
			0 => "Đã hủy",
			1 => "Đang xử lý",
			2 => "Hoàn tất",
			3 => "Đang giao",
			4 => "Chờ thanh toán",
			_ => "Không xác định"
		};
	}
}

