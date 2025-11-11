using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Common
{
	public class IdsRequest
	{
		[MinLength(1, ErrorMessage = "Danh sách ids phải có ít nhất 1 phần tử.")]
		public int[] Ids { get; set; } = Array.Empty<int>();
	}
}


