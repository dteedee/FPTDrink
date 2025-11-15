using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class CartMergeService : ICartMergeService
	{
		private readonly IGioHangRepository _gioHangRepository;
		private readonly IGioHangTamRepository _gioHangTamRepository;
		private readonly IProductRepository _productRepository;

		public CartMergeService(
			IGioHangRepository gioHangRepository,
			IGioHangTamRepository gioHangTamRepository,
			IProductRepository productRepository)
		{
			_gioHangRepository = gioHangRepository;
			_gioHangTamRepository = gioHangTamRepository;
			_productRepository = productRepository;
		}

		public async Task<IReadOnlyList<GioHang>> GetCustomerCartAsync(string customerId, CancellationToken cancellationToken = default)
		{
			return await _gioHangRepository.Query()
				.Where(x => x.IdKhachHang == customerId)
				.Include(x => x.MaSanPhamNavigation)
				.ToListAsync(cancellationToken);
		}

		public async Task<(bool success, string? error)> AddItemAsync(string customerId, string productId, int quantity, CancellationToken cancellationToken = default)
		{
			if (quantity <= 0) return (false, "Số lượng phải lớn hơn 0.");
			var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
			if (product == null) return (false, "Sản phẩm không tồn tại.");

			var existing = await _gioHangRepository.FindByCustomerAndProductAsync(customerId, productId, cancellationToken);
			var newQuantity = quantity + (existing?.SoLuong ?? 0);
			if (newQuantity > product.SoLuong) return (false, $"Không đủ tồn kho. Còn lại: {product.SoLuong}");

			if (existing == null)
			{
				var entity = new GioHang
				{
					IdKhachHang = customerId,
					MaSanPham = productId,
					SoLuong = quantity,
					CreatedDate = DateTime.UtcNow
				};
				await _gioHangRepository.AddAsync(entity, cancellationToken);
			}
			else
			{
				existing.SoLuong = newQuantity;
				existing.ModifiedDate = DateTime.UtcNow;
				_gioHangRepository.Update(existing);
			}

			await _gioHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error)> UpdateItemQuantityAsync(string customerId, string productId, int quantity, CancellationToken cancellationToken = default)
		{
			if (quantity < 0) return (false, "Số lượng không hợp lệ.");
			var existing = await _gioHangRepository.FindByCustomerAndProductAsync(customerId, productId, cancellationToken);
			if (existing == null) return (false, "Sản phẩm không có trong giỏ hàng.");

			if (quantity == 0)
			{
				_gioHangRepository.Remove(existing);
				await _gioHangRepository.SaveChangesAsync(cancellationToken);
				return (true, null);
			}

			var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
			if (product == null) return (false, "Sản phẩm không tồn tại.");
			if (quantity > product.SoLuong) return (false, $"Không đủ tồn kho. Còn lại: {product.SoLuong}");

			existing.SoLuong = quantity;
			existing.ModifiedDate = DateTime.UtcNow;
			_gioHangRepository.Update(existing);
			await _gioHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error)> RemoveItemAsync(string customerId, string productId, CancellationToken cancellationToken = default)
		{
			var existing = await _gioHangRepository.FindByCustomerAndProductAsync(customerId, productId, cancellationToken);
			if (existing == null) return (false, "Sản phẩm không tồn tại trong giỏ hàng.");
			_gioHangRepository.Remove(existing);
			await _gioHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error)> ClearAsync(string customerId, CancellationToken cancellationToken = default)
		{
			await _gioHangRepository.RemoveByCustomerIdAsync(customerId, cancellationToken);
			await _gioHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error)> MergeGuestCartAsync(string customerId, string guestCartId, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(guestCartId)) return (true, null);

			var tempItems = await _gioHangTamRepository.GetByCartIdAsync(guestCartId, cancellationToken);
			if (tempItems.Count == 0) return (true, null);

			foreach (var item in tempItems)
			{
				var product = await _productRepository.GetByIdAsync(item.MaSanPham, cancellationToken);
				if (product == null) continue;
				var existing = await _gioHangRepository.FindByCustomerAndProductAsync(customerId, item.MaSanPham, cancellationToken);
				var targetQuantity = item.SoLuong + (existing?.SoLuong ?? 0);
				if (targetQuantity > product.SoLuong)
				{
					targetQuantity = product.SoLuong;
				}

				if (existing == null)
				{
					var entity = new GioHang
					{
						IdKhachHang = customerId,
						MaSanPham = item.MaSanPham,
						SoLuong = targetQuantity,
						CreatedDate = DateTime.UtcNow
					};
					await _gioHangRepository.AddAsync(entity, cancellationToken);
				}
				else
				{
					existing.SoLuong = targetQuantity;
					existing.ModifiedDate = DateTime.UtcNow;
					_gioHangRepository.Update(existing);
				}
			}

			await _gioHangTamRepository.ClearByCartIdAsync(guestCartId, cancellationToken);
			await _gioHangRepository.SaveChangesAsync(cancellationToken);
			await _gioHangTamRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}
	}
}

