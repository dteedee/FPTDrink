using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminSupplierListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? Image { get; set; }
		public bool IsActive { get; set; }
		public int Status { get; set; }
	}

	public class AdminSupplierFormViewModel
	{
		public string? Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
		[Display(Name = "Tên nhà cung cấp")]
		public string Title { get; set; } = string.Empty;

		[Display(Name = "Số điện thoại")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string Phone { get; set; } = string.Empty;

		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = string.Empty;

		[Display(Name = "Hoạt động")]
		public bool IsActive { get; set; } = true;
	}
}

