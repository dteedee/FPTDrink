using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.ViewModels
{
	public class CustomerRegisterViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
		[StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập phải từ 4 đến 50 ký tự")]
		[Display(Name = "Tên đăng nhập")]
		public string TenDangNhap { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string MatKhau { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
		[DataType(DataType.Password)]
		[Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp")]
		[Display(Name = "Xác nhận mật khẩu")]
		public string ConfirmMatKhau { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		[StringLength(20, ErrorMessage = "Số điện thoại không hợp lệ")]
		[Display(Name = "Số điện thoại")]
		public string SoDienThoai { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự")]
		[Display(Name = "Họ tên")]
		public string HoTen { get; set; } = string.Empty;
	}

	public class CustomerLoginViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
		[Display(Name = "Tên đăng nhập hoặc Email")]
		public string UsernameOrEmail { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Ghi nhớ đăng nhập")]
		public bool RememberMe { get; set; }

		public string? ReturnUrl { get; set; }
	}

	public class VerifyOtpViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mã OTP")]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
		[Display(Name = "Mã OTP")]
		public string Otp { get; set; } = string.Empty;
	}

	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;
	}

	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mã OTP")]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
		[Display(Name = "Mã OTP")]
		public string Otp { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu mới")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
		[Display(Name = "Xác nhận mật khẩu mới")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}

