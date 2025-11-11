using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class NhanVienRepository : INhanVienRepository
	{
		private readonly FptdrinkContext _db;

		public NhanVienRepository(FptdrinkContext db)
		{
			_db = db;
		}

		public IQueryable<NhanVien> Query() => _db.NhanViens.AsQueryable();

		public Task<NhanVien?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return _db.NhanViens.FirstOrDefaultAsync(x => x.TenDangNhap != null && x.TenDangNhap.ToLower() == username.ToLower(), cancellationToken);
		}
	}
}


