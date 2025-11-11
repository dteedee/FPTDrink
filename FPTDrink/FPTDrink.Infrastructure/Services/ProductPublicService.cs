using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class ProductPublicService : IProductPublicService
	{
		private readonly IProductRepository _productRepo;

		public ProductPublicService(IProductRepository productRepo)
		{
			_productRepo = productRepo;
		}

		public Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _productRepo.Query()
				.Where(p => p.Status != 0 && p.IsActive)
				.Include(p => p.ProductCategory)
				.Include(p => p.Supplier)
				.FirstOrDefaultAsync(p => p.MaSanPham == id, cancellationToken);
		}

		public async Task<IReadOnlyList<Product>> GetRelatedAsync(string id, int limit, CancellationToken cancellationToken = default)
		{
			var current = await GetByIdAsync(id, cancellationToken);
			if (current == null) return Array.Empty<Product>();
			var q = _productRepo.Query()
				.Where(p => p.Status != 0 && p.IsActive && p.MaSanPham != id && p.ProductCategoryId == current.ProductCategoryId)
				.OrderByDescending(p => p.ViewCount)
				.Take(limit)
				.Include(p => p.ProductCategory);
			return await q.ToListAsync(cancellationToken);
		}
	}
}


