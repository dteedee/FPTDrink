using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class SearchService : ISearchService
	{
		private readonly IProductRepository _productRepo;

		public SearchService(IProductRepository productRepo)
		{
			_productRepo = productRepo;
		}

		public async Task<IReadOnlyList<string>> SuggestAsync(string q, int limit, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(q)) return Array.Empty<string>();
			q = q.ToLowerInvariant();
			var results = await _productRepo.Query()
				.Where(x => x.IsActive && x.Status != 0 && x.Title.ToLower().Contains(q))
				.OrderBy(x => x.Title)
				.Select(x => x.Title)
				.Distinct()
				.Take(limit)
				.ToListAsync(cancellationToken);
			return results;
		}

		public async Task<IReadOnlyList<Product>> SearchAsync(string q, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(q)) return Array.Empty<Product>();
			q = q.ToLowerInvariant();
			var items = await _productRepo.Query()
				.Include(p => p.ProductCategory)
				.Where(x => x.IsActive && x.Status != 0 && x.ProductCategory != null && x.ProductCategory.IsActive && x.ProductCategory.Status != 0)
				.Where(p => p.Title.ToLower().Contains(q))
				.ToListAsync(cancellationToken);
			return items;
		}
	}
}


