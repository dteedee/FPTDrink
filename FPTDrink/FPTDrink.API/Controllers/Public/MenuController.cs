using FPTDrink.API.DTOs.Public.Menu;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class MenuController : ControllerBase
	{
		private readonly IMenuQueryService _service;

		public MenuController(IMenuQueryService service)
		{
			_service = service;
		}

		[HttpGet("top")]
		public async Task<IActionResult> MenuTop(CancellationToken ct)
		{
			var items = await _service.GetTopMenuAsync(ct);
			return Ok(items.Select(x => new MenuCategoryDto { Id = x.Id, Title = x.Title, Alias = x.Alias, Position = x.Position }));
		}

		[HttpGet("product-categories")]
		public async Task<IActionResult> MenuProductCategory(CancellationToken ct)
		{
			var items = await _service.GetProductCategoriesAsync(ct);
			return Ok(items.Select(x => new MenuProductCategoryDto { MaLoaiSanPham = x.MaLoaiSanPham, Title = x.Title, Alias = x.Alias }));
		}

		[HttpGet("left")]
		public async Task<IActionResult> MenuLeft([FromQuery] string? id, CancellationToken ct)
		{
			var items = await _service.GetProductCategoriesAsync(ct);
			return Ok(new
			{
				selected = id,
				items = items.Select(x => new MenuProductCategoryDto { MaLoaiSanPham = x.MaLoaiSanPham, Title = x.Title, Alias = x.Alias })
			});
		}

		[HttpGet("suppliers")]
		public async Task<IActionResult> MenuSupplier([FromQuery] string? id, CancellationToken ct)
		{
			var items = await _service.GetSuppliersAsync(ct);
			return Ok(new
			{
				selected = id,
				items = items.Select(x => new MenuSupplierDto { MaNhaCungCap = x.MaNhaCungCap, Title = x.Title })
			});
		}

		[HttpGet("arrivals")]
		public async Task<IActionResult> MenuArrivals(CancellationToken ct)
		{
			var items = await _service.GetProductCategoriesAsync(ct);
			return Ok(items.Select(x => new MenuProductCategoryDto { MaLoaiSanPham = x.MaLoaiSanPham, Title = x.Title, Alias = x.Alias }));
		}
	}
}


