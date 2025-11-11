using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IProductCategoryRepository
	{
		IQueryable<ProductCategory> Query();
		Task<ProductCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task AddAsync(ProductCategory entity, CancellationToken cancellationToken = default);
		void Update(ProductCategory entity);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


