using System.Text.Json.Serialization;

namespace FPTDrink.API.DTOs.Admin.Auth
{
	public class LoginResponse
	{
		public bool Success { get; set; }
		public string? Error { get; set; }
		public UserDto? User { get; set; }
	}

	public class UserDto
	{
		public string? Id { get; set; }
		public string? TenDangNhap { get; set; }
		public string? TenHienThi { get; set; }
		public string? FullName { get; set; }
		public bool? IsActiveAccount { get; set; }
	}
}


