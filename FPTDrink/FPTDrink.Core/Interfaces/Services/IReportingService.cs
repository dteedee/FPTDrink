namespace FPTDrink.Core.Interfaces.Services
{
	public interface IReportingService
	{
		Task<object> GetOverviewStatsAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetRevenueChartAsync(string period, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetTopProductsAsync(int top, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
	}
}


