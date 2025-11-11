using AutoMapper;
using FPTDrink.API.DTOs.Admin.ChucVu;
using FPTDrink.API.DTOs.Common;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class ChucVuController : ControllerBase
	{
		private readonly IChucVuService _service;
		private readonly IMapper _mapper;

		public ChucVuController(IChucVuService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		// GET: api/admin/chucvu?status=Index|Trash|All&search=...
		[HttpGet]
		public async Task<IActionResult> GetList([FromQuery] string status = "All", [FromQuery] string? search = null, CancellationToken ct = default)
		{
			var data = await _service.GetListAsync(status, search, ct);
			return Ok(_mapper.Map<IReadOnlyList<ChucVuDto>>(data));
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> Get(int id, CancellationToken ct = default)
		{
			var item = await _service.GetByIdAsync(id, ct);
			if (item == null) return NotFound();
			return Ok(_mapper.Map<ChucVuDto>(item));
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ChucVuCreateRequest request, CancellationToken ct = default)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var entity = _mapper.Map<ChucVu>(request);
			var created = await _service.CreateAsync(entity, ct);
			return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<ChucVuDto>(created));
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] ChucVuUpdateRequest request, CancellationToken ct = default)
		{
			if (id != request.Id) return BadRequest("Id không khớp");
			var entity = _mapper.Map<ChucVu>(request);
			var ok = await _service.UpdateAsync(entity, ct);
			return ok ? NoContent() : NotFound();
		}

		[HttpPost("{id:int}/trash")]
		public async Task<IActionResult> Trash(int id, CancellationToken ct = default)
		{
			var ok = await _service.MoveToTrashAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("trash-bulk")]
		public async Task<IActionResult> TrashBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.MoveToTrashBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id:int}/delete")]
		public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
		{
			var ok = await _service.DeleteAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("delete-bulk")]
		public async Task<IActionResult> DeleteBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.DeleteBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id:int}/undo")]
		public async Task<IActionResult> Undo(int id, CancellationToken ct = default)
		{
			var ok = await _service.UndoAsync(id, ct);
			return ok ? Ok(new { success = true }) : NotFound();
		}

		[HttpPost("undo-bulk")]
		public async Task<IActionResult> UndoBulk([FromBody] IdsRequest req, CancellationToken ct = default)
		{
			var affected = await _service.UndoBulkAsync(req.Ids, ct);
			return Ok(new { success = true, affected });
		}

		[HttpPost("{id:int}/permissions/toggle")]
		public async Task<IActionResult> TogglePermission(int id, [FromBody] TogglePermissionRequest req, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(req.MaChucNang)) return BadRequest("Mã chức năng rỗng");
			var granted = await _service.TogglePermissionAsync(id, req.MaChucNang, ct);
			return Ok(new { success = true, granted });
		}
	}
}


