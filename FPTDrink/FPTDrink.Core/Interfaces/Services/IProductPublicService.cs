using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IProductPublicService
	{
		Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<Product>> GetRelatedAsync(string id, int limit, CancellationToken cancellationToken = default);
	}
}


