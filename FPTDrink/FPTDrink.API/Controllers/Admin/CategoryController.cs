using AutoMapper;
using FPTDrink.API.Authorization;
using FPTDrink.API.DTOs.Admin.Category;
using FPTDrink.API.DTOs.Common;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _service;
		private readonly IMapper _mapper;

		public CategoryController(ICategoryService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> GetList([FromQuery] string status = "All", CancellationToken ct = default)
		{
			var data = await _service.GetListAsync(status, ct);
			var result = _mapper.Map<IReadOnlyList<CategoryDto>>(data);
			return Ok(result);
		}

		[HttpGet("{id:int}")]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Get(int id, CancellationToken ct = default)
		{
			var item = await _service.GetByIdAsync(id, ct);
			if (item == null) return NotFound();
			return Ok(_mapper.Map<CategoryDto>(item));
		}

		[HttpPost]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request, CancellationToken ct = default)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var entity = _mapper.Map<Category>(request);
			var created = await _service.CreateAsync(entity, ct);
			return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<CategoryDto>(created));
		}

		[HttpPut("{id:int}")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateRequest request, CancellationToken ct = default)
		{
			if (id != request.Id) return BadRequest("Id không khớp");
			var entity = _mapper.Map<Category>(request);
			var ok = await _service.UpdateAsync(entity, ct);
			return ok ? NoContent() : NotFound();
		}

		[HttpPost("{id:int}/trash")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Trash(int id, CancellationToken ct = default)
		{
			var ok = await _service.MoveToTrashAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("trash-bulk")]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> TrashBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.MoveToTrashBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id:int}/undo")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Undo(int id, CancellationToken ct = default)
		{
			var ok = await _service.UndoAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("undo-bulk")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> UndoBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.UndoBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id:int}/toggle-active")]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> ToggleActive(int id, CancellationToken ct = default)
		{
			var result = await _service.ToggleActiveAsync(id, ct);
			if (!result.success) return NotFound();
			return Ok(new { success = true, isActive = result.isActive });
		}
	}
}


