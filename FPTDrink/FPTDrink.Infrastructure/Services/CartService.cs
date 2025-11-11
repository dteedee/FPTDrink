using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class CartService : ICartService
	{
		private readonly ICartStore _store;
		private readonly IProductRepository _productRepo;

		public CartService(ICartStore store, IProductRepository productRepo)
		{
			_store = store;
			_productRepo = productRepo;
		}

		private static string EnsureCartId(string? cartId) => string.IsNullOrWhiteSpace(cartId) ? Guid.NewGuid().ToString("N") : cartId!;

		public CartDto GetCart(string? cartId)
		{
			var id = EnsureCartId(cartId);
			return _store.Get(id);
		}

		public CartDto Add(string? cartId, string productId, int quantity)
		{
			if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
			var id = EnsureCartId(cartId);
			var cart = _store.Get(id);
			var product = _productRepo.Query().Include(p => p.ProductCategory).FirstOrDefault(p => p.MaSanPham == productId);
			if (product == null) throw new InvalidOperationException("Sản phẩm không tồn tại.");
			if (product.SoLuong < quantity) throw new InvalidOperationException($"Không đủ tồn kho. Còn lại: {product.SoLuong}");
			var existing = cart.Items.FirstOrDefault(x => x.ProductId == productId);
			if (existing != null)
			{
				existing.Quantity += quantity;
				existing.TotalPrice = existing.Quantity * existing.Price;
				existing.TonKho = product.SoLuong;
			}
			else
			{
				cart.Items.Add(new CartItemDto
				{
					ProductId = product.MaSanPham,
					ProductName = product.Title,
					ProductImg = product.Image,
					Quantity = quantity,
					Price = product.GiaBan ?? product.GiaNiemYet,
					TotalPrice = quantity * (product.GiaBan ?? product.GiaNiemYet),
					TonKho = product.SoLuong
				});
			}
			_store.Save(cart);
			return cart;
		}

		public CartDto Update(string cartId, string productId, int quantity)
		{
			if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
			var cart = _store.Get(cartId);
			var item = cart.Items.FirstOrDefault(x => x.ProductId == productId) ?? throw new InvalidOperationException("Không tìm thấy sản phẩm trong giỏ.");
			var product = _productRepo.Query().FirstOrDefault(p => p.MaSanPham == productId) ?? throw new InvalidOperationException("Sản phẩm không tồn tại.");
			item.Quantity = quantity;
			item.TotalPrice = item.Quantity * item.Price;
			item.TonKho = product.SoLuong;
			_store.Save(cart);
			return cart;
		}

		public CartDto Delete(string cartId, string productId)
		{
			var cart = _store.Get(cartId);
			cart.Items.RemoveAll(x => x.ProductId == productId);
			_store.Save(cart);
			return cart;
		}

		public CartDto Clear(string cartId)
		{
			var cart = _store.Get(cartId);
			cart.Items.Clear();
			_store.Save(cart);
			return cart;
		}
	}
}

