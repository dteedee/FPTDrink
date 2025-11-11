using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class CatalogQueryService : ICatalogQueryService
	{
		private readonly IProductRepository _productRepo;
		private readonly IProductCategoryRepository _categoryRepo;
		private readonly ICartService _cartService;

		public CatalogQueryService(IProductRepository productRepo, IProductCategoryRepository categoryRepo, ICartService cartService)
		{
			_productRepo = productRepo;
			_categoryRepo = categoryRepo;
			_cartService = cartService;
		}

		public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
		{
			// Category entity is in Core.Models.Category; use DbContext via ProductCategory to reach? Better add Category via repository later.
			// For now, infer active categories via ProductCategory.Category navigation
			var cats = await _categoryRepo.Query()
				.Where(pc => pc.Status != 0 && pc.IsActive)
				.Select(pc => pc.Category!)
				.Where(c => c != null)
				.Distinct()
				.ToListAsync(cancellationToken);
			return cats!;
		}

		public async Task<(IReadOnlyList<Product> items, int total)> GetProductsAsync(int page, int pageSize, string? categoryId, string? supplierId, string? q, decimal? priceFrom, decimal? priceTo, string? sort, string? cartId, CancellationToken cancellationToken = default)
		{
			IQueryable<Product> qy = _productRepo.Query()
				.Where(p => p.Status != 0 && p.IsActive)
				.Include(p => p.ProductCategory)
				.Include(p => p.Supplier);

			if (!string.IsNullOrWhiteSpace(categoryId))
			{
				qy = qy.Where(p => p.ProductCategoryId == categoryId);
			}
			if (!string.IsNullOrWhiteSpace(supplierId))
			{
				qy = qy.Where(p => p.SupplierId == supplierId);
			}
			if (!string.IsNullOrWhiteSpace(q))
			{
				qy = qy.Where(p => p.Title.Contains(q) || (p.ProductCategory != null && p.ProductCategory.Title.Contains(q)) || (p.Supplier != null && p.Supplier.Title.Contains(q)));
			}
			if (priceFrom.HasValue) qy = qy.Where(p => (p.GiaBan ?? p.GiaNiemYet) >= priceFrom.Value);
			if (priceTo.HasValue) qy = qy.Where(p => (p.GiaBan ?? p.GiaNiemYet) <= priceTo.Value);

			qy = sort switch
			{
				"price_asc" => qy.OrderBy(p => (p.GiaBan ?? p.GiaNiemYet)),
				"price_desc" => qy.OrderByDescending(p => (p.GiaBan ?? p.GiaNiemYet)),
				"newest" => qy.OrderByDescending(p => p.CreatedDate),
				"popular" => qy.OrderByDescending(p => p.ViewCount),
				_ => qy.OrderBy(p => p.Title)
			};

			int total = await qy.CountAsync(cancellationToken);
			var items = await qy.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
			
			if (!string.IsNullOrWhiteSpace(cartId))
			{
				var cart = _cartService.GetCart(cartId);
				var cartQuantities = cart.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
				
				foreach (var product in items)
				{
					if (cartQuantities.TryGetValue(product.MaSanPham, out var cartQuantity))
					{
						product.SoLuong = Math.Max(0, product.SoLuong - cartQuantity);
					}
				}
			}
			
			return (items, total);
		}

		public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
		{
			var items = await _productRepo.Query()
				.Where(p => p.Status != 0 && p.IsActive && p.ProductCategory != null && p.ProductCategory.CategoryId == categoryId)
				.Include(p => p.ProductCategory)
				.ToListAsync(cancellationToken);
			return items;
		}
	}
}


