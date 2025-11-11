namespace FPTDrink.Core.Interfaces.Services
{
	public interface IVisitorStatsService
	{
		Task<FPTDrink.Core.Models.Reports.VisitorStats> GetVisitorStatsAsync(CancellationToken cancellationToken = default);
	}
}


