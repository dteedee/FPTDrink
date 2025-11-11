using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface INhaCungCapRepository
	{
		IQueryable<NhaCungCap> Query();
		Task<NhaCungCap?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task AddAsync(NhaCungCap entity, CancellationToken cancellationToken = default);
		void Update(NhaCungCap entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


