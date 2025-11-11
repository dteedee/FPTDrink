using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface INhanVienService
	{
		Task<IReadOnlyList<NhanVien>> GetListAsync(string status, string? search, string? excludeUserId, CancellationToken cancellationToken = default);
		Task<NhanVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<NhanVien> CreateAsync(NhanVien model, CancellationToken cancellationToken = default);
		Task<bool> UpdateAsync(NhanVien model, CancellationToken cancellationToken = default);
		Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default);
		Task<int> MoveToTrashBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
		Task<int> DeleteBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<bool> UndoAsync(string id, CancellationToken cancellationToken = default);
		Task<int> UndoBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
		Task<bool> ResetPasswordAsync(string id, string newPassword, CancellationToken cancellationToken = default);
	}
}


