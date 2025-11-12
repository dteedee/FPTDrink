using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
	public class ProductsListViewModel
	{
		[JsonPropertyName("page")]
		public int Page { get; set; } = 1;

		[JsonPropertyName("pageSize")]
		public int PageSize { get; set; } = 12;

		[JsonPropertyName("total")]
		public int Total { get; set; }

		[JsonPropertyName("items")]
		public List<ProductListItemViewModel> Items { get; set; } = new();

		public int? CategoryId { get; set; }
		public string? SupplierId { get; set; }
		public string? Q { get; set; }
		public decimal? PriceFrom { get; set; }
		public decimal? PriceTo { get; set; }
		public string? Sort { get; set; }
	}
}

