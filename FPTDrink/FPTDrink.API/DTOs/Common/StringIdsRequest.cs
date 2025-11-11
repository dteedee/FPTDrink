using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Common
{
	public class StringIdsRequest
	{
		[MinLength(1)]
		public string[] Ids { get; set; } = Array.Empty<string>();
	}
}


