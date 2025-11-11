using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IPhanQuyenRepository
	{
		IQueryable<PhanQuyen> Query();
		Task<PhanQuyen?> FindAsync(int idChucVu, string maChucNang, CancellationToken cancellationToken = default);
		Task AddAsync(PhanQuyen entity, CancellationToken cancellationToken = default);
		void Remove(PhanQuyen entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


