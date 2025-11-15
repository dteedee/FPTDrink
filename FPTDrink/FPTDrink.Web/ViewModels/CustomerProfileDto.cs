namespace FPTDrink.Web.ViewModels
{
	public class CustomerProfileDto
	{
		public string Id { get; set; } = string.Empty;
		public string TenDangNhap { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string SoDienThoai { get; set; } = string.Empty;
		public string HoTen { get; set; } = string.Empty;
		public DateTime? NgaySinh { get; set; }
		public bool? GioiTinh { get; set; }
		public string? DiaChi { get; set; }
		public string? Image { get; set; }
		public bool IsEmailVerified { get; set; }
	}
}

