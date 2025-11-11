using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Order
{
	public class OrderDto
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public string TenKhachHang { get; set; } = string.Empty;
		public string SoDienThoai { get; set; } = string.Empty;
		public string DiaChi { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int PhuongThucThanhToan { get; set; }
		public int TrangThai { get; set; }
		public DateTime CreatedDate { get; set; }
	}

	public class OrderItemDto
	{
		public string? ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? ProductImage { get; set; }
		[Range(1, int.MaxValue)]
		public int SoLuong { get; set; }
		[Range(0, double.MaxValue)]
		public decimal GiaBan { get; set; }
	}

	public class CustomerSummaryDto
	{
		public string? MaKhachHang { get; set; }
		public string? TenKhachHang { get; set; }
		public string? CCCD { get; set; }
		public string? Email { get; set; }
		public string? SoDienThoai { get; set; }
	}

	public class CustomerOrderBriefDto
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public DateTime ThoiGianLap { get; set; }
		public string? SoDienThoai { get; set; }
		public string? Email { get; set; }
		public string? DiaChi { get; set; }
		public int Status { get; set; }
	}

	public class CustomerDetailsDto
	{
		public string MaKhachHang { get; set; } = string.Empty;
		public string? TenKhachHang { get; set; }
		public string? CCCD { get; set; }
		public List<CustomerOrderBriefDto> HoaDons { get; set; } = new();
	}
}


