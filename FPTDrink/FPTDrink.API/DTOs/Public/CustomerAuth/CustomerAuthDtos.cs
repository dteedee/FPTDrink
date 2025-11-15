using System;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.CustomerAuth
{
	public class RegisterRequestDto
	{
		[Required, StringLength(50, MinimumLength = 4)]
		public string TenDangNhap { get; set; } = string.Empty;

		[Required, StringLength(255, MinimumLength = 6)]
		public string MatKhau { get; set; } = string.Empty;

		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, StringLength(20)]
		public string SoDienThoai { get; set; } = string.Empty;

		[Required, StringLength(150)]
		public string HoTen { get; set; } = string.Empty;
	}

	public class LoginRequestDto
	{
		[Required]
		public string UsernameOrEmail { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}

	public class VerifyOtpRequestDto
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, StringLength(6, MinimumLength = 6)]
		public string Otp { get; set; } = string.Empty;
	}

	public class ResendOtpRequestDto
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;
	}

	public class ChangeCustomerPasswordRequestDto
	{
		[Required]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required, MinLength(6)]
		public string NewPassword { get; set; } = string.Empty;

		[Required, Compare(nameof(NewPassword))]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}

	public class ResetPasswordRequestDto
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, StringLength(6, MinimumLength = 6)]
		public string Otp { get; set; } = string.Empty;

		[Required, MinLength(6)]
		public string NewPassword { get; set; } = string.Empty;

		[Required, Compare(nameof(NewPassword))]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}

	public class CustomerAuthResponseDto
	{
		public string Token { get; set; } = string.Empty;
		public CustomerProfileDto Customer { get; set; } = new();
	}

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

