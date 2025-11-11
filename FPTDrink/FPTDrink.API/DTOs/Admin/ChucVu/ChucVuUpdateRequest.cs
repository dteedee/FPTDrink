namespace FPTDrink.API.DTOs.Admin.ChucVu
{
	public class ChucVuUpdateRequest
	{
		public int Id { get; set; }
		public string TenChucVu { get; set; } = string.Empty;
		public string? MoTa { get; set; }
		public string? Modifiedby { get; set; }
	}
}


