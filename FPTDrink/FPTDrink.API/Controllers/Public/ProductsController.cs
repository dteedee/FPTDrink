using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.API.DTOs.Public.Products;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductPublicService _productService;
		private readonly IMapper _mapper;
		private readonly ILogger<ProductsController> _logger;
		private readonly ICartService _cartService;

		public ProductsController(IProductPublicService productService, IMapper mapper, ILogger<ProductsController> logger, ICartService cartService)
		{
			_productService = productService;
			_mapper = mapper;
			_logger = logger;
			_cartService = cartService;
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Detail(string id, [FromQuery] int related = 8, [FromQuery] string? cartId = null, CancellationToken ct = default)
		{
			var entity = await _productService.GetByIdAsync(id, ct);
			if (entity == null) return NotFound();
			
			if (!string.IsNullOrWhiteSpace(cartId))
			{
				var cart = _cartService.GetCart(cartId);
				var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == id);
				if (cartItem != null)
				{
					entity.SoLuong = Math.Max(0, entity.SoLuong - cartItem.Quantity);
				}
			}
			
			var detail = _mapper.Map<ProductDetailDto>(entity);
			var relatedItems = await _productService.GetRelatedAsync(id, Math.Clamp(related, 0, 20), ct);
			
			if (!string.IsNullOrWhiteSpace(cartId))
			{
				var cart = _cartService.GetCart(cartId);
				var cartQuantities = cart.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
				foreach (var product in relatedItems)
				{
					if (cartQuantities.TryGetValue(product.MaSanPham, out var cartQuantity))
					{
						product.SoLuong = Math.Max(0, product.SoLuong - cartQuantity);
					}
				}
			}
			
			var relatedDtos = _mapper.Map<IReadOnlyList<ProductListItemDto>>(relatedItems);
			return Ok(new { detail, related = relatedDtos });
		}
	}
}


