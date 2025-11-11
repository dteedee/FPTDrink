using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Order
{
	public class UpdateStatusRequest
	{
		[Range(2, 3, ErrorMessage = "Trạng thái chỉ cho phép 2 hoặc 3")]
		public int TrangThai { get; set; }
		public bool Confirmed { get; set; } = false;
	}
}


