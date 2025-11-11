using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ICategoryService
	{
		Task<IReadOnlyList<Category>> GetListAsync(string status, CancellationToken cancellationToken = default);
		Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
		Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default);
		Task<bool> MoveToTrashAsync(int id, CancellationToken cancellationToken = default);
		Task<int> MoveToTrashBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
		Task<bool> UndoAsync(int id, CancellationToken cancellationToken = default);
		Task<int> UndoBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
		Task<(bool success, bool? isActive)> ToggleActiveAsync(int id, CancellationToken cancellationToken = default);
	}
}


