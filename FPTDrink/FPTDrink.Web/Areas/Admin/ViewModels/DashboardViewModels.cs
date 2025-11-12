using System;
using System.Collections.Generic;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class DashboardOverviewViewModel
	{
		public decimal RevenueTotal { get; set; }
		public decimal RevenueToday { get; set; }
		public decimal RevenueThisMonth { get; set; }
		public decimal RevenueLastMonth { get; set; }
		public decimal RevenueGrowthPercent { get; set; }

		public int OrdersTotal { get; set; }
		public int OrdersToday { get; set; }
		public int OrdersPaid { get; set; }
		public int OrdersPending { get; set; }

		public int ProductActiveCount { get; set; }
		public int ProductOutOfStock { get; set; }

		public int CustomerTotal { get; set; }
		public int CustomerNewThisMonth { get; set; }

		public int CategoryTotal { get; set; }
		public int SupplierTotal { get; set; }
		public int EmployeeTotal { get; set; }
	}

	public class DashboardTopProductViewModel
	{
		public string ProductId { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string? ProductImage { get; set; }
		public int TotalQuantity { get; set; }
		public decimal TotalRevenue { get; set; }
	}

	public class DashboardRecentOrderViewModel
	{
		public string OrderCode { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; }
		public int Status { get; set; }
		public int PaymentMethod { get; set; }
		public decimal TotalAmount { get; set; }
	}

	public class DashboardViewModel
	{
		public DashboardOverviewViewModel Overview { get; set; } = new();
		public List<DashboardTopProductViewModel> TopProducts { get; set; } = new();
		public List<DashboardRecentOrderViewModel> RecentOrders { get; set; } = new();
		public List<(string Label, decimal Value)> RevenueChart { get; set; } = new();
	}
}

