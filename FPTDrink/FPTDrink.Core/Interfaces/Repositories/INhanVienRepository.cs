using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface INhanVienRepository
	{
		IQueryable<NhanVien> Query();
		Task<NhanVien?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<NhanVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task AddAsync(NhanVien entity, CancellationToken cancellationToken = default);
		void Update(NhanVien entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


