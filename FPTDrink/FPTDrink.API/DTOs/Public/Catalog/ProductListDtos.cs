using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Catalog
{
	public class ProductListQuery
	{
		[Range(1, int.MaxValue)] public int Page { get; set; } = 1;
		[Range(1, 200)] public int PageSize { get; set; } = 20;
		public string? CategoryId { get; set; }
		public string? SupplierId { get; set; }
		[StringLength(200)] public string? Q { get; set; }
		[Range(0, double.MaxValue)] public decimal? PriceFrom { get; set; }
		[Range(0, double.MaxValue)] public decimal? PriceTo { get; set; }
		[StringLength(20)] public string? Sort { get; set; }
		public string? CartId { get; set; }
	}

	public class ProductListItemDto
	{
		public string MaSanPham { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Image { get; set; }
		public decimal GiaNiemYet { get; set; }
		public decimal GiaHienThi { get; set; }
		public decimal? GiamGia { get; set; }
		public bool IsSale { get; set; }
		public string? ProductCategoryTitle { get; set; }
		public string? SupplierTitle { get; set; }
		public int SoLuong { get; set; }
	}

	public class PagedResultDto<T>
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int Total { get; set; }
		public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
	}
}


