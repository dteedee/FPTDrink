using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class StatisticsController : AdminBaseController
	{
		private readonly IReportingService _reportingService;
		private readonly IVisitorStatsService _visitorStatsService;

		public StatisticsController(IReportingService reportingService, IVisitorStatsService visitorStatsService)
		{
			_reportingService = reportingService;
			_visitorStatsService = visitorStatsService;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Index(CancellationToken cancellationToken)
		{
			ViewData["Title"] = "Báo cáo & thống kê";

			var overview = await _reportingService.GetOverviewStatsAsync(cancellationToken);
			var visitorStats = await _visitorStatsService.GetVisitorStatsAsync(cancellationToken);
			var revenueMonth = await _reportingService.GetRevenueChartAsync("thisMonth", cancellationToken);
			var revenueYear = await _reportingService.GetRevenueChartAsync("thisYear", cancellationToken);
			var topProducts = await _reportingService.GetTopProductsAsync(10, cancellationToken);

			var model = new AdminStatisticsViewModel
			{
				Overview = MapOverview(overview),
				VisitorStats = visitorStats,
				RevenueMonth = revenueMonth.Select(x => new AdminStatisticsViewModel.ChartPoint(ToString(x, "Date"), ToDecimal(x, "Revenue"))).ToList(),
				RevenueYear = revenueYear.Select(x => new AdminStatisticsViewModel.ChartPoint(ToString(x, "Date"), ToDecimal(x, "Revenue"))).ToList(),
				TopProducts = topProducts.Select(x => new AdminStatisticsViewModel.TopProductPoint(ToString(x, "ProductName"), ToInt(x, "TotalQuantity"), ToDecimal(x, "TotalRevenue"))).ToList()
			};
			return View(model);
		}

		private static StatisticsOverviewSection MapOverview(object source)
		{
			try
			{
				var json = JsonSerializer.Serialize(source);
				var model = JsonSerializer.Deserialize<StatisticsOverviewSection>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				return model ?? new StatisticsOverviewSection();
			}
			catch
			{
				return new StatisticsOverviewSection();
			}
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

		private static int ToInt(object obj, string property)
		{
			dynamic dynamicObj = obj;
			try
			{
				return Convert.ToInt32(dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null) ?? 0);
			}
			catch
			{
				return 0;
			}
		}
	}
}

