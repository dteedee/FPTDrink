using System.Collections.Generic;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminStatisticsViewModel
	{
		public StatisticsOverviewSection Overview { get; set; } = new();
		public FPTDrink.Core.Models.Reports.VisitorStats VisitorStats { get; set; } = new();
		public List<ChartPoint> RevenueMonth { get; set; } = new();
		public List<ChartPoint> RevenueYear { get; set; } = new();
		public List<TopProductPoint> TopProducts { get; set; } = new();

		public record ChartPoint(string Label, decimal Value);

		public record TopProductPoint(string Name, int Quantity, decimal Revenue);
	}

	public class StatisticsOverviewSection
	{
		public StatisticsRevenueOverview Revenue { get; set; } = new();
		public StatisticsOrdersOverview Orders { get; set; } = new();
		public StatisticsProductsOverview Products { get; set; } = new();
		public StatisticsCustomersOverview Customers { get; set; } = new();
		public int Categories { get; set; }
		public int Suppliers { get; set; }
		public int Employees { get; set; }
	}

	public class StatisticsRevenueOverview
	{
		public decimal Total { get; set; }
		public decimal Today { get; set; }
		public decimal ThisMonth { get; set; }
		public decimal LastMonth { get; set; }
		public decimal Growth { get; set; }
	}

	public class StatisticsOrdersOverview
	{
		public int Total { get; set; }
		public int Today { get; set; }
		public int Paid { get; set; }
		public int Pending { get; set; }
	}

	public class StatisticsProductsOverview
	{
		public int Total { get; set; }
		public int OutOfStock { get; set; }
	}

	public class StatisticsCustomersOverview
	{
		public int Total { get; set; }
		public int NewThisMonth { get; set; }
	}
}

