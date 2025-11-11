using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IProductCategoryService
	{
		Task<IReadOnlyList<ProductCategory>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default);
		Task<ProductCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<ProductCategory> CreateAsync(ProductCategory model, CancellationToken cancellationToken = default);
		Task<bool> UpdateAsync(ProductCategory model, CancellationToken cancellationToken = default);
		Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default);
		Task<int> MoveToTrashBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
		Task<int> DeleteBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<bool> UndoAsync(string id, CancellationToken cancellationToken = default);
		Task<int> UndoBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<(bool success, bool? isActive)> ToggleActiveAsync(string id, CancellationToken cancellationToken = default);
	}
}


