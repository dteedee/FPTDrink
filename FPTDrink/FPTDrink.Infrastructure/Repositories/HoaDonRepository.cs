using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class HoaDonRepository : IHoaDonRepository
	{
		private readonly FptdrinkContext _db;
		private readonly DbSet<HoaDon> _set;

		public HoaDonRepository(FptdrinkContext db)
		{
			_db = db;
			_set = _db.Set<HoaDon>();
		}

		public IQueryable<HoaDon> Query() => _set.AsQueryable();

		public Task<HoaDon?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _set.Include(x => x.ChiTietHoaDons).FirstOrDefaultAsync(x => x.MaHoaDon == id, cancellationToken);
		}

		public void Update(HoaDon entity) => _set.Update(entity);

		public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
	}
}


