using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class ProfileController : AdminBaseController
	{
		private readonly INhanVienService _staffService;
		private readonly IChucVuService _roleService;

		public ProfileController(INhanVienService staffService, IChucVuService roleService)
		{
			_staffService = staffService;
			_roleService = roleService;
		}

		[HttpGet]
		public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
		{
			var userId = CurrentUserId;
			if (string.IsNullOrWhiteSpace(userId))
			{
				TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
				return RedirectToAction("Index", "Dashboard");
			}

			var staff = await _staffService.GetByIdAsync(userId, cancellationToken);
			if (staff == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
				return RedirectToAction("Index", "Dashboard");
			}

			ViewData["Title"] = "Thông tin tài khoản";
			var model = new AdminProfileViewModel
			{
				Id = staff.Id,
				Username = staff.TenDangNhap,
				FullName = staff.FullName,
				DisplayName = staff.TenHienThi ?? staff.FullName,
				Phone = staff.SoDienThoai ?? string.Empty,
				Email = staff.Email ?? string.Empty,
				Address = staff.DiaChi ?? string.Empty,
				BirthDate = staff.NgaySinh != default ? staff.NgaySinh : null,
				Gender = staff.GioiTinh,
				RoleName = staff.IdChucVuNavigation?.TenChucVu ?? "Chưa phân quyền"
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(AdminProfileViewModel model, CancellationToken cancellationToken = default)
		{
			var userId = CurrentUserId;
			if (string.IsNullOrWhiteSpace(userId) || userId != model.Id)
			{
				TempData["ErrorMessage"] = "Không có quyền chỉnh sửa thông tin này.";
				return RedirectToAction("Index", "Dashboard");
			}

			if (!ModelState.IsValid)
			{
				var staff = await _staffService.GetByIdAsync(userId, cancellationToken);
				if (staff != null)
				{
					model.RoleName = staff.IdChucVuNavigation?.TenChucVu ?? "Chưa phân quyền";
				}
				return View(model);
			}

			try
			{
				var staff = await _staffService.GetByIdAsync(userId, cancellationToken);
				if (staff == null)
				{
					TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
					return RedirectToAction("Index", "Dashboard");
				}

				staff.FullName = model.FullName;
				staff.TenHienThi = string.IsNullOrWhiteSpace(model.DisplayName) ? model.FullName : model.DisplayName;
				staff.SoDienThoai = model.Phone;
				staff.Email = model.Email;
				staff.DiaChi = model.Address;
				staff.NgaySinh = model.BirthDate ?? staff.NgaySinh;
				staff.GioiTinh = model.Gender;
				staff.Modifiedby = CurrentDisplayName ?? CurrentUsername ?? "Admin";
				staff.ModifiedDate = DateTime.Now;

				// Update password if provided
				if (!string.IsNullOrWhiteSpace(model.NewPassword))
				{
					var passwordUpdated = await _staffService.ResetPasswordAsync(userId, model.NewPassword, cancellationToken);
					if (!passwordUpdated)
					{
						ModelState.AddModelError(string.Empty, "Không thể cập nhật mật khẩu.");
						model.RoleName = staff.IdChucVuNavigation?.TenChucVu ?? "Chưa phân quyền";
						return View(model);
					}
				}

				var ok = await _staffService.UpdateAsync(staff, cancellationToken);
				if (!ok)
				{
					TempData["ErrorMessage"] = "Cập nhật thông tin thất bại.";
					return RedirectToAction(nameof(Index));
				}

				// Update session
				HttpContext.Session.SetString(AdminSessionKeys.DisplayName, staff.TenHienThi ?? staff.FullName);
				HttpContext.Session.SetString(AdminSessionKeys.FullName, staff.FullName ?? string.Empty);

				TempData["SuccessMessage"] = "Đã cập nhật thông tin tài khoản thành công.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				var staff = await _staffService.GetByIdAsync(userId, cancellationToken);
				if (staff != null)
				{
					model.RoleName = staff.IdChucVuNavigation?.TenChucVu ?? "Chưa phân quyền";
				}
				return View(model);
			}
		}
	}
}

