using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IChiTietHoaDonRepository
	{
		IQueryable<ChiTietHoaDon> Query();
		Task<IReadOnlyList<ChiTietHoaDon>> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
	}
}


