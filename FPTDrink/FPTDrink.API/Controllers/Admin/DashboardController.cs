using FPTDrink.API.Authorization;
using FPTDrink.API.DTOs.Admin.Dashboard;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class DashboardController : ControllerBase
	{
		private readonly IReportingService _reporting;

		public DashboardController(IReportingService reporting)
		{
			_reporting = reporting;
		}

		[HttpGet("overview")]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> GetOverview(CancellationToken ct)
		{
			var data = await _reporting.GetOverviewStatsAsync(ct);
			return Ok(new { success = true, data });
		}

		[HttpGet("revenue-chart")]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> GetRevenueChart([FromQuery] string period = "7days", CancellationToken ct = default)
		{
			var data = await _reporting.GetRevenueChartAsync(period, ct);
			return Ok(new { success = true, data });
		}

		[HttpGet("top-products")]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10, CancellationToken ct = default)
		{
			var data = await _reporting.GetTopProductsAsync(top, ct);
			return Ok(new { success = true, data });
		}

		[HttpGet("recent-orders")]
		[PermissionAuthorize("FPTDrink_ThongKe", "Quản lý", "Kế toán")]
		public async Task<IActionResult> GetRecentOrders([FromQuery] int count = 10, CancellationToken ct = default)
		{
			var data = await _reporting.GetRecentOrdersAsync(count, ct);
			return Ok(new { success = true, data });
		}
	}
}


