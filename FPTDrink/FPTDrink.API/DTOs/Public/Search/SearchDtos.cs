using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Search
{
	public class SearchQuery
	{
		[Required, MinLength(2), StringLength(200)]
		public string Q { get; set; } = string.Empty;
	}

	public class SuggestQuery
	{
		[Required, MinLength(2), StringLength(200)]
		public string Q { get; set; } = string.Empty;
		[Range(1, 20)] public int Limit { get; set; } = 8;
	}
}


