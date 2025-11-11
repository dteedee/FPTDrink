using System.Globalization;
using FPTDrink.API.DTOs.Admin.Statistics;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class StatisticsController : ControllerBase
	{
		private readonly IReportingService _reporting;

		public StatisticsController(IReportingService reporting)
		{
			_reporting = reporting;
		}

		[HttpGet("revenue")]
		public async Task<IActionResult> Revenue([FromQuery] DateRangeRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var from = DateTime.ParseExact(req.FromDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var to = DateTime.ParseExact(req.ToDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var data = await _reporting.GetStatisticalAsync(from, to, ct);
			return Ok(new { Data = data });
		}

		[HttpGet("product-sales")]
		public async Task<IActionResult> ProductSales([FromQuery] DateRangeRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var from = DateTime.ParseExact(req.FromDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var to = DateTime.ParseExact(req.ToDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var data = await _reporting.GetProductSalesAsync(from, to, ct);
			return Ok(new { Data = data });
		}

		[HttpGet("product-sales/{date}")]
		public async Task<IActionResult> ProductSalesDetail([FromRoute] string date, CancellationToken ct)
		{
			var d = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var data = await _reporting.GetProductSalesDetailAsync(d, ct);
			return Ok(new { Data = data });
		}

		[HttpGet("payment-methods")]
		public async Task<IActionResult> PaymentMethods([FromQuery] DateRangeRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var from = DateTime.ParseExact(req.FromDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var to = DateTime.ParseExact(req.ToDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var data = await _reporting.GetPaymentMethodsStatisticalAsync(from, to, ct);
			return Ok(new { Data = data });
		}

		[HttpGet("payment-methods/{date}")]
		public async Task<IActionResult> PaymentMethodDetail([FromRoute] string date, CancellationToken ct)
		{
			var d = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			var data = await _reporting.GetPaymentMethodDetailAsync(d, ct);
			return Ok(new { Data = data });
		}
	}
}


