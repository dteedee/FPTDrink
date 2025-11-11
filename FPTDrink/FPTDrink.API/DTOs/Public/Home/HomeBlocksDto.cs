namespace FPTDrink.API.DTOs.Public.Home
{
	public class HomeBlocksDto
	{
		public IReadOnlyList<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto> Featured { get; set; } = Array.Empty<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto>();
		public IReadOnlyList<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto> Hot { get; set; } = Array.Empty<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto>();
		public IReadOnlyList<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto> Newest { get; set; } = Array.Empty<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto>();
		public IReadOnlyList<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto> BestSale { get; set; } = Array.Empty<FPTDrink.API.DTOs.Public.Catalog.ProductListItemDto>();
	}
}


