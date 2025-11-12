using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class DashboardController : AdminBaseController
	{
		private readonly IReportingService _reportingService;

		public DashboardController(IReportingService reportingService)
		{
			_reportingService = reportingService;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Index(CancellationToken cancellationToken)
		{
			ViewData["Title"] = "Bảng điều khiển";

			var overviewSource = await _reportingService.GetOverviewStatsAsync(cancellationToken);
			var overview = MapOverview(overviewSource);

			var revenueChartSource = await _reportingService.GetRevenueChartAsync("7days", cancellationToken);
			var revenueChart = revenueChartSource.Select(x => (Label: ToString(x, "Date"), Value: ToDecimal(x, "Revenue"))).ToList();

			var topProductsSource = await _reportingService.GetTopProductsAsync(6, cancellationToken);
			var topProducts = MapTopProducts(topProductsSource);

			var recentOrdersSource = await _reportingService.GetRecentOrdersAsync(8, cancellationToken);
			var recentOrders = MapRecentOrders(recentOrdersSource);

			var model = new DashboardViewModel
			{
				Overview = overview,
				RevenueChart = revenueChart,
				TopProducts = topProducts,
				RecentOrders = recentOrders
			};

			return View(model);
		}

		private static DashboardOverviewViewModel MapOverview(object source)
		{
			var overview = new DashboardOverviewViewModel();
			try
			{
				var json = JsonSerializer.Serialize(source);
				var stats = JsonSerializer.Deserialize<StatisticsOverviewSection>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (stats != null)
				{
					overview.RevenueTotal = stats.Revenue?.Total ?? 0m;
					overview.RevenueToday = stats.Revenue?.Today ?? 0m;
					overview.RevenueThisMonth = stats.Revenue?.ThisMonth ?? 0m;
					overview.RevenueLastMonth = stats.Revenue?.LastMonth ?? 0m;
					overview.RevenueGrowthPercent = stats.Revenue?.Growth ?? 0m;

					overview.OrdersTotal = stats.Orders?.Total ?? 0;
					overview.OrdersToday = stats.Orders?.Today ?? 0;
					overview.OrdersPaid = stats.Orders?.Paid ?? 0;
					overview.OrdersPending = stats.Orders?.Pending ?? 0;

					overview.ProductActiveCount = stats.Products?.Total ?? 0;
					overview.ProductOutOfStock = stats.Products?.OutOfStock ?? 0;

					overview.CustomerTotal = stats.Customers?.Total ?? 0;
					overview.CustomerNewThisMonth = stats.Customers?.NewThisMonth ?? 0;

					overview.CategoryTotal = stats.Categories;
					overview.SupplierTotal = stats.Suppliers;
					overview.EmployeeTotal = stats.Employees;
				}
			}
			catch
			{
			}
			return overview;
		}

		private static decimal ToDecimal(object obj, string property)
		{
			dynamic dynamicObj = obj;
			try
			{
				return Convert.ToDecimal(dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null) ?? 0m);
			}
			catch
			{
				return 0m;
			}
		}

		private static string ToString(object obj, string property)
		{
			dynamic dynamicObj = obj;
			try
			{
				return Convert.ToString(dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null)) ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		private static List<DashboardTopProductViewModel> MapTopProducts(IReadOnlyList<object> source)
		{
			var list = new List<DashboardTopProductViewModel>();
			foreach (var item in source)
			{
				try
				{
					var json = JsonSerializer.Serialize(item);
					var element = JsonSerializer.Deserialize<JsonElement>(json);
					
					list.Add(new DashboardTopProductViewModel
					{
						ProductId = element.TryGetProperty("ProductID", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty,
						ProductName = element.TryGetProperty("ProductName", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
						ProductImage = element.TryGetProperty("ProductImage", out var imgProp) ? imgProp.GetString() : null,
						TotalQuantity = element.TryGetProperty("TotalQuantity", out var qtyProp) && qtyProp.ValueKind == JsonValueKind.Number ? qtyProp.GetInt32() : 0,
						TotalRevenue = element.TryGetProperty("TotalRevenue", out var revProp) && revProp.ValueKind == JsonValueKind.Number ? revProp.GetDecimal() : 0m
					});
				}
				catch
				{
				}
			}
			return list;
		}

		private static List<DashboardRecentOrderViewModel> MapRecentOrders(IReadOnlyList<object> source)
		{
			var list = new List<DashboardRecentOrderViewModel>();
			foreach (var item in source)
			{
				try
				{
					var json = JsonSerializer.Serialize(item);
					var element = JsonSerializer.Deserialize<JsonElement>(json);
					
					DateTime createdDate = DateTime.Now;
					if (element.TryGetProperty("CreatedDate", out var dateProp))
					{
						if (dateProp.ValueKind == JsonValueKind.String)
						{
							DateTime.TryParse(dateProp.GetString(), out createdDate);
						}
						else if (dateProp.TryGetDateTime(out var dt))
						{
							createdDate = dt;
						}
					}

					list.Add(new DashboardRecentOrderViewModel
					{
						OrderCode = element.TryGetProperty("MaHoaDon", out var codeProp) ? codeProp.GetString() ?? string.Empty : string.Empty,
						CustomerName = element.TryGetProperty("TenKhachHang", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
						CreatedDate = createdDate,
						Status = element.TryGetProperty("TrangThai", out var statusProp) && statusProp.ValueKind == JsonValueKind.Number ? statusProp.GetInt32() : 0,
						PaymentMethod = element.TryGetProperty("PhuongThucThanhToan", out var payProp) && payProp.ValueKind == JsonValueKind.Number ? payProp.GetInt32() : 0,
						TotalAmount = element.TryGetProperty("TongTien", out var totalProp) && totalProp.ValueKind == JsonValueKind.Number ? totalProp.GetDecimal() : 0m
					});
				}
				catch
				{
				}
			}
			return list;
		}

	}
}

