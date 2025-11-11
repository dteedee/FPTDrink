namespace FPTDrink.API.DTOs.Admin.Category
{
	public class CategoryUpdateRequest
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public int? Position { get; set; }
		public bool IsActive { get; set; } = true;
		public string? SeoTitle { get; set; }
		public string? SeoDescription { get; set; }
		public string? SeoKeywords { get; set; }
		public string? Modifiedby { get; set; }
	}
}


