using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Orders
{
	public class OrderCodeQuery
	{
		[Required, StringLength(50)]
		public string MaHoaDon { get; set; } = string.Empty;
	}

	public class OrderNameCccdQuery
	{
		[Required, StringLength(500)] public string TenKhachHang { get; set; } = string.Empty;
		[Required, StringLength(50)] public string CCCD { get; set; } = string.Empty;
	}
}


