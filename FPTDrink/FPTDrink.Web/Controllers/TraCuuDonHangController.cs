using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPTDrink.Web.Controllers
{
	public class TraCuuDonHangController : Controller
	{
		private readonly ApiClient _api;
		public TraCuuDonHangController(ApiClient api)
		{
			_api = api;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string? code, string? tenKhachHang, string? soDienThoai)
		{
			var vm = new OrderLookupViewModel
			{
				OrderCode = code?.Trim(),
				TenKhachHang = tenKhachHang?.Trim(),
				SoDienThoai = soDienThoai?.Trim()
			};

			if (!string.IsNullOrWhiteSpace(vm.OrderCode))
			{
				vm.Searched = true;
				vm.SearchedByCode = true;
				var order = await _api.GetAsync<OrderDetailViewModel>($"api/public/Orders/{System.Net.WebUtility.UrlEncode(vm.OrderCode)}");
				if (order != null)
				{
					vm.Results.Add(order);
				}
				else
				{
					vm.Message = $"Không tìm thấy đơn hàng với mã {vm.OrderCode}.";
				}
			}
			else if (!string.IsNullOrWhiteSpace(vm.TenKhachHang) && !string.IsNullOrWhiteSpace(vm.SoDienThoai))
			{
				vm.Searched = true;
				vm.SearchedByCode = false;
				var orders = await _api.GetAsync<List<OrderDetailViewModel>>(
					$"api/public/Orders/by-customer?TenKhachHang={System.Net.WebUtility.UrlEncode(vm.TenKhachHang)}&SoDienThoai={System.Net.WebUtility.UrlEncode(vm.SoDienThoai)}");
				if (orders != null && orders.Count > 0)
				{
					vm.Results = orders
						.OrderByDescending(o => o.CreatedDate)
						.ToList();
				}
				else
				{
					vm.Message = $"Không tìm thấy đơn hàng cho khách hàng {vm.TenKhachHang}.";
				}
			}

			return View(vm);
		}
	}
}

