using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminStaffListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public int Status { get; set; }
	}

	public class AdminStaffFormViewModel
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
		public DateTime? BirthDate { get; set; }

		[Display(Name = "Giới tính")]
		public bool Gender { get; set; }

		[Display(Name = "Chức vụ")]
		[Required(ErrorMessage = "Vui lòng chọn chức vụ")]
		public int? RoleId { get; set; }

		[Display(Name = "Hoạt động")]
		public bool IsActive { get; set; } = true;

		[Display(Name = "Mật khẩu (đối với tài khoản mới)")]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		public IEnumerable<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
	}

	public class AdminRoleViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int Status { get; set; }
	}

	public class AdminRoleFormViewModel
	{
		public int? Id { get; set; }

		[Display(Name = "Tên chức vụ")]
		[Required(ErrorMessage = "Vui lòng nhập tên chức vụ")]
		[StringLength(100, ErrorMessage = "Tên chức vụ tối đa 100 ký tự")]
		public string Title { get; set; } = string.Empty;

		[Display(Name = "Mô tả")]
		[StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
		public string? Description { get; set; }

		[Display(Name = "Kích hoạt")]
		public bool IsActive { get; set; } = true;
	}
}

