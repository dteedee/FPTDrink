using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class SupplierPublicService : ISupplierPublicService
	{
		private readonly INhaCungCapRepository _supRepo;
		private readonly IProductRepository _prodRepo;

		public SupplierPublicService(INhaCungCapRepository supRepo, IProductRepository prodRepo)
		{
			_supRepo = supRepo;
			_prodRepo = prodRepo;
		}

		public Task<IReadOnlyList<NhaCungCap>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default)
		{
			return _supRepo.Query().Where(x => x.Status != 0).OrderBy(x => x.Title).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<NhaCungCap>)t.Result, cancellationToken);
		}

		public async Task<(IReadOnlyList<Product> items, int total)> GetProductsBySupplierAsync(string supplierId, int page, int pageSize, CancellationToken cancellationToken = default)
		{
			var activeCateIds = await _prodRepo.Query().Select(p => p.ProductCategoryId).Distinct().ToListAsync(cancellationToken);
			IQueryable<Product> q = _prodRepo.Query()
				.Include(p => p.ProductCategory)
				.Where(p => p.Status != 0 && p.IsActive && p.SupplierId == supplierId && activeCateIds.Contains(p.ProductCategoryId));
			int total = await q.CountAsync(cancellationToken);
			var items = await q.OrderBy(p => p.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
			return (items, total);
		}
	}
}


