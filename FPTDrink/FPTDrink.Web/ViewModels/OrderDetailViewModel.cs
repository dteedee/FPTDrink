using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
	public class OrderDetailViewModel
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public string TenKhachHang { get; set; } = string.Empty;
		public string SoDienThoai { get; set; } = string.Empty;
		public string DiaChi { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int PhuongThucThanhToan { get; set; }
		public int TrangThai { get; set; }
		public DateTime CreatedDate { get; set; }
		public List<OrderItemViewModel> ChiTietHoaDons { get; set; } = new();

		[JsonIgnore]
		public decimal TongTien => ChiTietHoaDons.Sum(x => x.ThanhTien);

		[JsonIgnore]
		public string PaymentMethodName => PhuongThucThanhToan switch
		{
			1 => "COD - Thanh toán khi nhận hàng",
			2 => "Thanh toán qua VNPay",
			3 => "Thanh toán trực tiếp tại cửa hàng",
			_ => "Khác"
		};

		[JsonIgnore]
		public string TrangThaiHienThi => TrangThai switch
		{
			1 => "Chờ xử lý",
			2 => "Đã thanh toán",
			3 => "Đang giao",
			4 => "Hoàn tất",
			5 => "Đã hủy",
			_ => "Không xác định"
		};

		[JsonIgnore]
		public bool DaThanhToan => PhuongThucThanhToan == 2 ? TrangThai >= 2 : TrangThai >= 1;
	}

	public class OrderItemViewModel
	{
		public string? ProductId { get; set; }
		public decimal GiaBan { get; set; }
		public int GiamGia { get; set; }
		public int SoLuong { get; set; }
		public OrderItemProductViewModel? Product { get; set; }

		[JsonIgnore]
		public decimal GiaSauGiam
		{
			get
			{
				var discountPercent = Math.Clamp(GiamGia, 0, 100);
				var factor = (100 - discountPercent) / 100m;
				return Math.Round(GiaBan * factor, 2);
			}
		}

		[JsonIgnore]
		public decimal ThanhTien => Math.Round(GiaSauGiam * SoLuong, 2);
	}

	public class OrderItemProductViewModel
	{
		public string? MaSanPham { get; set; }
		public string? Title { get; set; }
		public string? Image { get; set; }
	}
}

