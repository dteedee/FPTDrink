using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ISearchService
	{
		Task<IReadOnlyList<string>> SuggestAsync(string q, int limit, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<Product>> SearchAsync(string q, CancellationToken cancellationToken = default);
	}
}


