using System;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Customer
{
	public class UpdateProfileRequestDto
	{
		[StringLength(150)]
		public string? HoTen { get; set; }

		[StringLength(20)]
		public string? SoDienThoai { get; set; }

		public DateTime? NgaySinh { get; set; }

		public bool? GioiTinh { get; set; }

		[StringLength(500)]
		public string? DiaChi { get; set; }

		[Url]
		public string? Image { get; set; }
	}

	public class CustomerOrderSummaryDto
	{
		public string MaHoaDon { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; }
		public int TrangThai { get; set; }
		public decimal TongTien { get; set; }
	}
}

