namespace FPTDrink.API.DTOs.Public.Products
{
	public class ProductDetailDto
	{
		public string MaSanPham { get; set; } = string.Empty;
	 public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
		public string? Image { get; set; }
		public string? MoTa { get; set; }
		public string? ChiTiet { get; set; }
		public decimal GiaNhap { get; set; }
		public decimal GiaNiemYet { get; set; }
		public decimal? GiaBan { get; set; }
		public decimal? GiamGia { get; set; }
		public int SoLuong { get; set; }
		public bool IsSale { get; set; }
		public string? ProductCategoryId { get; set; }
		public string? ProductCategoryTitle { get; set; }
		public string? SupplierId { get; set; }
		public string? SupplierTitle { get; set; }
	}
}


