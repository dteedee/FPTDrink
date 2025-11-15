using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IKhachHangRepository
	{
		IQueryable<KhachHang> Query();
		Task<KhachHang?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<KhachHang?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<KhachHang?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<KhachHang?> FindByPhoneAsync(string phone, CancellationToken cancellationToken = default);
		Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default);
		Task AddAsync(KhachHang entity, CancellationToken cancellationToken = default);
		void Update(KhachHang entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}

