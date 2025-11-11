using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IOrderPublicQueryService
	{
		Task<HoaDon?> GetByCodeAsync(string maHoaDon, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<HoaDon>> GetByNameAndCccdAsync(string tenKhachHang, string cccd, CancellationToken cancellationToken = default);
	}
}


