namespace FPTDrink.API.DTOs.Admin.ProductCategory
{
	public class ProductCategoryDto
	{
		public string MaLoaiSanPham { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public int Status { get; set; }
		public string? SeoTitle { get; set; }
		public string? SeoDescription { get; set; }
		public string? SeoKeywords { get; set; }
	}
}


