using AutoMapper;
using FPTDrink.API.Authorization;
using FPTDrink.API.DTOs.Admin.Order;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _service;
		private readonly IMapper _mapper;

		public OrderController(IOrderService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> GetList([FromQuery] string? search = null, CancellationToken ct = default)
		{
			var data = await _service.GetListAsync(search, ct);
			return Ok(_mapper.Map<IReadOnlyList<OrderDto>>(data));
		}

		[HttpGet("{id}")]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Get(string id, CancellationToken ct = default)
		{
			var item = await _service.GetByIdAsync(id, ct);
			if (item == null) return NotFound();
			return Ok(_mapper.Map<OrderDto>(item));
		}

		[HttpGet("{id}/items")]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> GetItems(string id, CancellationToken ct = default)
		{
			var items = await _service.GetItemsAsync(id, ct);
			return Ok(_mapper.Map<IReadOnlyList<OrderItemDto>>(items));
		}

		[HttpPost("{id}/status")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý", "Thu ngân")]
		public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request, CancellationToken ct = default)
		{
			var (success, message) = await _service.UpdateStatusAsync(id, request.TrangThai, request.Confirmed, ct);
			if (!success) return BadRequest(new { success, message });
			return Ok(new { success, message });
		}

		[HttpGet("customers")]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Customers([FromQuery] string? search = null, CancellationToken ct = default)
		{
			var data = await _service.GetCustomersAsync(search, ct);
			return Ok(_mapper.Map<IReadOnlyList<CustomerSummaryDto>>(data));
		}

		[HttpGet("customers/{id}")]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> CustomerDetails(string id, CancellationToken ct = default)
		{
			var data = await _service.GetCustomerDetailsAsync(id, ct);
			if (data == null) return NotFound();
			return Ok(_mapper.Map<CustomerDetailsDto>(data));
		}
	}
}


