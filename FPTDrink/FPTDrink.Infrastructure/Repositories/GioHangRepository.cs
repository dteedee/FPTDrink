using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class GioHangRepository : IGioHangRepository
	{
		private readonly FptdrinkContext _dbContext;
		private readonly DbSet<GioHang> _set;

		public GioHangRepository(FptdrinkContext dbContext)
		{
			_dbContext = dbContext;
			_set = _dbContext.Set<GioHang>();
		}

		public IQueryable<GioHang> Query() => _set.AsQueryable();

		public Task<GioHang?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public async Task<IReadOnlyList<GioHang>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
		{
			return await _set
				.Where(x => x.IdKhachHang == customerId)
				.ToListAsync(cancellationToken);
		}

		public Task<GioHang?> FindByCustomerAndProductAsync(string customerId, string productId, CancellationToken cancellationToken = default)
		{
			return _set.FirstOrDefaultAsync(
				x => x.IdKhachHang == customerId && x.MaSanPham == productId,
				cancellationToken);
		}

		public Task AddAsync(GioHang entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(GioHang entity) => _set.Update(entity);

		public void Remove(GioHang entity) => _set.Remove(entity);

		public async Task RemoveByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
		{
			var items = await _set.Where(x => x.IdKhachHang == customerId).ToListAsync(cancellationToken);
			if (items.Count > 0)
			{
				_set.RemoveRange(items);
			}
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}

