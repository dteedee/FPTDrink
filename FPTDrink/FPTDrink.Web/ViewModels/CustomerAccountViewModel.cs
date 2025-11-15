using System;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.ViewModels
{
	public class CustomerAccountViewModel
	{
		public CustomerProfileDto Profile { get; set; } = new();
	}

	public class UpdateProfileViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự")]
		[Display(Name = "Họ tên")]
		public string HoTen { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		[StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[Display(Name = "Số điện thoại")]
		public string SoDienThoai { get; set; } = string.Empty;

		[Display(Name = "Ngày sinh")]
		[CustomDateValidation(ErrorMessage = "Ngày sinh không hợp lệ. Bạn phải trên 15 tuổi và ngày sinh không được ở tương lai")]
		public DateTime? NgaySinh { get; set; }

		[Display(Name = "Giới tính")]
		public bool? GioiTinh { get; set; }

		[StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
		[Display(Name = "Địa chỉ")]
		public string? DiaChi { get; set; }
	}

	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu hiện tại")]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có từ 6 đến 255 ký tự")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu mới")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới")]
		[Display(Name = "Xác nhận mật khẩu mới")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}

	public class CustomDateValidationAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}

			if (value is not DateTime dateValue)
			{
				return new ValidationResult("Ngày sinh không hợp lệ");
			}

			var today = DateTime.Today;
			var minDate = today.AddYears(-100);
			var maxDate = today.AddYears(-15);

			if (dateValue > today)
			{
				return new ValidationResult("Ngày sinh không được ở tương lai");
			}

			if (dateValue > maxDate)
			{
				return new ValidationResult("Bạn phải trên 15 tuổi để sử dụng dịch vụ");
			}

			if (dateValue < minDate)
			{
				return new ValidationResult("Ngày sinh không hợp lệ");
			}

			return ValidationResult.Success;
		}
	}
}

