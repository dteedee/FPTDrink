using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class PhanQuyenRepository : IPhanQuyenRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<PhanQuyen> _set;

		public PhanQuyenRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<PhanQuyen>();
		}

		public IQueryable<PhanQuyen> Query() => _set.AsQueryable();

		public Task<PhanQuyen?> FindAsync(int idChucVu, string maChucNang, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.IdchucVu == idChucVu && x.MaChucNang == maChucNang, cancellationToken);
		}

		public Task AddAsync(PhanQuyen entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Remove(PhanQuyen entity)
		{
			_set.Remove(entity);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _db.SaveChangesAsync(cancellationToken);
		}
	}
}


