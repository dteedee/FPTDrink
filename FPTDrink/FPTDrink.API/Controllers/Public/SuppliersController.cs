using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.API.DTOs.Public.Suppliers;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class SuppliersController : ControllerBase
	{
		private readonly ISupplierPublicService _service;
		private readonly IMapper _mapper;

		public SuppliersController(ISupplierPublicService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> Get(CancellationToken ct)
		{
			var items = await _service.GetActiveSuppliersAsync(ct);
			return Ok(items.Select(x => new { x.MaNhaCungCap, x.Title }));
		}

		[HttpGet("{supplierId}/products")]
		public async Task<IActionResult> GetProducts([FromRoute] string supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
		{
			var (items, total) = await _service.GetProductsBySupplierAsync(supplierId, page, pageSize, ct);
			return Ok(new PagedResultDto<ProductListItemDto>
			{
				Page = page,
				PageSize = pageSize,
				Total = total,
				Items = _mapper.Map<IReadOnlyList<ProductListItemDto>>(items)
			});
		}
	}
}


