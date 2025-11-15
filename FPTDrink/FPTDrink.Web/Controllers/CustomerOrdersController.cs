using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Web.Extensions;
using FPTDrink.Web.Helpers;
using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;

namespace FPTDrink.Web.Controllers
{
	public class CustomerOrdersController : Controller
	{
		private readonly ApiClient _apiClient;
		private readonly IHttpClientFactory _httpClientFactory;

		public CustomerOrdersController(ApiClient apiClient, IHttpClientFactory httpClientFactory)
		{
			_apiClient = apiClient;
			_httpClientFactory = httpClientFactory;
		}

		[HttpGet]
		public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
		{
			if (!HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Login", "CustomerAuth", new { returnUrl = "/CustomerOrders" });
			}

			// Call API and deserialize to ViewModel directly (they have same structure)
			var orders = await _apiClient.GetAsync<System.Collections.Generic.List<CustomerOrderSummaryDto>>("api/public/customer/orders", cancellationToken);
			
			var model = new CustomerOrdersViewModel
			{
				Orders = orders ?? new System.Collections.Generic.List<CustomerOrderSummaryDto>()
			};

			return View(model);
		}

		[HttpGet("Details/{orderCode}")]
		public async Task<IActionResult> Details(string orderCode, CancellationToken cancellationToken = default)
		{
			if (!HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Login", "CustomerAuth");
			}

			// Get order from API using raw HTTP call to handle mapping
			var httpClient = _httpClientFactory.CreateClient("FPTDrinkApi");
			var token = HttpContext.Request.Cookies["customer_token"];
			if (!string.IsNullOrWhiteSpace(token))
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
			}

			var response = await httpClient.GetAsync($"api/public/Orders/{System.Net.WebUtility.UrlEncode(orderCode)}", cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
				return RedirectToAction(nameof(Index));
			}

			// Read as JSON and map manually
			var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var jsonDoc = JsonDocument.Parse(jsonContent);
			var order = OrderMappingHelper.MapFromJson(jsonDoc.RootElement);

			if (order == null)
			{
				TempData["ErrorMessage"] = "Không thể đọc thông tin đơn hàng.";
				return RedirectToAction(nameof(Index));
			}

			// Verify this order belongs to the current customer
			var customerId = HttpContext.GetCustomerId();
			if (!string.IsNullOrWhiteSpace(customerId) && !string.IsNullOrWhiteSpace(order.IdKhachHang) && order.IdKhachHang != customerId)
			{
				TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này.";
				return RedirectToAction(nameof(Index));
			}

			return View(order);
		}
	}
}

