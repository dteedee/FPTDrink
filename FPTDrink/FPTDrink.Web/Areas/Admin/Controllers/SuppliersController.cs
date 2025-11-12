using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class SuppliersController : AdminBaseController
	{
		private readonly INhaCungCapService _supplierService;

		public SuppliersController(INhaCungCapService supplierService)
		{
			_supplierService = supplierService;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Index(string status = "Index", string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Nhà cung cấp";
			ViewBag.Status = status;
			ViewBag.Search = search;
			var suppliers = await _supplierService.GetListAsync(status, search, cancellationToken);
			var model = suppliers.Select(s => new AdminSupplierListItemViewModel
			{
				Id = s.MaNhaCungCap,
				Title = s.Title,
				Phone = s.SoDienThoai,
				Email = s.Email,
				Image = s.Image,
				IsActive = s.Status != 0,
				Status = s.Status
			}).ToList();
			return View(model);
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public IActionResult Create()
		{
			ViewData["Title"] = "Thêm nhà cung cấp";
			return View(new AdminSupplierFormViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create(AdminSupplierFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var entity = new NhaCungCap
				{
					Title = model.Title,
					SoDienThoai = model.Phone,
					Email = model.Email,
					Status = model.IsActive ? 1 : 0,
					CreatedBy = CurrentDisplayName ?? CurrentUsername ?? "Admin"
				};
				await _supplierService.CreateAsync(entity, cancellationToken);
				TempData["SuccessMessage"] = "Đã thêm nhà cung cấp mới.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(model);
			}
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
		{
			var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
			if (supplier == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy nhà cung cấp.";
				return RedirectToAction(nameof(Index));
			}

			var model = new AdminSupplierFormViewModel
			{
				Id = supplier.MaNhaCungCap,
				Title = supplier.Title,
				Phone = supplier.SoDienThoai,
				Email = supplier.Email,
				IsActive = supplier.Status != 0
			};
			ViewData["Title"] = $"Chỉnh sửa - {supplier.Title}";
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Edit(string id, AdminSupplierFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
			if (supplier == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy nhà cung cấp.";
				return RedirectToAction(nameof(Index));
			}

			supplier.Title = model.Title;
			supplier.SoDienThoai = model.Phone;
			supplier.Email = model.Email;
			supplier.Status = model.IsActive ? 1 : 0;
			supplier.Modifiedby = CurrentDisplayName ?? CurrentUsername ?? "Admin";

			var ok = await _supplierService.UpdateAsync(supplier, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật nhà cung cấp." : "Không thể cập nhật nhà cung cấp.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Delete(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _supplierService.MoveToTrashAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã chuyển vào thùng rác." : "Không thể chuyển nhà cung cấp.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Restore(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _supplierService.UndoAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã khôi phục nhà cung cấp." : "Không thể khôi phục nhà cung cấp.";
			return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
		}
	}
}

