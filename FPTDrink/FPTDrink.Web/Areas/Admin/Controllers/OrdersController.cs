using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class OrdersController : AdminBaseController
	{
		private readonly IOrderService _orderService;

		public OrdersController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
		{
			ViewData["Title"] = "Quản lý đơn hàng";
			ViewBag.Search = search;
			var orders = await _orderService.GetListAsync(search, cancellationToken);
			var model = new List<AdminOrderListItemViewModel>();
			foreach (var order in orders)
			{
				var items = await _orderService.GetItemsAsync(order.MaHoaDon, cancellationToken);
				var total = items.Sum(x => x.SoLuong * x.GiaBan);
				model.Add(new AdminOrderListItemViewModel
				{
					OrderCode = order.MaHoaDon,
					CustomerName = order.TenKhachHang,
					Phone = order.SoDienThoai,
					Email = order.Email,
					CreatedDate = order.CreatedDate,
					Status = order.TrangThai,
					PaymentMethod = order.PhuongThucThanhToan,
					TotalAmount = total
				});
			}
			return View(model);
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemChiTiet", "Quản lý", "Kế toán", "Thu ngân")]
		public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
		{
			var order = await _orderService.GetByIdAsync(id, cancellationToken);
			if (order == null)
			{
				TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
				return RedirectToAction(nameof(Index));
			}

			var items = await _orderService.GetItemsAsync(id, cancellationToken);
			var model = new AdminOrderDetailViewModel
			{
				OrderCode = order.MaHoaDon,
				CustomerName = order.TenKhachHang,
				Phone = order.SoDienThoai,
				Email = order.Email,
				Address = order.DiaChi ?? string.Empty,
				PaymentMethod = order.PhuongThucThanhToan,
				Status = order.TrangThai,
				CreatedDate = order.CreatedDate,
				UpdatedDate = order.ModifiedDate,
				TotalAmount = order.ChiTietHoaDons.Sum(x => x.SoLuong * x.GiaBan),
				Items = items.Select(x => new AdminOrderItemViewModel
				{
					ProductId = x.ProductId,
					ProductTitle = x.Product?.Title,
					Quantity = x.SoLuong,
					UnitPrice = x.GiaBan,
					Discount = x.GiamGia
				}).ToList()
			};

			ViewData["Title"] = $"Chi tiết đơn hàng - {order.MaHoaDon}";
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_ChinhSua", "Quản lý", "Thu ngân")]
		public async Task<IActionResult> UpdateStatus(AdminOrderStatusUpdateViewModel model, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessage"] = "Vui lòng chọn trạng thái hợp lệ.";
				return RedirectToAction(nameof(Details), new { id = model.OrderCode });
			}

			var (success, message) = await _orderService.UpdateStatusAsync(model.OrderCode, model.NewStatus, false, cancellationToken);
			TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
			return RedirectToAction(nameof(Details), new { id = model.OrderCode });
		}

	}
}

