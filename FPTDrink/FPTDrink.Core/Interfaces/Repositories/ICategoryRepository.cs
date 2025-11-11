using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface ICategoryRepository
	{
		IQueryable<Category> Query();
		Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task AddAsync(Category entity, CancellationToken cancellationToken = default);
		void Update(Category entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


