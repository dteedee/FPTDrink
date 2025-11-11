using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Products
{
	public class ProductDto
	{
		public string MaSanPham { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
		public string ProductCategoryId { get; set; } = string.Empty;
		public string SupplierId { get; set; } = string.Empty;
		public string? ProductCategoryTitle { get; set; }
		public string? SupplierTitle { get; set; }
		public decimal GiaNhap { get; set; }
		public decimal GiaNiemYet { get; set; }
		public decimal? GiaBan { get; set; }
		public decimal? GiamGia { get; set; }
		public int SoLuong { get; set; }
		public bool IsActive { get; set; }
		public int Status { get; set; }
	}

	public class ProductCreateRequest
	{
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		[Required] public string ProductCategoryId { get; set; } = string.Empty;
		[Required] public string SupplierId { get; set; } = string.Empty;
		[Range(0, double.MaxValue)] public decimal GiaNhap { get; set; }
		[Range(0, double.MaxValue)] public decimal GiaNiemYet { get; set; }
		[Range(0, 100)] public decimal? GiamGia { get; set; }
		[Range(0, int.MaxValue)] public int SoLuong { get; set; }
	}

	public class ProductUpdateRequest
	{
		[Required] public string MaSanPham { get; set; } = string.Empty;
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		[Required] public string ProductCategoryId { get; set; } = string.Empty;
		[Required] public string SupplierId { get; set; } = string.Empty;
		[Range(0, double.MaxValue)] public decimal GiaNhap { get; set; }
		[Range(0, double.MaxValue)] public decimal GiaNiemYet { get; set; }
		[Range(0, 100)] public decimal? GiamGia { get; set; }
		[Range(0, int.MaxValue)] public int SoLuong { get; set; }
		public string? Modifiedby { get; set; }
	}
}


