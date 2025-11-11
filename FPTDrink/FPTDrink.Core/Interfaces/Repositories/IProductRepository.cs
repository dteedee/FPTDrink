using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IProductRepository
	{
		Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		void Update(Product product);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


