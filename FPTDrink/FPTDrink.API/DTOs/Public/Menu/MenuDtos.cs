namespace FPTDrink.API.DTOs.Public.Menu
{
	public class MenuCategoryDto
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
		public int? Position { get; set; }
	}

	public class MenuProductCategoryDto
	{
		public string MaLoaiSanPham { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
	}

	public class MenuSupplierDto
	{
		public string MaNhaCungCap { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
	}
}


