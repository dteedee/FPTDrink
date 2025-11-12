using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class ProductCategoriesController : AdminBaseController
	{
		private readonly IProductCategoryService _categoryService;

		public ProductCategoriesController(IProductCategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string status = "Index", string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Loại sản phẩm";
			ViewBag.Status = status;
			ViewBag.Search = search;
			var categories = await _categoryService.GetListAsync(status, search, cancellationToken);
			var model = categories.Select(c => new AdminProductCategoryListItemViewModel
			{
				Id = c.MaLoaiSanPham,
				Title = c.Title,
				Description = c.MoTa,
				IsActive = c.IsActive,
				Status = c.Status,
				SeoTitle = c.SeoTitle,
				SeoKeywords = c.SeoKeywords
			}).ToList();
			return View(model);
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewData["Title"] = "Thêm loại sản phẩm";
			return View(new AdminProductCategoryFormViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AdminProductCategoryFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var entity = new ProductCategory
				{
					Title = model.Title,
					MoTa = model.Description,
					IsActive = model.IsActive,
					SeoTitle = model.SeoTitle,
					SeoKeywords = model.SeoKeywords,
					SeoDescription = model.SeoDescription,
					CreatedBy = CurrentDisplayName ?? CurrentUsername ?? "Admin"
				};
				await _categoryService.CreateAsync(entity, cancellationToken);
				TempData["SuccessMessage"] = "Đã tạo loại sản phẩm mới.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
		{
			var category = await _categoryService.GetByIdAsync(id, cancellationToken);
			if (category == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy loại sản phẩm.";
				return RedirectToAction(nameof(Index));
			}

			var model = new AdminProductCategoryFormViewModel
			{
				Id = category.MaLoaiSanPham,
				Title = category.Title,
				Description = category.MoTa,
				IsActive = category.IsActive,
				SeoTitle = category.SeoTitle,
				SeoKeywords = category.SeoKeywords,
				SeoDescription = category.SeoDescription
			};
			ViewData["Title"] = $"Chỉnh sửa loại - {category.Title}";
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, AdminProductCategoryFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var existing = await _categoryService.GetByIdAsync(id, cancellationToken);
			if (existing == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy loại sản phẩm.";
				return RedirectToAction(nameof(Index));
			}

			existing.Title = model.Title;
			existing.MoTa = model.Description;
			existing.IsActive = model.IsActive;
			existing.SeoTitle = model.SeoTitle;
			existing.SeoKeywords = model.SeoKeywords;
			existing.SeoDescription = model.SeoDescription;
			existing.Modifiedby = CurrentDisplayName ?? CurrentUsername ?? "Admin";
			var ok = await _categoryService.UpdateAsync(existing, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã cập nhật loại sản phẩm." : "Không thể cập nhật loại sản phẩm.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleActive(string id, string? status, CancellationToken cancellationToken = default)
		{
			var (_, isActive) = await _categoryService.ToggleActiveAsync(id, cancellationToken);
			if (isActive == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy loại sản phẩm.";
			}
			else
			{
				TempData["SuccessMessage"] = isActive.Value ? "Đã bật loại sản phẩm." : "Đã tắt loại sản phẩm.";
			}
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _categoryService.MoveToTrashAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã chuyển loại sản phẩm vào thùng rác." : "Không thể chuyển loại sản phẩm.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Restore(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _categoryService.UndoAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã khôi phục loại sản phẩm." : "Không thể khôi phục loại sản phẩm.";
			return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
		}
	}
}

