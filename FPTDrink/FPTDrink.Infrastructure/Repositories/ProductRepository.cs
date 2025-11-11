using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class ProductRepository : IProductRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<Product> _set;

		public ProductRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<Product>();
		}

		public IQueryable<Product> Query() => _set.AsQueryable();

		public Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.MaSanPham == id, cancellationToken);
		}

		public Task AddAsync(Product product, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(product, cancellationToken).AsTask();
		}

		public void Update(Product product) => _set.Update(product);

		public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
	}
}


