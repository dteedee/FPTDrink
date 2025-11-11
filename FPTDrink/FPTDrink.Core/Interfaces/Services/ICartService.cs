namespace FPTDrink.Core.Interfaces.Services
{
	public class CartItemDto
	{
		public string ProductId { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string? ProductImg { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal TotalPrice { get; set; }
		public int TonKho { get; set; }
	}

	public class CartDto
	{
		public string CartId { get; set; } = string.Empty;
		public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
	}

	public interface ICartStore
	{
		CartDto Get(string cartId);
		void Save(CartDto cart);
		void Remove(string cartId);
	}

	public interface ICartService
	{
		CartDto GetCart(string? cartId);
		CartDto Add(string? cartId, string productId, int quantity);
		CartDto Update(string cartId, string productId, int quantity);
		CartDto Delete(string cartId, string productId);
		CartDto Clear(string cartId);
	}
}

