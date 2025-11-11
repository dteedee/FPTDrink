using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Checkout
{
	public class CreateOrderItemRequestDto
	{
		[Required] public string ProductId { get; set; } = string.Empty;
		[Range(1, int.MaxValue)] public int Quantity { get; set; }
	}

	public class CreateOrderRequestDto
	{
		[Required, StringLength(500)] public string TenKhachHang { get; set; } = string.Empty;
		[Required, StringLength(20)] public string SoDienThoai { get; set; } = string.Empty;
		[Required, StringLength(500)] public string DiaChi { get; set; } = string.Empty;
		[EmailAddress] public string? Email { get; set; }
		[Required, StringLength(50)] public string CCCD { get; set; } = string.Empty;
		[Range(1, 3)] public int TypePayment { get; set; }
		[MinLength(1)] public List<CreateOrderItemRequestDto> Items { get; set; } = new();
	}
}


