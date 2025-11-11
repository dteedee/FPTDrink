using AutoMapper;
using FPTDrink.API.DTOs.Public.Home;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class HomeController : ControllerBase
	{
		private readonly IVisitorStatsService _service;
		private readonly IHomePageService _homePageService;
		private readonly IMapper _mapper;

		public HomeController(IVisitorStatsService service, IHomePageService homePageService, IMapper mapper)
		{
			_service = service;
			_homePageService = homePageService;
			_mapper = mapper;
		}

		[HttpGet("refresh")]
		public async Task<IActionResult> Refresh(CancellationToken ct)
		{
			var stats = await _service.GetVisitorStatsAsync(ct);
			return Ok(_mapper.Map<VisitorStatsDto>(stats));
		}

		[HttpGet("blocks")]
		public async Task<IActionResult> Blocks([FromQuery] int limit = 8, CancellationToken ct = default)
		{
			if (limit <= 0) limit = 8;
			if (limit > 48) limit = 48;
			var blocks = await _homePageService.GetHomeBlocksAsync(limit, ct);
			var dto = new HomeBlocksDto
			{
				Featured = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Featured),
				Hot = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Hot),
				Newest = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.Newest),
				BestSale = _mapper.Map<IReadOnlyList<ProductListItemDto>>(blocks.BestSale)
			};
			return Ok(dto);
		}
	}
}


