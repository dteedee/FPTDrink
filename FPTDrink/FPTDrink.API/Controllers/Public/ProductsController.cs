using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.API.DTOs.Public.Products;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductPublicService _productService;
		private readonly IMapper _mapper;
		private readonly ILogger<ProductsController> _logger;

		public ProductsController(IProductPublicService productService, IMapper mapper, ILogger<ProductsController> logger)
		{
			_productService = productService;
			_mapper = mapper;
			_logger = logger;
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Detail(string id, [FromQuery] int related = 8, CancellationToken ct = default)
		{
			var entity = await _productService.GetByIdAsync(id, ct);
			if (entity == null) return NotFound();
			var detail = _mapper.Map<ProductDetailDto>(entity);
			var relatedItems = await _productService.GetRelatedAsync(id, Math.Clamp(related, 0, 20), ct);
			var relatedDtos = _mapper.Map<IReadOnlyList<ProductListItemDto>>(relatedItems);
			return Ok(new { detail, related = relatedDtos });
		}
	}
}


