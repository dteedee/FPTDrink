using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class NhaCungCapRepository : INhaCungCapRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<NhaCungCap> _set;

		public NhaCungCapRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<NhaCungCap>();
		}

		public IQueryable<NhaCungCap> Query() => _set.AsQueryable();

		public Task<NhaCungCap?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.MaNhaCungCap == id, cancellationToken);
		}

		public Task AddAsync(NhaCungCap entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(NhaCungCap entity) => _set.Update(entity);

		public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
	}
}


