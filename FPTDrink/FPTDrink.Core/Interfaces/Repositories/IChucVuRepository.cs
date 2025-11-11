using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IChucVuRepository
	{
		IQueryable<ChucVu> Query();
		Task<ChucVu?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task AddAsync(ChucVu entity, CancellationToken cancellationToken = default);
		void Update(ChucVu entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


