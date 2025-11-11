using System;

namespace FPTDrink.Core.Models.Reports
{
	public class PaymentMethodStatPoint
	{
		public DateTime Date { get; set; }
		public int OnlineCount { get; set; }
		public int OfflineCount { get; set; }
	}

	public class InvoiceBrief
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; }
		public string ID_KhachHang { get; set; } = string.Empty;
		public string TenKhachHang { get; set; } = string.Empty;
		public int PhuongThucThanhToan { get; set; }
		public int TrangThai { get; set; }
		public decimal TongHoaDon { get; set; }
	}
}


