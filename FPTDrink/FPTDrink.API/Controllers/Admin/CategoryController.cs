using AutoMapper;
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

		// GET: api/admin/category?status=Index|Trash|All
		[HttpGet]
		public async Task<IActionResult> GetList([FromQuery] string status = "All", CancellationToken ct = default)
		{
			var data = await _service.GetListAsync(status, ct);
			var result = _mapper.Map<IReadOnlyList<CategoryDto>>(data);
			return Ok(result);
		}

		// GET: api/admin/category/5
		[HttpGet("{id:int}")]
		public async Task<IActionResult> Get(int id, CancellationToken ct = default)
		{
			var item = await _service.GetByIdAsync(id, ct);
			if (item == null) return NotFound();
			return Ok(_mapper.Map<CategoryDto>(item));
		}

		// POST: api/admin/category
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request, CancellationToken ct = default)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var entity = _mapper.Map<Category>(request);
			var created = await _service.CreateAsync(entity, ct);
			return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<CategoryDto>(created));
		}

		// PUT: api/admin/category/5
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateRequest request, CancellationToken ct = default)
		{
			if (id != request.Id) return BadRequest("Id không khớp");
			var entity = _mapper.Map<Category>(request);
			var ok = await _service.UpdateAsync(entity, ct);
			return ok ? NoContent() : NotFound();
		}

		// POST: api/admin/category/5/trash
		[HttpPost("{id:int}/trash")]
		public async Task<IActionResult> Trash(int id, CancellationToken ct = default)
		{
			var ok = await _service.MoveToTrashAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		// POST: api/admin/category/trash-bulk
		[HttpPost("trash-bulk")]
		public async Task<IActionResult> TrashBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.MoveToTrashBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		// POST: api/admin/category/5/undo
		[HttpPost("{id:int}/undo")]
		public async Task<IActionResult> Undo(int id, CancellationToken ct = default)
		{
			var ok = await _service.UndoAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		// POST: api/admin/category/undo-bulk
		[HttpPost("undo-bulk")]
		public async Task<IActionResult> UndoBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.UndoBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		// POST: api/admin/category/5/toggle-active
		[HttpPost("{id:int}/toggle-active")]
		public async Task<IActionResult> ToggleActive(int id, CancellationToken ct = default)
		{
			var result = await _service.ToggleActiveAsync(id, ct);
			if (!result.success) return NotFound();
			return Ok(new { success = true, isActive = result.isActive });
		}
	}
}


