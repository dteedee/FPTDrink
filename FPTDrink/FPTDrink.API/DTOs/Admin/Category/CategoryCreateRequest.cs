namespace FPTDrink.API.DTOs.Admin.Category
{
	public class CategoryCreateRequest
	{
		public string Title { get; set; } = string.Empty;
		public int? Position { get; set; }
		public bool IsActive { get; set; } = true;
		public string? SeoTitle { get; set; }
		public string? SeoDescription { get; set; }
		public string? SeoKeywords { get; set; }
	}
}


