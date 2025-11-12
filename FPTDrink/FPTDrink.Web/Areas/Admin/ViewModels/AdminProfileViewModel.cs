using System;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminProfileViewModel
	{
		public string? Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
		[Display(Name = "Tên đăng nhập")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[Display(Name = "Họ tên đầy đủ")]
		public string FullName { get; set; } = string.Empty;

		[Display(Name = "Tên hiển thị")]
		public string DisplayName { get; set; } = string.Empty;

		[Display(Name = "Số điện thoại")]
		public string Phone { get; set; } = string.Empty;

		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = string.Empty;

		[Display(Name = "Địa chỉ")]
		public string Address { get; set; } = string.Empty;

		[Display(Name = "Ngày sinh")]
		[DataType(DataType.Date)]
		public DateTime? BirthDate { get; set; }

		[Display(Name = "Giới tính")]
		public bool Gender { get; set; }

		[Display(Name = "Chức vụ")]
		public string RoleName { get; set; } = string.Empty;

		[Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string? NewPassword { get; set; }

		[Display(Name = "Xác nhận mật khẩu mới")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
		public string? ConfirmPassword { get; set; }
	}
}

