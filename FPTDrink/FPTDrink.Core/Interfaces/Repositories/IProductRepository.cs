using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IProductRepository
	{
		IQueryable<Product> Query();
		Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task AddAsync(Product product, CancellationToken cancellationToken = default);
		void Update(Product product);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


