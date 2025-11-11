using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class NhanVienRepository : INhanVienRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<NhanVien> _set;

		public NhanVienRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<NhanVien>();
		}

		public IQueryable<NhanVien> Query() => _set.AsQueryable();

		public Task<NhanVien?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.TenDangNhap != null && x.TenDangNhap.ToLower() == username.ToLower(), cancellationToken);
		}

		public Task<NhanVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public Task AddAsync(NhanVien entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(NhanVien entity)
		{
			_set.Update(entity);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _db.SaveChangesAsync(cancellationToken);
		}
	}
}


