using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FPTDrink.API.DTOs.Public.Customer;
using FPTDrink.API.DTOs.Public.CustomerAuth;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/customer")]
	[Authorize(Policy = "Customer")]
	public class CustomerController : ControllerBase
	{
		private readonly ICustomerService _customerService;
		private readonly IOrderPublicQueryService _orderPublicQueryService;
		private readonly IMapper _mapper;

		public CustomerController(ICustomerService customerService, IOrderPublicQueryService orderPublicQueryService, IMapper mapper)
		{
			_customerService = customerService;
			_orderPublicQueryService = orderPublicQueryService;
			_mapper = mapper;
		}

		[HttpGet("profile")]
		public async Task<IActionResult> GetProfile(CancellationToken ct)
		{
			var customerId = GetCustomerId();
			if (customerId == null) return Forbid();
			var customer = await _customerService.GetByIdAsync(customerId, ct);
			if (customer == null) return NotFound("Không tìm thấy khách hàng.");
			return Ok(_mapper.Map<CustomerProfileDto>(customer));
		}

		[HttpPut("profile")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var customerId = GetCustomerId();
			if (customerId == null) return Forbid();
			var (success, error) = await _customerService.UpdateProfileAsync(customerId, new UpdateCustomerProfileRequest
			{
				HoTen = dto.HoTen,
				SoDienThoai = dto.SoDienThoai,
				NgaySinh = dto.NgaySinh,
				GioiTinh = dto.GioiTinh,
				DiaChi = dto.DiaChi,
				Image = dto.Image
			}, ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "Cập nhật hồ sơ thành công." });
		}

		[HttpGet("orders")]
		public async Task<IActionResult> GetOrders(CancellationToken ct)
		{
			var customerId = GetCustomerId();
			if (customerId == null) return Forbid();
			var orders = await _orderPublicQueryService.GetByCustomerIdAsync(customerId, ct);
			var dto = orders.Select(order => new CustomerOrderSummaryDto
			{
				MaHoaDon = order.MaHoaDon,
				CreatedDate = order.CreatedDate,
				TrangThai = order.TrangThai,
				TongTien = order.ChiTietHoaDons.Sum(x => x.GiaBan * x.SoLuong)
			});
			return Ok(dto);
		}

		private string? GetCustomerId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
	}
}

