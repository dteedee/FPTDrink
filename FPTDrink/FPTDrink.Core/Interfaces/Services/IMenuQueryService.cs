using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IMenuQueryService
	{
		Task<IReadOnlyList<Category>> GetTopMenuAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<ProductCategory>> GetProductCategoriesAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<NhaCungCap>> GetSuppliersAsync(CancellationToken cancellationToken = default);
	}
}


