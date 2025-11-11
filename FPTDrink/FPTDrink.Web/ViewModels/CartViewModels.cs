using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
	public class CartItemViewModel
	{
		[JsonPropertyName("productId")]
		public string ProductId { get; set; } = string.Empty;

		[JsonPropertyName("productName")]
		public string ProductName { get; set; } = string.Empty;

		[JsonPropertyName("productImg")]
		public string? ProductImg { get; set; }

		[JsonPropertyName("quantity")]
		public int Quantity { get; set; }

		[JsonPropertyName("price")]
		public decimal Price { get; set; }

		[JsonPropertyName("totalPrice")]
		public decimal TotalPrice { get; set; }

		[JsonPropertyName("tonKho")]
		public int TonKho { get; set; }
	}

	public class CartViewModel
	{
		public string CartId { get; set; } = string.Empty;
		public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
	}
}

