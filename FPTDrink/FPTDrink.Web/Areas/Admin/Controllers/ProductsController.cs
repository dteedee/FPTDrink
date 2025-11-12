using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class ProductsController : AdminBaseController
	{
		private readonly IProductService _productService;
		private readonly IProductCategoryService _categoryService;
		private readonly INhaCungCapService _supplierService;
		private readonly IWebHostEnvironment _environment;

		public ProductsController(
			IProductService productService,
			IProductCategoryService categoryService,
			INhaCungCapService supplierService,
			IWebHostEnvironment environment)
		{
			_productService = productService;
			_categoryService = categoryService;
			_supplierService = supplierService;
			_environment = environment;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Index(string status = "Index", string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Quản lý sản phẩm";
			ViewBag.Status = status;
			ViewBag.Search = search;

			var items = await _productService.GetListAsync(status, search, cancellationToken);
			var model = items.Select(p => new AdminProductListItemViewModel
			{
				Id = p.MaSanPham,
				Name = p.Title,
				CategoryName = p.ProductCategory?.Title,
				SupplierName = p.Supplier?.Title,
				CostPrice = p.GiaNhap,
				ListPrice = p.GiaNiemYet,
				SalePrice = p.GiaBan ?? p.GiaNiemYet,
				DiscountPercent = p.GiamGia,
				Stock = p.SoLuong,
				IsActive = p.IsActive,
				Status = p.Status,
				Image = p.Image
			}).ToList();

			return View(model);
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Thêm sản phẩm";
			var vm = new AdminProductFormViewModel
			{
				IsActive = true,
				IsHome = false,
				IsNew = true,
				IsHot = false
			};
			await PopulateDropdownsAsync(vm, cancellationToken);
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ThemMoi", "Quản lý")]
		public async Task<IActionResult> Create(AdminProductFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				await PopulateDropdownsAsync(model, cancellationToken);
				return View(model);
			}

			try
			{
				var imagePath = await SaveImageAsync(model.UploadImage, null, cancellationToken);
				var entity = new Product
				{
					Title = model.Name,
					ProductCategoryId = model.ProductCategoryId,
					SupplierId = model.SupplierId,
					MoTa = model.Description,
					ChiTiet = model.Detail,
					GiaNhap = model.CostPrice,
					GiaNiemYet = model.ListPrice,
					GiamGia = model.DiscountPercent,
					SoLuong = model.Stock,
					IsHome = model.IsHome,
					IsHot = model.IsHot,
					IsNew = model.IsNew,
					IsActive = model.IsActive,
					Image = imagePath,
					CreatedBy = CurrentDisplayName ?? CurrentUsername ?? "Admin"
				};

				var created = await _productService.CreateAsync(entity, cancellationToken);
				TempData["SuccessMessage"] = $"Đã tạo sản phẩm {created.Title}.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				await PopulateDropdownsAsync(model, cancellationToken);
				return View(model);
			}
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
		{
			var product = await _productService.GetByIdAsync(id, cancellationToken);
			if (product == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
				return RedirectToAction(nameof(Index));
			}

			ViewData["Title"] = $"Chỉnh sửa sản phẩm - {product.Title}";
			var model = new AdminProductFormViewModel
			{
				Id = product.MaSanPham,
				Name = product.Title,
				ProductCategoryId = product.ProductCategoryId,
				SupplierId = product.SupplierId,
				Description = product.MoTa,
				Detail = product.ChiTiet,
				CostPrice = product.GiaNhap,
				ListPrice = product.GiaNiemYet,
				DiscountPercent = product.GiamGia,
				Stock = product.SoLuong,
				IsHome = product.IsHome,
				IsHot = product.IsHot,
				IsNew = product.IsNew,
				IsActive = product.IsActive,
				ExistingImage = product.Image
			};
			await PopulateDropdownsAsync(model, cancellationToken);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Edit(string id, AdminProductFormViewModel model, CancellationToken cancellationToken = default)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				await PopulateDropdownsAsync(model, cancellationToken);
				return View(model);
			}

			try
			{
				var product = await _productService.GetByIdAsync(id, cancellationToken);
				if (product == null)
				{
					TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
					return RedirectToAction(nameof(Index));
				}

				var imagePath = await SaveImageAsync(model.UploadImage, model.ExistingImage, cancellationToken);

				product.Title = model.Name;
				product.ProductCategoryId = model.ProductCategoryId;
				product.SupplierId = model.SupplierId;
				product.MoTa = model.Description;
				product.ChiTiet = model.Detail;
				product.GiaNhap = model.CostPrice;
				product.GiaNiemYet = model.ListPrice;
				product.GiamGia = model.DiscountPercent;
				product.SoLuong = model.Stock;
				product.IsHome = model.IsHome;
				product.IsHot = model.IsHot;
				product.IsNew = model.IsNew;
				product.IsActive = model.IsActive;
				product.Image = imagePath;
				product.Modifiedby = CurrentDisplayName ?? CurrentUsername ?? "Admin";

				var ok = await _productService.UpdateAsync(product, cancellationToken);
				if (!ok)
				{
					TempData["ErrorMessage"] = "Cập nhật sản phẩm thất bại.";
					return RedirectToAction(nameof(Index));
				}
				TempData["SuccessMessage"] = $"Đã cập nhật sản phẩm {product.Title}.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				await PopulateDropdownsAsync(model, cancellationToken);
				return View(model);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> ToggleActive(string id, string? status, CancellationToken cancellationToken = default)
		{
			var (_, isActive) = await _productService.ToggleActiveAsync(id, cancellationToken);
			if (isActive == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
			}
			else
			{
				TempData["SuccessMessage"] = isActive.Value
					? "Đã kích hoạt sản phẩm."
					: "Đã tạm ngưng sản phẩm.";
			}
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> Delete(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _productService.MoveToTrashAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
				? "Đã chuyển sản phẩm vào thùng rác."
				: "Không thể xoá sản phẩm.";
			return RedirectToAction(nameof(Index), new { status });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý")]
		public async Task<IActionResult> Restore(string id, string? status, CancellationToken cancellationToken = default)
		{
			var ok = await _productService.UndoAsync(id, cancellationToken);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
				? "Đã khôi phục sản phẩm."
				: "Không thể khôi phục sản phẩm.";
			return RedirectToAction(nameof(Index), new { status = status ?? "Trash" });
		}

		private async Task PopulateDropdownsAsync(AdminProductFormViewModel model, CancellationToken cancellationToken)
		{
			var categories = await _categoryService.GetListAsync("Index", null, cancellationToken);
			var suppliers = await _supplierService.GetListAsync("Index", null, cancellationToken);

			model.CategoryOptions = categories
				.Where(c => c.Status != 0)
				.Select(c => new SelectListItem
				{
					Value = c.MaLoaiSanPham,
					Text = c.Title,
					Selected = string.Equals(c.MaLoaiSanPham, model.ProductCategoryId, StringComparison.Ordinal)
				}).ToList();

			model.SupplierOptions = suppliers
				.Where(s => s.Status != 0)
				.Select(s => new SelectListItem
				{
					Value = s.MaNhaCungCap,
					Text = s.Title,
					Selected = string.Equals(s.MaNhaCungCap, model.SupplierId, StringComparison.Ordinal)
				}).ToList();
		}

		private async Task<string?> SaveImageAsync(IFormFile? upload, string? existing, CancellationToken cancellationToken)
		{
			if (upload == null || upload.Length == 0)
			{
				return existing;
			}

			var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

			var extension = Path.GetExtension(upload.FileName);
			var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
			if (!allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Định dạng ảnh không hợp lệ. Vui lòng chọn JPG, PNG, GIF hoặc WEBP.");
			}

			var fileName = $"{Guid.NewGuid():N}{extension}";
			var filePath = Path.Combine(uploadsFolder, fileName);
			await using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await upload.CopyToAsync(stream, cancellationToken);
			}

			return $"/uploads/products/{fileName}";
		}
	}
}

