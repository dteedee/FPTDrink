using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Category
{
	public class CategoryCreateRequest
	{
		[Required, StringLength(150)]
		public string Title { get; set; } = string.Empty;
		[Range(1, int.MaxValue, ErrorMessage = "Position phải >= 1")]
		public int? Position { get; set; }
		public bool IsActive { get; set; } = true;
		[StringLength(150)] public string? SeoTitle { get; set; }
		[StringLength(250)] public string? SeoDescription { get; set; }
		[StringLength(150)] public string? SeoKeywords { get; set; }
	}
}


