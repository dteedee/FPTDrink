using System.Collections.Generic;

namespace FPTDrink.Core.Models.Reports
{
	public class CustomerOrderBrief
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public System.DateTime ThoiGianLap { get; set; }
		public string? SoDienThoai { get; set; }
		public string? Email { get; set; }
		public string? DiaChi { get; set; }
		public int Status { get; set; }
	}

	public class CustomerDetailsInfo
	{
		public string MaKhachHang { get; set; } = string.Empty;
		public string? TenKhachHang { get; set; }
		public List<CustomerOrderBrief> HoaDons { get; set; } = new List<CustomerOrderBrief>();
	}
}


