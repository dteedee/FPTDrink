using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.API.DTOs.Public.Search;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class SearchController : ControllerBase
	{
		private readonly ISearchService _service;
		private readonly IMapper _mapper;

		public SearchController(ISearchService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet("suggest")]
		public async Task<IActionResult> Suggest([FromQuery] SuggestQuery query, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var items = await _service.SuggestAsync(query.Q, query.Limit, ct);
			return Ok(items);
		}

		[HttpGet]
		public async Task<IActionResult> Search([FromQuery] SearchQuery query, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var items = await _service.SearchAsync(query.Q, ct);
			return Ok(_mapper.Map<IReadOnlyList<ProductListItemDto>>(items));
		}
	}
}


