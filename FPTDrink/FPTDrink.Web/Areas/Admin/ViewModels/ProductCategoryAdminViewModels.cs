using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminProductCategoryListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		public int Status { get; set; }
		public string? SeoTitle { get; set; }
		public string? SeoKeywords { get; set; }
	}

	public class AdminProductCategoryFormViewModel
	{
		public string? Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên loại sản phẩm")]
		[Display(Name = "Tên loại sản phẩm")]
		public string Title { get; set; } = string.Empty;

		[Display(Name = "Mô tả")]
		public string? Description { get; set; }

		[Display(Name = "Trạng thái")]
		public bool IsActive { get; set; } = true;

		[Display(Name = "SEO Title")]
		public string? SeoTitle { get; set; }

		[Display(Name = "SEO Keywords")]
		public string? SeoKeywords { get; set; }

		[Display(Name = "SEO Description")]
		public string? SeoDescription { get; set; }
	}
}

