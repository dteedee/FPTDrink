using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class StaffsController : AdminBaseController
	{
		private readonly INhanVienService _staffService;
		private readonly IChucVuService _roleService;

		public StaffsController(INhanVienService staffService, IChucVuService roleService)
		{
			_staffService = staffService;
			_roleService = roleService;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string status = "Index", string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Nhân viên";
			ViewBag.Status = status;
			ViewBag.Search = search;

			var items = await _staffService.GetListAsync(status, search, null, cancellationToken);
			var model = items.Select(nv => new AdminStaffListItemViewModel
			{
				Id = nv.Id,
				Username = nv.TenDangNhap,
				DisplayName = nv.TenHienThi,
				FullName = nv.FullName,
				Phone = nv.SoDienThoai,
				Email = nv.Email,
				RoleName = nv.IdChucVuNavigation?.TenChucVu ?? "Chưa phân quyền",
				IsActive = nv.IsActiveAccount,
				Status = nv.Status
			}).ToList();

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Create(CancellationToken cancellationToken)
		{
			ViewData["Title"] = "Thêm nhân viên";
			var model = new AdminStaffFormViewModel
			{
				RoleOptions = await GetRoleOptionsAsync(null, cancellationToken)
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AdminStaffFormViewModel model, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				model.RoleOptions = await GetRoleOptionsAsync(model.RoleId, cancellationToken);
				return View(model);
			}

			try
			{
				var nv = new NhanVien
				{
					TenDangNhap = model.Username,
					FullName = model.FullName,
					TenHienThi = string.IsNullOrWhiteSpace(model.DisplayName) ? model.FullName : model.DisplayName,
					SoDienThoai = model.Phone,
					Email = model.Email,
					DiaChi = model.Address,
					NgaySinh = model.BirthDate ?? new DateTime(1990, 1, 1),
					GioiTinh = model.Gender,
					IdChucVu = model.RoleId,
					IsActiveAccount = model.IsActive,
					Status = model.IsActive ? 1 : 0,
					CreatedBy = CurrentDisplayName ?? CurrentUsername ?? "Admin",
					CreatedDate = DateTime.Now,
					MatKhau = string.IsNullOrWhiteSpace(model.Password) ? "123456" : model.Password
				};
				await _staffService.CreateAsync(nv, cancellationToken);
				TempData["SuccessMessage"] = "Đã tạo tài khoản nhân viên.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				model.RoleOptions = await GetRoleOptionsAsync(model.RoleId, cancellationToken);
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
		{
			var nv = await _staffService.GetByIdAsync(id, cancellationToken);
			if (nv == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy nhân viên.";
				return RedirectToAction(nameof(Index));
			}

			var model = new AdminStaffFormViewModel
			{
				Id = nv.Id,
				Username = nv.TenDangNhap,
				FullName = nv.FullName,
				DisplayName = nv.TenHienThi,
				Phone = nv.SoDienThoai,
				Email = nv.Email,
				Address = nv.DiaChi,
				BirthDate = nv.NgaySinh != default ? nv.NgaySinh : null,
				Gender = nv.GioiTinh,
				RoleId = nv.IdChucVu,
				IsActive = nv.IsActiveAccount,
				RoleOptions = await GetRoleOptionsAsync(nv.IdChucVu, cancellationToken)
			};
			ViewData["Title"] = $"Chỉnh sửa - {nv.TenHienThi}";
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, AdminStaffFormViewModel model, CancellationToken cancellationToken)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				model.RoleOptions = await GetRoleOptionsAsync(model.RoleId, cancellationToken);
				return View(model);
			}

			var nv = await _staffService.GetByIdAsync(id, cancellationToken);
			if (nv == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy nhân viên.";
				return RedirectToAction(nameof(Index));
			}

			nv.FullName = model.FullName;
			nv.TenHienThi = string.IsNullOrWhiteSpace(model.DisplayName) ? model.FullName : model.DisplayName;
			nv.SoDienThoai = model.Phone;
			nv.Email = model.Email;
			nv.DiaChi = model.Address;
			nv.NgaySinh = model.BirthDate ?? nv.NgaySinh;
			nv.GioiTinh = model.Gender;
			nv.IdChucVu = model.RoleId;
			nv.IsActiveAccount = model.IsActive;
			nv.Status = model.IsActive ? 1 : 0;
			nv.Modifiedby = CurrentDisplayName ?? CurrentUsername ?? "Admin";
			nv.ModifiedDate = DateTime.Now;

			var ok = await _staffService.UpdateAsync(nv, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật thông tin nhân viên." : "Không thể cập nhật nhân viên.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(string id, string? status, CancellationToken cancellationToken)
		{
			var newPassword = Guid.NewGuid().ToString("N").Substring(0, 8);
			var ok = await _staffService.ResetPasswordAsync(id, newPassword, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
				? $"Đã đặt lại mật khẩu mới: {newPassword}"
				: "Không thể đặt lại mật khẩu.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id, string? status, CancellationToken cancellationToken)
		{
			var ok = await _staffService.MoveToTrashAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã khóa tài khoản." : "Không thể khóa tài khoản.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Restore(string id, string? status, CancellationToken cancellationToken)
		{
			var ok = await _staffService.UndoAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã mở khóa tài khoản." : "Không thể mở khóa.";
			return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
		}

		private async Task<IReadOnlyList<SelectListItem>> GetRoleOptionsAsync(int? selectedId, CancellationToken cancellationToken)
		{
			var roles = await _roleService.GetListAsync("Index", null, cancellationToken);
			return roles.Select(r => new SelectListItem
			{
				Value = r.Id.ToString(),
				Text = r.TenChucVu,
				Selected = selectedId.HasValue && r.Id == selectedId.Value
			}).ToList();
		}
	}
}

