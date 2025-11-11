using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Public.Suppliers
{
	public class SupplierProductsQuery
	{
		[Required] public string SupplierId { get; set; } = string.Empty;
		[Range(1, int.MaxValue)] public int Page { get; set; } = 1;
		[Range(1, 200)] public int PageSize { get; set; } = 20;
	}
}


