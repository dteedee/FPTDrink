using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class CatalogController : ControllerBase
	{
		private readonly ICatalogQueryService _service;
		private readonly IMapper _mapper;

		public CatalogController(ICatalogQueryService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet("categories")]
		public async Task<IActionResult> Categories(CancellationToken ct)
		{
			var cats = await _service.GetActiveCategoriesAsync(ct);
			return Ok(cats.Select(c => new { c.Id, c.Title, c.Alias }));
		}

		[HttpGet("products")]
		public async Task<IActionResult> Products([FromQuery] ProductListQuery query, CancellationToken ct)
		{
			var (items, total) = await _service.GetProductsAsync(query.Page, query.PageSize, query.CategoryId, query.SupplierId, query.Q, query.PriceFrom, query.PriceTo, query.Sort, query.CartId, ct);
			var dto = new PagedResultDto<ProductListItemDto>
			{
				Page = query.Page,
				PageSize = query.PageSize,
				Total = total,
				Items = _mapper.Map<IReadOnlyList<ProductListItemDto>>(items)
			};
			return Ok(dto);
		}

		[HttpGet("categories/{categoryId}/products")]
		public async Task<IActionResult> ProductsByCategory([FromRoute] int categoryId, CancellationToken ct)
		{
			var items = await _service.GetProductsByCategoryAsync(categoryId, ct);
			return Ok(_mapper.Map<IReadOnlyList<ProductListItemDto>>(items));
		}
	}
}


