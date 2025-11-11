using AutoMapper;
using FPTDrink.API.Authorization;
using FPTDrink.API.DTOs.Admin.ProductCategory;
using FPTDrink.API.DTOs.Common;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class ProductCategoryController : ControllerBase
	{
		private readonly IProductCategoryService _service;
		private readonly IMapper _mapper;

		public ProductCategoryController(IProductCategoryService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> GetList([FromQuery] string status = "All", [FromQuery] string? search = null, CancellationToken ct = default)
		{
			var data = await _service.GetListAsync(status, search, ct);
			return Ok(_mapper.Map<IReadOnlyList<ProductCategoryDto>>(data));
		}

		[HttpGet("{id}")]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Get(string id, CancellationToken ct = default)
		{
			var item = await _service.GetByIdAsync(id, ct);
			if (item == null) return NotFound();
			return Ok(_mapper.Map<ProductCategoryDto>(item));
		}

		[HttpPost]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create([FromBody] ProductCategoryCreateRequest request, CancellationToken ct = default)
		{
			var entity = _mapper.Map<ProductCategory>(request);
			var created = await _service.CreateAsync(entity, ct);
			return CreatedAtAction(nameof(Get), new { id = created.MaLoaiSanPham }, _mapper.Map<ProductCategoryDto>(created));
		}

		[HttpPut("{id}")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Update(string id, [FromBody] ProductCategoryUpdateRequest request, CancellationToken ct = default)
		{
			if (id != request.MaLoaiSanPham) return BadRequest("Id không khớp");
			var entity = _mapper.Map<ProductCategory>(request);
			var ok = await _service.UpdateAsync(entity, ct);
			return ok ? NoContent() : NotFound();
		}

		[HttpPost("{id}/trash")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Trash(string id, CancellationToken ct = default)
		{
			var ok = await _service.MoveToTrashAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("trash-bulk")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> TrashBulk([FromBody] StringIdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.MoveToTrashBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id}/delete")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
		{
			var ok = await _service.DeleteAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("delete-bulk")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> DeleteBulk([FromBody] StringIdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.DeleteBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id}/undo")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Undo(string id, CancellationToken ct = default)
		{
			var ok = await _service.UndoAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("undo-bulk")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> UndoBulk([FromBody] StringIdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.UndoBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id}/toggle-active")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> ToggleActive(string id, CancellationToken ct = default)
		{
			var result = await _service.ToggleActiveAsync(id, ct);
			if (!result.success) return NotFound();
			return Ok(new { success = true, isActive = result.isActive });
		}
	}
}


