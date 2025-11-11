using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class HomePageService : IHomePageService
	{
		private readonly IProductRepository _productRepo;

		public HomePageService(IProductRepository productRepo)
		{
			_productRepo = productRepo;
		}

		public async Task<HomeBlocks> GetHomeBlocksAsync(int limit, CancellationToken cancellationToken = default)
		{
			IQueryable<Product> baseQ = _productRepo.Query()
				.Where(p => p.Status != 0 && p.IsActive)
				.Include(p => p.ProductCategory)
				.Include(p => p.Supplier);

			var featured = await baseQ.Where(p => p.IsHome).OrderByDescending(p => p.CreatedDate).Take(limit).ToListAsync(cancellationToken);
			var hot = await baseQ.Where(p => p.IsHot).OrderByDescending(p => p.ViewCount).Take(limit).ToListAsync(cancellationToken);
			var newest = await baseQ.OrderByDescending(p => p.CreatedDate).Take(limit).ToListAsync(cancellationToken);
			var bestSale = await baseQ.Where(p => p.IsSale).OrderByDescending(p => p.GiamGia).Take(limit).ToListAsync(cancellationToken);

			return new HomeBlocks
			{
				Featured = featured,
				Hot = hot,
				Newest = newest,
				BestSale = bestSale
			};
		}
	}
}


