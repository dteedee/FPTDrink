namespace FPTDrink.API.DTOs.Admin.Dashboard
{
	public class OverviewStatsDto
	{
		public RevenueDto Revenue { get; set; } = new RevenueDto();
		public OrdersDto Orders { get; set; } = new OrdersDto();
		public ProductsDto Products { get; set; } = new ProductsDto();
		public CustomersDto Customers { get; set; } = new CustomersDto();
		public int Categories { get; set; }
		public int Suppliers { get; set; }
		public int Employees { get; set; }
	}

	public class RevenueDto
	{
		public decimal Total { get; set; }
		public decimal Today { get; set; }
		public decimal ThisMonth { get; set; }
		public decimal LastMonth { get; set; }
		public decimal Growth { get; set; }
	}

	public class OrdersDto
	{
		public int Total { get; set; }
		public int Today { get; set; }
		public int Paid { get; set; }
		public int Pending { get; set; }
	}

	public class ProductsDto
	{
		public int Total { get; set; }
		public int OutOfStock { get; set; }
	}

	public class CustomersDto
	{
		public int Total { get; set; }
		public int NewThisMonth { get; set; }
	}
}


