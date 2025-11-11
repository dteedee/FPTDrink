using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IOrderService
	{
		Task<IReadOnlyList<HoaDon>> GetListAsync(string? search, CancellationToken cancellationToken = default);
		Task<HoaDon?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<ChiTietHoaDon>> GetItemsAsync(string id, CancellationToken cancellationToken = default);
		Task<(bool success, string message)> UpdateStatusAsync(string id, int newStatus, bool confirmed, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.CustomerSummaryInfo>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default);
		Task<FPTDrink.Core.Models.Reports.CustomerDetailsInfo?> GetCustomerDetailsAsync(string id, CancellationToken cancellationToken = default);
	}
}


