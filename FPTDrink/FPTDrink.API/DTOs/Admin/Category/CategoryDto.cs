namespace FPTDrink.API.DTOs.Admin.Category
{
	public class CategoryDto
	{
		public int Id { get; set; }
		public string? Title { get; set; }
		public string? Alias { get; set; }
		public int? Position { get; set; }
		public bool? IsActive { get; set; }
		public int? Status { get; set; }
		public string? SeoTitle { get; set; }
		public string? SeoDescription { get; set; }
		public string? SeoKeywords { get; set; }
	}
}


