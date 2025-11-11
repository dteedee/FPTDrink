namespace FPTDrink.API.DTOs.Admin.NhanVien
{
	public class NhanVienDto
	{
		public string? Id { get; set; }
		public string? FullName { get; set; }
		public string? TenDangNhap { get; set; }
		public string? Email { get; set; }
		public string? SoDienThoai { get; set; }
		public bool? IsActiveAccount { get; set; }
		public int? Status { get; set; }
		public int? IdChucVu { get; set; }
		public string? TenChucVu { get; set; }
	}
}


