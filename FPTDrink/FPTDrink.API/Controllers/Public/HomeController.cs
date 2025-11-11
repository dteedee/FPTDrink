using AutoMapper;
using FPTDrink.API.DTOs.Public.Home;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class HomeController : ControllerBase
	{
		private readonly IVisitorStatsService _service;
		private readonly IHomePageService _homePageService;
		private readonly IMapper _mapper;
		private readonly ICartService _cartService;

		public HomeController(IVisitorStatsService service, IHomePageService homePageService, IMapper mapper, ICartService cartService)
		{
			_service = service;
			_homePageService = homePageService;
			_mapper = mapper;
			_cartService = cartService;
		}

		[HttpGet("refresh")]
		public async Task<IActionResult> Refresh(CancellationToken ct)
		{
			var stats = await _service.GetVisitorStatsAsync(ct);
			return Ok(_mapper.Map<VisitorStatsDto>(stats));
		}

		[HttpGet("blocks")]
		public async Task<IActionResult> Blocks([FromQuery] int limit = 8, [FromQuery] string? cartId = null, CancellationToken ct = default)
		{
			if (limit <= 0) limit = 8;
			if (limit > 48) limit = 48;
			var blocks = await _homePageService.GetHomeBlocksAsync(limit, ct);
			
			if (!string.IsNullOrWhiteSpace(cartId))
			{
				var cart = _cartService.GetCart(cartId);
				var cartQuantities = cart.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
				
				AdjustProductQuantities(blocks.Featured, cartQuantities);
				AdjustProductQuantities(blocks.Hot, cartQuantities);
				AdjustProductQuantities(blocks.Newest, cartQuantities);
				AdjustProductQuantities(blocks.BestSale, cartQuantities);
			}
			
			var dto = new HomeBlocksDto
			{
				Featured = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Featured),
				Hot = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Hot),
				Newest = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Newest),
				BestSale = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.BestSale)
			};
			return Ok(dto);
		}
		
		private static void AdjustProductQuantities(IReadOnlyList<Product> products, Dictionary<string, int> cartQuantities)
		{
			foreach (var product in products)
			{
				if (cartQuantities.TryGetValue(product.MaSanPham, out var cartQuantity))
				{
					product.SoLuong = Math.Max(0, product.SoLuong - cartQuantity);
				}
			}
		}
	}
}


