using System.Collections.Concurrent;
using FPTDrink.Core.Interfaces.Services;

namespace FPTDrink.Infrastructure.Services
{
	public class InMemoryCartStore : ICartStore
	{
		private readonly ConcurrentDictionary<string, CartDto> _store = new();

		public CartDto Get(string cartId)
		{
			if (!_store.TryGetValue(cartId, out var cart))
			{
				cart = new CartDto { CartId = cartId };
				_store[cartId] = cart;
			}
			return cart;
		}

		public void Save(CartDto cart)
		{
			_store[cart.CartId] = cart;
		}

		public void Remove(string cartId)
		{
			_store.TryRemove(cartId, out _);
		}
	}
}


