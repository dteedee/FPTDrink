using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class RolesController : AdminBaseController
	{
		private readonly IChucVuService _roleService;
		private readonly IPhanQuyenRepository _permissionRepository;

		public RolesController(IChucVuService roleService, IPhanQuyenRepository permissionRepository)
		{
			_roleService = roleService;
			_permissionRepository = permissionRepository;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Index(string status = "Index", string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Chức vụ & phân quyền";
			ViewBag.Status = status;
			ViewBag.Search = search;

			var roles = await _roleService.GetListAsync(status, search, cancellationToken);
			var model = roles.Select(r => new AdminRoleViewModel
			{
				Id = r.Id,
				Title = r.TenChucVu,
				Description = r.MoTa,
				Status = r.Status
			}).ToList();
			return View(model);
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public IActionResult Create()
		{
			ViewData["Title"] = "Thêm chức vụ";
			return View(new AdminRoleFormViewModel { IsActive = true });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create(AdminRoleFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var entity = new ChucVu
			{
				TenChucVu = model.Title,
				MoTa = model.Description,
				Status = model.IsActive ? 1 : 0
			};

			await _roleService.CreateAsync(entity, cancellationToken);
			TempData["SuccessMessage"] = "Đã tạo chức vụ mới.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index));
			}

			// Không cho phép chỉnh sửa chức vụ "Quản lý"
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Không thể chỉnh sửa chức vụ 'Quản lý'. Chức vụ này mặc định có toàn quyền.";
				return RedirectToAction(nameof(Index));
			}

			ViewData["Title"] = $"Chỉnh sửa chức vụ - {role.TenChucVu}";
			var model = new AdminRoleFormViewModel
			{
				Id = role.Id,
				Title = role.TenChucVu,
				Description = role.MoTa,
				IsActive = role.Status != 0
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Edit(int id, AdminRoleFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index));
			}

			// Không cho phép chỉnh sửa chức vụ "Quản lý"
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Không thể chỉnh sửa chức vụ 'Quản lý'. Chức vụ này mặc định có toàn quyền.";
				return RedirectToAction(nameof(Index));
			}

			role.TenChucVu = model.Title;
			role.MoTa = model.Description;
			role.Status = model.IsActive ? 1 : 0;

			var ok = await _roleService.UpdateAsync(role, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật chức vụ." : "Không thể cập nhật chức vụ.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Delete(int id, string? status, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index), new { status });
			}

			// Không cho phép xóa chức vụ "Quản lý"
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Không thể xóa chức vụ 'Quản lý'. Chức vụ này mặc định có toàn quyền.";
				return RedirectToAction(nameof(Index), new { status });
			}

			var ok = await _roleService.MoveToTrashAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã chuyển chức vụ vào thùng rác." : "Không thể chuyển chức vụ.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Restore(int id, string? status, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
			}

			// Không cho phép khôi phục chức vụ "Quản lý" (vì nó không bao giờ bị xóa)
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Chức vụ 'Quản lý' luôn hoạt động và không thể bị xóa.";
				return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
			}

			var ok = await _roleService.UndoAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã khôi phục chức vụ." : "Không thể khôi phục chức vụ.";
			return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> ToggleActive(int id, string? status, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index), new { status });
			}

			// Không cho phép thay đổi trạng thái chức vụ "Quản lý"
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Không thể thay đổi trạng thái chức vụ 'Quản lý'. Chức vụ này luôn hoạt động.";
				return RedirectToAction(nameof(Index), new { status });
			}

			role.Status = role.Status == 0 ? 1 : 0;
			var ok = await _roleService.UpdateAsync(role, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật trạng thái chức vụ." : "Không thể cập nhật trạng thái.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Permissions(int id, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(id, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index));
			}

			var allFeatures = await _permissionRepository.Query()
				.Include(p => p.MaChucNangNavigation)
				.Select(p => p.MaChucNangNavigation)
				.Distinct()
				.OrderBy(p => p.MaChucNang)
				.ToListAsync(cancellationToken);

			var grantedCodes = await _permissionRepository.Query()
				.Where(p => p.IdchucVu == id)
				.Select(p => p.MaChucNang)
				.ToListAsync(cancellationToken);

			ViewBag.Role = role;
			ViewBag.Granted = grantedCodes;
			return View(allFeatures);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> TogglePermission(int roleId, string featureCode, CancellationToken cancellationToken = default)
		{
			var role = await _roleService.GetByIdAsync(roleId, cancellationToken);
			if (role == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy chức vụ.";
				return RedirectToAction(nameof(Index));
			}

			// Không cho phép thay đổi quyền của chức vụ "Quản lý"
			if (string.Equals(role.TenChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				TempData["ErrorMessage"] = "Không thể thay đổi quyền của chức vụ 'Quản lý'. Chức vụ này mặc định có toàn quyền.";
				return RedirectToAction(nameof(Permissions), new { id = roleId });
			}

			var ok = await _roleService.TogglePermissionAsync(roleId, featureCode, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật phân quyền." : "Không thể cập nhật phân quyền.";
			return RedirectToAction(nameof(Permissions), new { id = roleId });
		}
	}
}

