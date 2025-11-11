using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public class HomeBlocks
	{
		public IReadOnlyList<Product> Featured { get; set; } = Array.Empty<Product>();
		public IReadOnlyList<Product> Hot { get; set; } = Array.Empty<Product>();
		public IReadOnlyList<Product> Newest { get; set; } = Array.Empty<Product>();
		public IReadOnlyList<Product> BestSale { get; set; } = Array.Empty<Product>();
	}

	public interface IHomePageService
	{
		Task<HomeBlocks> GetHomeBlocksAsync(int limit, CancellationToken cancellationToken = default);
	}
}


