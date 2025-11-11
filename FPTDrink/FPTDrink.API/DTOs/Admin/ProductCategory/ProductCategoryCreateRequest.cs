using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.ProductCategory
{
	public class ProductCategoryCreateRequest
	{
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		[StringLength(500)] public string? MoTa { get; set; }
		[StringLength(150)] public string? SeoTitle { get; set; }
		[StringLength(250)] public string? SeoDescription { get; set; }
		[StringLength(150)] public string? SeoKeywords { get; set; }
	}
}


