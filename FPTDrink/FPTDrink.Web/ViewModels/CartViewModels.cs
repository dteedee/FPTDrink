using System.Collections.Generic;

namespace FPTDrink.Web.ViewModels
{
	public class CartItemViewModel
	{
		public string ProductId { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string? ProductImg { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal TotalPrice { get; set; }
		public int TonKho { get; set; }
	}

	public class CartViewModel
	{
		public string CartId { get; set; } = string.Empty;
		public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
	}
}

