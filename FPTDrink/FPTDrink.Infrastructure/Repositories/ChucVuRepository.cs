using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class ChucVuRepository : IChucVuRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<ChucVu> _set;

		public ChucVuRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<ChucVu>();
		}

		public IQueryable<ChucVu> Query() => _set.AsQueryable();

		public Task<ChucVu?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public Task AddAsync(ChucVu entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(ChucVu entity)
		{
			_set.Update(entity);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _db.SaveChangesAsync(cancellationToken);
		}
	}
}


