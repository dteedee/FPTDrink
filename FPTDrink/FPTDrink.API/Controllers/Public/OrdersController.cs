using FPTDrink.API.DTOs.Public.Orders;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderPublicQueryService _service;

		public OrdersController(IOrderPublicQueryService service)
		{
			_service = service;
		}

		[HttpGet("{maHoaDon}")]
		public async Task<IActionResult> GetByCode([FromRoute] string maHoaDon, CancellationToken ct = default)
		{
			var order = await _service.GetByCodeAsync(maHoaDon, ct);
			if (order == null) return NotFound();
			return Ok(order);
		}

		[HttpGet("by-customer")]
		public async Task<IActionResult> GetByCustomer([FromQuery] OrderNameCccdQuery query, CancellationToken ct = default)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var orders = await _service.GetByNameAndCccdAsync(query.TenKhachHang, query.CCCD, ct);
			return Ok(orders);
		}
	}
}


