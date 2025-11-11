using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class ChiTietHoaDonRepository : IChiTietHoaDonRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<ChiTietHoaDon> _set;

		public ChiTietHoaDonRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<ChiTietHoaDon>();
		}

		public IQueryable<ChiTietHoaDon> Query() => _set.AsQueryable();

		public async Task<IReadOnlyList<ChiTietHoaDon>> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
		{
			return await _set.Where(x => x.OrderId == orderId).ToListAsync(cancellationToken);
		}
	}
}


