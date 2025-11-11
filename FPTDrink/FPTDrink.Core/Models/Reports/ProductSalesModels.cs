using System.Collections.Generic;

namespace FPTDrink.Core.Models.Reports
{
	public class ProductQuantity
	{
		public string ProductName { get; set; } = string.Empty;
		public int Quantity { get; set; }
	}

	public class ProductSalesPoint
	{
		public System.DateTime Date { get; set; }
		public int TotalProducts { get; set; }
		public List<ProductQuantity> Products { get; set; } = new List<ProductQuantity>();
		public string BestSellingProduct { get; set; } = string.Empty;
	}

	public class ProductSalesDetailItem
	{
		public string? ProductID { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal GiaNhap { get; set; }
	}
}


