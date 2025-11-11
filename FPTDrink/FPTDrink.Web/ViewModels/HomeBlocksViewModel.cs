using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
	public class HomeBlocksViewModel
	{
		[JsonPropertyName("featured")]
		public List<ProductListItemViewModel> Featured { get; set; } = new();

		[JsonPropertyName("hot")]
		public List<ProductListItemViewModel> Hot { get; set; } = new();

		[JsonPropertyName("newest")]
		public List<ProductListItemViewModel> Newest { get; set; } = new();

		[JsonPropertyName("bestSale")]
		public List<ProductListItemViewModel> BestSale { get; set; } = new();
	}
}

