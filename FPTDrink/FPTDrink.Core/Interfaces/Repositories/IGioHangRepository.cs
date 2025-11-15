using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IGioHangRepository
	{
		IQueryable<GioHang> Query();
		Task<GioHang?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<GioHang>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
		Task<GioHang?> FindByCustomerAndProductAsync(string customerId, string productId, CancellationToken cancellationToken = default);
		Task AddAsync(GioHang entity, CancellationToken cancellationToken = default);
		void Update(GioHang entity);
		void Remove(GioHang entity);
		Task RemoveByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}

