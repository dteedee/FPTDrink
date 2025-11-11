using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.ProductCategory
{
	public class ProductCategoryUpdateRequest
	{
		[Required]
		public string MaLoaiSanPham { get; set; } = string.Empty;
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		[StringLength(500)] public string? MoTa { get; set; }
		[StringLength(150)] public string? SeoTitle { get; set; }
		[StringLength(250)] public string? SeoDescription { get; set; }
		[StringLength(150)] public string? SeoKeywords { get; set; }
		public string? Modifiedby { get; set; }
	}
}


