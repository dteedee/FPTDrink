using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface INhanVienRepository
	{
		IQueryable<NhanVien> Query();
		Task<NhanVien?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
	}
}


