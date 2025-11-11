using FPTDrink.Infrastructure.Data;
using FPTDrink.Core.Models;
using FPTDrink.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<Category> _set;

		public CategoryRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<Category>();
		}

		public IQueryable<Category> Query()
		{
			return _set.AsQueryable();
		}

		public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public Task AddAsync(Category entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(Category entity)
		{
			_set.Update(entity);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _db.SaveChangesAsync(cancellationToken);
		}
	}
}


