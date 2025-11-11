using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ICatalogQueryService
	{
		Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
		Task<(IReadOnlyList<Product> items, int total)> GetProductsAsync(int page, int pageSize, string? categoryId, string? supplierId, string? q, decimal? priceFrom, decimal? priceTo, string? sort, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
	}
}


