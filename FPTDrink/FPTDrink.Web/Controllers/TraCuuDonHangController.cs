using FPTDrink.Web.Helpers;
using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace FPTDrink.Web.Controllers
{
	public class TraCuuDonHangController : Controller
	{
		private readonly ApiClient _api;
		private readonly IHttpClientFactory _httpClientFactory;

		public TraCuuDonHangController(ApiClient api, IHttpClientFactory httpClientFactory)
		{
			_api = api;
			_httpClientFactory = httpClientFactory;
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
				var order = await GetOrderByCodeAsync(vm.OrderCode);
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
				var orders = await GetOrdersByCustomerAsync(vm.TenKhachHang, vm.SoDienThoai);
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

		private async Task<OrderDetailViewModel?> GetOrderByCodeAsync(string orderCode)
		{
			var httpClient = _httpClientFactory.CreateClient("FPTDrinkApi");
			var response = await httpClient.GetAsync($"api/public/Orders/{System.Net.WebUtility.UrlEncode(orderCode)}");
			if (!response.IsSuccessStatusCode) return null;

			var jsonContent = await response.Content.ReadAsStringAsync();
			var jsonDoc = JsonDocument.Parse(jsonContent);
			return OrderMappingHelper.MapFromJson(jsonDoc.RootElement);
		}

		private async Task<List<OrderDetailViewModel>> GetOrdersByCustomerAsync(string tenKhachHang, string soDienThoai)
		{
			var httpClient = _httpClientFactory.CreateClient("FPTDrinkApi");
			var url = $"api/public/Orders/by-customer?TenKhachHang={System.Net.WebUtility.UrlEncode(tenKhachHang)}&SoDienThoai={System.Net.WebUtility.UrlEncode(soDienThoai)}";
			var response = await httpClient.GetAsync(url);
			if (!response.IsSuccessStatusCode) return new List<OrderDetailViewModel>();

			var jsonContent = await response.Content.ReadAsStringAsync();
			var jsonDoc = JsonDocument.Parse(jsonContent);
			if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array) return new List<OrderDetailViewModel>();

			var orders = new List<OrderDetailViewModel>();
			foreach (var element in jsonDoc.RootElement.EnumerateArray())
			{
				var order = OrderMappingHelper.MapFromJson(element);
				if (order != null) orders.Add(order);
			}
			return orders;
		}
	}
}

