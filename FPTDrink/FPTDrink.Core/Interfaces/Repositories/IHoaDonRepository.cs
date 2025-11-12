using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IHoaDonRepository
	{
		IQueryable<HoaDon> Query();
		Task<HoaDon?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		void Add(HoaDon entity);
		void Update(HoaDon entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


