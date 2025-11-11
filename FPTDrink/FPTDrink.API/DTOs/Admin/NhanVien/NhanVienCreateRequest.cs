using System.ComponentModel.DataAnnotations;
using FPTDrink.API.Validators;

namespace FPTDrink.API.DTOs.Admin.NhanVien
{
	public class NhanVienCreateRequest
	{
		[Required, StringLength(150)]
		public string? FullName { get; set; }
		public string? TenHienThi { get; set; }
		[Required, MinLength(3)]
		public string TenDangNhap { get; set; } = string.Empty;
		[Required, MinLength(3)]
		public string MatKhau { get; set; } = string.Empty;
		[AgeRange(18, 40)]
		public DateTime NgaySinh { get; set; }
		public bool GioiTinh { get; set; }
		[EmailAddress] public string? Email { get; set; }
		[StringLength(20)] public string? SoDienThoai { get; set; }
		public string? DiaChi { get; set; }
		public int? IdChucVu { get; set; }
	}
}


