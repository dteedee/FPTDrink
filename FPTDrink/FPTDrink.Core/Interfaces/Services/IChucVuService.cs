using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IChucVuService
	{
		Task<IReadOnlyList<ChucVu>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default);
		Task<ChucVu?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<ChucVu> CreateAsync(ChucVu model, CancellationToken cancellationToken = default);
		Task<bool> UpdateAsync(ChucVu model, CancellationToken cancellationToken = default);
		Task<bool> MoveToTrashAsync(int id, CancellationToken cancellationToken = default);
		Task<int> MoveToTrashBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
		Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
		Task<bool> UndoAsync(int id, CancellationToken cancellationToken = default);
		Task<int> UndoBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
		Task<bool> TogglePermissionAsync(int chucVuId, string maChucNang, CancellationToken cancellationToken = default);
	}
}


