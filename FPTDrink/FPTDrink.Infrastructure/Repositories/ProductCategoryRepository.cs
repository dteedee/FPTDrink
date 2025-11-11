using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class ProductCategoryRepository : IProductCategoryRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<ProductCategory> _set;

		public ProductCategoryRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<ProductCategory>();
		}

		public IQueryable<ProductCategory> Query() => _set.AsQueryable();

		public Task<ProductCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.MaLoaiSanPham == id, cancellationToken);
		}

		public Task AddAsync(ProductCategory entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(ProductCategory entity)
		{
			_set.Update(entity);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _db.SaveChangesAsync(cancellationToken);
		}
	}
}


