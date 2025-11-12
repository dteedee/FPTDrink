using System;
using System.Collections.Generic;

namespace FPTDrink.API.DTOs.Public.Checkout
{
	public class OrderDetailDto
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public string TenKhachHang { get; set; } = string.Empty;
		public string SoDienThoai { get; set; } = string.Empty;
		public string DiaChi { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int PhuongThucThanhToan { get; set; }
		public int TrangThai { get; set; }
		public DateTime CreatedDate { get; set; }
		public List<OrderDetailItemDto> ChiTietHoaDons { get; set; } = new();
	}

	public class OrderDetailItemDto
	{
		public string? ProductId { get; set; }
		public decimal GiaBan { get; set; }
		public int GiamGia { get; set; }
		public int SoLuong { get; set; }
		public OrderDetailProductDto? Product { get; set; }
	}

	public class OrderDetailProductDto
	{
		public string? MaSanPham { get; set; }
		public string? Title { get; set; }
		public string? Image { get; set; }
	}
}

