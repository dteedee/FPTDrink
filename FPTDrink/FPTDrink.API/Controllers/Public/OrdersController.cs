using FPTDrink.API.DTOs.Public.Checkout;
using FPTDrink.API.DTOs.Public.Orders;
using FPTDrink.API.Extensions;
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
		[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetByCode([FromRoute] string maHoaDon, CancellationToken ct = default)
		{
			var order = await _service.GetByCodeAsync(maHoaDon, ct);
			if (order == null) return NotFound();
			return Ok(OrderDetailMapper.ToDto(order));
		}

		[HttpGet("by-customer")]
		[ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetByCustomer([FromQuery] OrderNamePhoneQuery query, CancellationToken ct = default)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var orders = await _service.GetByNameAndPhoneAsync(query.TenKhachHang, query.SoDienThoai, ct);
			return Ok(OrderDetailMapper.ToDtos(orders));
		}
	}
}


