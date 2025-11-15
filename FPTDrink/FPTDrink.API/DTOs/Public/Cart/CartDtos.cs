using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Cart
{
	public class AddToCartRequest
	{
		[Required] public string ProductId { get; set; } = string.Empty;
		[Range(1, int.MaxValue)] public int Quantity { get; set; }
		public string? CartId { get; set; }
	}

	public class UpdateCartItemRequest
	{
		[Required] public string ProductId { get; set; } = string.Empty;
		[Range(0, int.MaxValue)] public int Quantity { get; set; }
		public string CartId { get; set; } = string.Empty;
	}

	public class MergeCartRequestDto
	{
		[Required]
		public string GuestCartId { get; set; } = string.Empty;
	}
}


