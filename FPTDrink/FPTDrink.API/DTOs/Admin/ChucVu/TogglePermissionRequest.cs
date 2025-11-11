using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.ChucVu
{
	public class TogglePermissionRequest
	{
		[Required, StringLength(50)]
		public string MaChucNang { get; set; } = string.Empty;
	}
}


