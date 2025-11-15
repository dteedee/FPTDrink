using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	public class CustomersController : AdminBaseController
	{
		private readonly IKhachHangRepository _customerRepository;
		private readonly IHoaDonRepository _orderRepository;

		public CustomersController(IKhachHangRepository customerRepository, IHoaDonRepository orderRepository)
		{
			_customerRepository = customerRepository;
			_orderRepository = orderRepository;
		}

		[HttpGet]
		[PermissionAuthorize("FPTDrink_XemDanhSach", "Quản lý", "Kế toán")]
		public async Task<IActionResult> Index(string? search = null, CancellationToken cancellationToken = default)
		{
			ViewData["Title"] = "Khách hàng";
			ViewBag.Search = search;

			var query = _customerRepository.Query();

			// Filter by search
			if (!string.IsNullOrWhiteSpace(search))
			{
				var searchLower = search.ToLower();
				query = query.Where(c =>
					c.TenDangNhap.ToLower().Contains(searchLower) ||
					c.HoTen.ToLower().Contains(searchLower) ||
					c.Email.ToLower().Contains(searchLower) ||
					c.SoDienThoai.Contains(search));
			}

			// Get customers with order statistics
			var customers = await query
				.OrderByDescending(c => c.CreatedDate)
				.Select(c => new
				{
					Customer = c,
					TotalOrders = c.HoaDons.Count,
					TotalSpent = c.HoaDons
						.SelectMany(o => o.ChiTietHoaDons)
						.Sum(od => (decimal?)(od.SoLuong * od.GiaBan)) ?? 0
				})
				.ToListAsync(cancellationToken);

			var model = customers.Select(x => new AdminCustomerListItemViewModel
			{
				Id = x.Customer.Id,
				Username = x.Customer.TenDangNhap,
				FullName = x.Customer.HoTen,
				Email = x.Customer.Email,
				Phone = x.Customer.SoDienThoai,
				Address = x.Customer.DiaChi,
				IsActive = x.Customer.IsActive,
				IsEmailVerified = x.Customer.IsEmailVerified,
				CreatedDate = x.Customer.CreatedDate,
				LastLoginDate = x.Customer.LastLoginDate,
				TotalOrders = x.TotalOrders,
				TotalSpent = x.TotalSpent,
				Status = x.Customer.Status
			}).ToList();

			// Get top spenders for statistics
			ViewBag.TopSpenders = await GetTopSpendersAsync(10, cancellationToken);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PermissionAuthorize("FPTDrink_Xoa", "Quản lý")]
		public async Task<IActionResult> ToggleLock(string id, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return Json(new { success = false, message = "ID khách hàng không hợp lệ." });
			}

			var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
			if (customer == null)
			{
				return Json(new { success = false, message = "Không tìm thấy khách hàng." });
			}

			customer.IsActive = !customer.IsActive;
			customer.ModifiedDate = DateTime.UtcNow;
			customer.Modifiedby = CurrentUserId;

			_customerRepository.Update(customer);
			await _customerRepository.SaveChangesAsync(cancellationToken);

			var action = customer.IsActive ? "mở khóa" : "khóa";
			return Json(new { success = true, message = $"Đã {action} tài khoản khách hàng thành công.", isActive = customer.IsActive });
		}

		private async Task<List<AdminCustomerTopSpenderViewModel>> GetTopSpendersAsync(int topCount, CancellationToken cancellationToken)
		{
			var topSpenders = await _customerRepository.Query()
				.Where(c => c.HoaDons.Any())
				.Select(c => new
				{
					Customer = c,
					TotalOrders = c.HoaDons.Count,
					TotalSpent = c.HoaDons
						.SelectMany(o => o.ChiTietHoaDons)
						.Sum(od => (decimal?)(od.SoLuong * od.GiaBan)) ?? 0,
					LastOrderDate = c.HoaDons.OrderByDescending(o => o.CreatedDate).Select(o => (DateTime?)o.CreatedDate).FirstOrDefault()
				})
				.OrderByDescending(x => x.TotalSpent)
				.Take(topCount)
				.ToListAsync(cancellationToken);

			return topSpenders.Select(x => new AdminCustomerTopSpenderViewModel
			{
				CustomerId = x.Customer.Id,
				CustomerName = x.Customer.HoTen,
				Email = x.Customer.Email,
				Phone = x.Customer.SoDienThoai,
				TotalOrders = x.TotalOrders,
				TotalSpent = x.TotalSpent,
				LastOrderDate = x.LastOrderDate
			}).ToList();
		}
	}
}

