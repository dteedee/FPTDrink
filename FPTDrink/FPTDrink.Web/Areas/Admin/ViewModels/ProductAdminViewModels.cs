using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminProductListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? CategoryName { get; set; }
		public string? SupplierName { get; set; }
		public decimal CostPrice { get; set; }
		public decimal ListPrice { get; set; }
		public decimal SalePrice { get; set; }
		public decimal? DiscountPercent { get; set; }
		public int Stock { get; set; }
		public bool IsActive { get; set; }
		public int Status { get; set; }
		public string? Image { get; set; }
	}

	public class AdminProductFormViewModel
	{
		public string? Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
		[Display(Name = "Tên sản phẩm")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
		[Display(Name = "Loại sản phẩm")]
		public string ProductCategoryId { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
		[Display(Name = "Nhà cung cấp")]
		public string SupplierId { get; set; } = string.Empty;

		[Display(Name = "Mô tả ngắn")]
		public string? Description { get; set; }

		[Display(Name = "Chi tiết sản phẩm")]
		public string? Detail { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập giá nhập")]
		[Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
		[Display(Name = "Giá nhập")]
		public decimal CostPrice { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập giá niêm yết")]
		[Range(0, double.MaxValue, ErrorMessage = "Giá niêm yết phải lớn hơn hoặc bằng 0")]
		[Display(Name = "Giá niêm yết")]
		public decimal ListPrice { get; set; }

		[Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100%")]
		[Display(Name = "% giảm giá")]
		public decimal? DiscountPercent { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
		[Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
		[Display(Name = "Số lượng tồn")]
		public int Stock { get; set; }

		[Display(Name = "Hiển thị trên trang chủ")]
		public bool IsHome { get; set; }

		[Display(Name = "Sản phẩm mới")]
		public bool IsNew { get; set; }

		[Display(Name = "Sản phẩm nổi bật")]
		public bool IsHot { get; set; }

		[Display(Name = "Hoạt động")]
		public bool IsActive { get; set; } = true;

		[Display(Name = "URL hình ảnh hiện tại")]
		public string? ExistingImage { get; set; }

		[Display(Name = "Upload hình ảnh")]
		public IFormFile? UploadImage { get; set; }

		public IEnumerable<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> SupplierOptions { get; set; } = new List<SelectListItem>();
	}
}

