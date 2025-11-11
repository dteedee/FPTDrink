using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ISupplierPublicService
	{
		Task<IReadOnlyList<NhaCungCap>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default);
		Task<(IReadOnlyList<Product> items, int total)> GetProductsBySupplierAsync(string supplierId, int page, int pageSize, CancellationToken cancellationToken = default);
	}
}


