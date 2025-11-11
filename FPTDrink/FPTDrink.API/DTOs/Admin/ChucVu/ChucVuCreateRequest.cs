using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.ChucVu
{
	public class ChucVuCreateRequest
	{
		[Required, StringLength(500)]
		public string TenChucVu { get; set; } = string.Empty;
		[StringLength(500)]
		public string? MoTa { get; set; }
	}
}


