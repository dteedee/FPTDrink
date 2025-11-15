using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class KhachHangRepository : IKhachHangRepository
	{
		private readonly FptdrinkContext _dbContext;
		private readonly DbSet<KhachHang> _set;

		public KhachHangRepository(FptdrinkContext dbContext)
		{
			_dbContext = dbContext;
			_set = _dbContext.Set<KhachHang>();
		}

		public IQueryable<KhachHang> Query() => _set.AsQueryable();

		public Task<KhachHang?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public Task<KhachHang?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.TenDangNhap == username, cancellationToken);
		}

		public Task<KhachHang?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
		}

		public Task<KhachHang?> FindByPhoneAsync(string phone, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.SoDienThoai == phone, cancellationToken);
		}

		public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return _set.AnyAsync(x => x.TenDangNhap == username, cancellationToken);
		}

		public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return _set.AnyAsync(x => x.Email == email, cancellationToken);
		}

		public Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default)
		{
			return _set.AnyAsync(x => x.SoDienThoai == phone, cancellationToken);
		}

		public Task AddAsync(KhachHang entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(KhachHang entity) => _set.Update(entity);

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}

