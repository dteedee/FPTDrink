using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Suppliers
{
	public class SupplierDto
	{
		public string MaNhaCungCap { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Alias { get; set; } = string.Empty;
		public string? SoDienThoai { get; set; }
		public string? Email { get; set; }
		public int Status { get; set; }
	}

	public class SupplierCreateRequest
	{
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		[Phone] public string? SoDienThoai { get; set; }
		[EmailAddress] public string? Email { get; set; }
		[StringLength(500)] public string? MoTa { get; set; }
	}

	public class SupplierUpdateRequest
	{
		[Required] public string MaNhaCungCap { get; set; } = string.Empty;
		[Required, StringLength(500)]
		public string Title { get; set; } = string.Empty;
		[Phone] public string? SoDienThoai { get; set; }
		[EmailAddress] public string? Email { get; set; }
		[StringLength(500)] public string? MoTa { get; set; }
		public string? Modifiedby { get; set; }
	}
}


