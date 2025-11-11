namespace FPTDrink.Core.Interfaces.Services
{
	public interface IReportingService
	{
		Task<object> GetOverviewStatsAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetRevenueChartAsync(string period, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetTopProductsAsync(int top, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<object>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.RevenuePoint>> GetStatisticalAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.ProductSalesPoint>> GetProductSalesAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.ProductSalesDetailItem>> GetProductSalesDetailAsync(DateTime date, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.PaymentMethodStatPoint>> GetPaymentMethodsStatisticalAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<FPTDrink.Core.Models.Reports.InvoiceBrief>> GetPaymentMethodDetailAsync(DateTime date, CancellationToken cancellationToken = default);
	}
}


