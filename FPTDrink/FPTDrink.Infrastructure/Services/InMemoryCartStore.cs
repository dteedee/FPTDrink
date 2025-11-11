using System.Collections.Concurrent;
using FPTDrink.Core.Interfaces.Services;

namespace FPTDrink.Infrastructure.Services
{
	public class InMemoryCartStore : ICartStore
	{
		private readonly ConcurrentDictionary<string, (CartDto Cart, DateTime LastUpdated)> _store = new();
		private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

		public CartDto Get(string cartId)
		{
			CleanupExpired();
			if (!_store.TryGetValue(cartId, out var entry) || IsExpired(entry.LastUpdated))
			{
				var newCart = new CartDto { CartId = cartId };
				_store[cartId] = (newCart, DateTime.UtcNow);
				return newCart;
			}
			_store[cartId] = (entry.Cart, DateTime.UtcNow);
			return entry.Cart;
		}

		public void Save(CartDto cart)
		{
			_store[cart.CartId] = (cart, DateTime.UtcNow);
		}

		public void Remove(string cartId)
		{
			_store.TryRemove(cartId, out _);
		}

		private static bool IsExpired(DateTime lastUpdated) => (DateTime.UtcNow - lastUpdated) > Ttl;

		private void CleanupExpired()
		{
			foreach (var kv in _store.ToArray())
			{
				if (IsExpired(kv.Value.LastUpdated))
				{
					_store.TryRemove(kv.Key, out _);
				}
			}
		}
	}
}


