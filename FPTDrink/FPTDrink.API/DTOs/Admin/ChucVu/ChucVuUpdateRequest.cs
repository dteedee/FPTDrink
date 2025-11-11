using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.ChucVu
{
	public class ChucVuUpdateRequest
	{
		public int Id { get; set; }
		[Required, StringLength(500)]
		public string TenChucVu { get; set; } = string.Empty;
		[StringLength(500)]
		public string? MoTa { get; set; }
		public string? Modifiedby { get; set; }
	}
}


