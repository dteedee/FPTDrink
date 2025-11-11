using FPTDrink.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
		public IActionResult Index() => View();

		[HttpGet("tra-cuu-don-hang/ma")]
		public async Task<IActionResult> ByCode(string code)
		{
			if (string.IsNullOrWhiteSpace(code)) return RedirectToAction("Index");
			var order = await _api.GetAsync<object>($"api/public/Orders/{code}");
			ViewBag.By = "code";
			return View("Index", order);
		}

		[HttpGet("tra-cuu-don-hang/khach-hang")]
		public async Task<IActionResult> ByCustomer(string tenKhachHang, string cccd)
		{
			if (string.IsNullOrWhiteSpace(tenKhachHang) || string.IsNullOrWhiteSpace(cccd)) return RedirectToAction("Index");
			var orders = await _api.GetAsync<List<object>>($"api/public/Orders/by-customer?TenKhachHang={System.Net.WebUtility.UrlEncode(tenKhachHang)}&CCCD={System.Net.WebUtility.UrlEncode(cccd)}");
			ViewBag.By = "customer";
			return View("Index", orders);
		}
	}
}

